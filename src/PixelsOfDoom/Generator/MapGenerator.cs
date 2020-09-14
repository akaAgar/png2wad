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

using PixelsOfDoom.Config;
using PixelsOfDoom.Map;
using System;
using System.Drawing;

namespace PixelsOfDoom.Generator
{
    public sealed class MapGenerator : IDisposable
    {
        public MapGenerator()
        {

        }

        public DoomMap Generate(string name, GeneratorConfig config, Bitmap bitmap)
        {
            DoomMap map = new DoomMap(name);

            int x, y;
            Color c;
            
            for (x = 0; x < bitmap.Width; x++)
                for (y = 0; y < bitmap.Height; y++)
                {
                    c = bitmap.GetPixel(x, y);
                    if (c == Color.White) continue;
                    map.Vertices.Add(new Vertex(x * 64, y * 64));
                }

            return map;
        }

        public void Dispose()
        {

        }
    }
}
