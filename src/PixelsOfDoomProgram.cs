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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

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
            if (args.Length == 0)
                args = new string[] { @"..\Release\wolf3d.png", @"..\Release\cave.png", @"..\Release\city_of_hell.png" };
#endif

            using (PixelsOfDoomProgram db = new PixelsOfDoomProgram(args)) { }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public PixelsOfDoomProgram(params string[] args)
        {
            string[] mapBitmapFiles = (from string file in args where File.Exists(file) && Path.GetExtension(file).ToLowerInvariant() == ".png" select file).ToArray();

            if (mapBitmapFiles.Length == 0)
            {
                Console.WriteLine("Missing parameters or no valid PNG file in the parameters.");
                Console.WriteLine("Syntax is: PixelsOfDoom.exe SomeImage.png [SomeOtherImage.png] [YetAnotherImage.png]...");
                Console.ReadKey();
                return;
            }

            string wadFile = Path.GetFileNameWithoutExtension(mapBitmapFiles[0]) + ".wad"; // Output file is the name of the first file with a WAD extension.

#if DEBUG
            Preferences config = new Preferences(@"..\Release\Preferences.ini");
#else
            Preferences config = new Preferences("Preferences.ini");
#endif
            MapGenerator generator = new MapGenerator(config);
            WadFile wad = new WadFile();

            for (int i = 0; i < mapBitmapFiles.Length; i++)
            {
                int mapNumber = i + 1;

                if ((config.Doom1Format && (mapNumber > 9)) || (!config.Doom1Format && (mapNumber > 99))) // Too many maps, stop here
                    break;

#if !DEBUG
                try
                {
#endif
                    string mapName = config.Doom1Format ? $"E{config.Episode:0}M{mapNumber:0}" : $"MAP{mapNumber:00}";

                    using (Bitmap bitmap = (Bitmap)Image.FromFile(mapBitmapFiles[i]))
                    {
                        using (DoomMap map = generator.Generate(mapName, bitmap))
                        {
                            map.AddToWad(wad);
                        }
                    }
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine();
                    continue;
                }
#endif
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

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Press any key...");
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
