/*
==========================================================================
This file is part of Pixels of Doom, a tool to create Doom maps from PNG files
by @akaAgar (https://github.com/akaAgar/one-bit-of-engine)
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
            using (PixelsOfDoomProgram db = new PixelsOfDoomProgram(args)) { }
        }

        public PixelsOfDoomProgram(string[] args)
        {
            WadFile wad = new WadFile();

            using (MapGenerator generator = new MapGenerator())
            {
                using (Bitmap bitmap = (Bitmap)Image.FromFile(@"..\..\media\wolf3d_e1m1.png"))
                {
                    using (DoomMap map = generator.Generate("MAP01", null, bitmap))
                    {
                        map.AddToWad(wad);
                    }
                }
            }

            wad.SaveToFile("test.wad");
        }

        public void Dispose()
        {

        }
    }
}
