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

using System;
using System.Drawing;
#if DEBUG
using System.Linq;
#endif
using PixelsOfDoom.Config;
using PixelsOfDoom.Generator;
using PixelsOfDoom.Map;
using PixelsOfDoom.Wad;

namespace PixelsOfDoom
{
    public sealed class PixelsOfDoomProgram : IDisposable
    {
        private static void Main(string[] args)
        {
            args = new string[] { "config.ini", "wolf3d_e1m1.png" };

            using (PixelsOfDoomProgram db = new PixelsOfDoomProgram(args)) { }
        }

        public PixelsOfDoomProgram(string[] args)
        {
#if DEBUG
            args = (from string a in args select @"..\..\media\" + a).ToArray();
#endif

            GeneratorConfig config = new GeneratorConfig(args[0]);

            using (WadFile wad = new WadFile())
            {
                using (MapGenerator generator = new MapGenerator())
                {
                    for (int i = 1; i < args.Length; i++)
                    {
                        if ((config.Doom1Format && (i > 9)) || (!config.Doom1Format && (i > 99))) // Too many maps
                            break;

                        string mapName = config.Doom1Format ? $"E{config.Episode.ToString("0")}M{i.ToString("0")}" : $"MAP{i.ToString("00")}";

                        using (Bitmap bitmap = (Bitmap)Image.FromFile(args[i]))
                        {
                            using (DoomMap map = generator.Generate("MAP01", config, bitmap))
                            {
                                map.AddToWad(wad);
                            }
                        }
                    }
                }

                wad.SaveToFile("test.wad");
            }

            config.Dispose();
        }

        public void Dispose()
        {

        }
    }
}
