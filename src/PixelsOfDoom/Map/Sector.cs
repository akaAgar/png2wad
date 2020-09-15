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

using PixelsOfDoom.Wad;
using System;
using System.Collections.Generic;

namespace PixelsOfDoom.Map
{
    public struct Sector
    {
        public int FloorHeight { get; }
        public int CeilingHeight { get; }
        public string FloorTexture { get; }
        public string CeilingTexture { get; }
        public int LightLevel { get; }
        public int Special { get; }
        public int Tag { get; }

        public Sector(int floorHeight, int ceilingHeight, string floorTexture, string ceilingTexture, int lightLevel, int special, int tag)
        {
            FloorHeight = floorHeight;
            CeilingHeight = ceilingHeight;
            FloorTexture = floorTexture;
            CeilingTexture = ceilingTexture;
            LightLevel = lightLevel;
            Special = special;
            Tag = tag;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes((short)FloorHeight));
            bytes.AddRange(BitConverter.GetBytes((short)CeilingHeight));
            bytes.AddRange(WadFile.GetStringBytes(FloorTexture));
            bytes.AddRange(WadFile.GetStringBytes(CeilingTexture));
            bytes.AddRange(BitConverter.GetBytes((short)LightLevel));
            bytes.AddRange(BitConverter.GetBytes((short)Special));
            bytes.AddRange(BitConverter.GetBytes((short)Tag));
            return bytes.ToArray();
        }
    }
}
