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

namespace PixelsOfDoom.Config
{
    public struct PixelSetting
    {
        public int[] FloorHeight { get; }
        public int[] CeilingHeight { get; }
        public int[] SectorSpecial { get; }
        public int[] LineSpecial { get; }

        public string[] CeilingTexture { get; }
        public string[] FloorTexture { get; }
        public string[] WallTexture { get; }
    }
}
