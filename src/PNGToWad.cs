/*
==========================================================================
This file is part of PNG2WAD, a tool to create Doom maps from PNG files,
created by @akaAgar (https://github.com/akaAgar/png2wad)

PNG2WAD is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PNG2WAD is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with PNG2WAD. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using PNG2WAD.Config;
using PNG2WAD.Generator;
using PNG2WAD.Doom.Map;
using PNG2WAD.Doom.Wad;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PNG2WAD
{
    /// <summary>
    /// Main program. Parses command-line arguments and creates an instance of the generator.
    /// </summary>
    public sealed class PNGToWad : IDisposable
    {
        /// <summary>
        /// Static Main() method. Program entry point.
        /// </summary>
        /// <param name="args">Command line parameters</param>
        private static void Main(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
                args = new string[]
                {
                    @"..\Release\wolf_prison.png",
                    @"..\Release\cave.png",
                    @"..\Release\city_of_earth.png",
                    @"..\Release\city_of_hell.png"
                };
#endif

            using PNGToWad db = new(args);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Command line parameters</param>
        public PNGToWad(params string[] args)
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
            Preferences config = new(@"..\Release\Preferences.ini");
#else
            Preferences config = new("Preferences.ini");
#endif
            MapGenerator generator = new(config);
            WadFile wad = new();

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

                using Bitmap bitmap = new(mapBitmapFiles[i]);
                using DoomMap map = generator.Generate(mapName, bitmap);
                map.AddToWad(wad);
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
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
