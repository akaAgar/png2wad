/*
==========================================================================
This file is part of Pixels of Doom, a tool to create Doom maps from PNG files
by @akaAgar (https://github.com/akaAgar/pixels-of-doom)
Pixels of Doom is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
Pixels of Doom is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with Pixels of Doom. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using PixelsOfDoom.Config;
using PixelsOfDoom.Generator;
using PixelsOfDoom.Map;
using PixelsOfDoom.Wad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace PixelsOfDoom
{
    /// <summary>
    /// Main program. Parses command-line arguments and creates an instance of the generator.
    /// </summary>
    public sealed class PixelsOfDoomProgram : IDisposable
    {
        /// <summary>
        /// Static Main() method. Program entry point.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        private static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "output.wad", @"..\Release\config.ini", @"..\Release\wolf3d_e1m1.png" };
#endif
            
            using (PixelsOfDoomProgram db = new PixelsOfDoomProgram(args)) { }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public PixelsOfDoomProgram(string[] args)
        {
            if (!ParseArguments(args, out string wadFile, out string configFile, out string[] mapBitmapFiles))
                return;

            Settings config = new Settings(configFile);
            MapGenerator generator = new MapGenerator(config);
            WadFile wad = new WadFile();

            for (int i = 0; i < mapBitmapFiles.Length; i++)
            {
                int mapNumber = i + 1;

                if ((config.Doom1Format && (mapNumber > 9)) || (!config.Doom1Format && (mapNumber > 99))) // Too many maps, stop here
                    break;

                try
                {
                    string mapName = config.Doom1Format ? $"E{config.Episode:0}M{mapNumber:0}" : $"MAP{mapNumber:00}";

                    using (Bitmap bitmap = (Bitmap)Image.FromFile(mapBitmapFiles[i]))
                    {
                        using (DoomMap map = generator.Generate(mapName, bitmap))
                        {
                            map.AddToWad(wad);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            wad.SaveToFile(wadFile);
            wad.Dispose();
            generator.Dispose();

            if (config.BuildNodes)
            {
                Process bspProcess;

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        Console.WriteLine("Building nodes with bsp-w32.exe...");
#if DEBUG
                        bspProcess = Process.Start(@"..\Release\bsp-w32.exe", $"\"{wadFile}\" -o \"{wadFile}\"");
#else
                        bspProcess = Process.Start("bsp-w32.exe", $"\"{wadFile}\" -o \"{wadFile}\"");
#endif
                        bspProcess.WaitForExit();
                        if (bspProcess.ExitCode != 0)
                            Console.WriteLine("Failed to build nodes!");
                        break;
                }
            }
        }

        /// <summary>
        /// Parses command-lines arguments to search for path for the various files.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        /// <param name="wadFile">Path to the output .wad file</param>
        /// <param name="configFile">Path to the config .ini file</param>
        /// <param name="mapBitmapFiles">Array of paths to the .bmp/.png images to create levels from</param>
        /// <returns>True if all required paths are present, false if they are not</returns>
        private bool ParseArguments(string[] args, out string wadFile, out string configFile, out string[] mapBitmapFiles)
        {
            wadFile = null;
            configFile = null;
            mapBitmapFiles = new string[0];

            if ((args.Length == 0) || (args == null))
            {
                PrintMissingParameterMessage();
                return false;
            }

            List<string> mapBitmapFilesList = new List<string>();

            foreach (string a in args)
            {
                switch (Path.GetExtension(a).ToLowerInvariant())
                {
                    case ".bmp":
                    case ".png":
                        if (!File.Exists(a)) break;
                        mapBitmapFilesList.Add(a);
                        break;

                    case ".ini":
                        if (!File.Exists(a)) break;
                        configFile = a;
                        break;

                    case ".wad":
                        wadFile = a;
                        break;
                }
            }

            mapBitmapFiles = mapBitmapFilesList.ToArray();

            if ((mapBitmapFiles.Length > 0) && (configFile != null) && (wadFile != null))
                return true;

            PrintMissingParameterMessage();
            return false;
        }

        /// <summary>
        /// Print error message when a parameter is missing.
        /// </summary>
        private void PrintMissingParameterMessage()
        {
            Console.WriteLine("Missing parameters.");
            Console.WriteLine("Parameters must include, in any order: path to an output .wad file, path to a .ini config file, paths to any number of .bmp or .png images");
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
