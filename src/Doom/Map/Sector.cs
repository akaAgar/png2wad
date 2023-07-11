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

using PNG2WAD.Doom.Wad;
using System;
using System.Collections.Generic;

namespace PNG2WAD.Doom.Map
{
    /// <summary>
    /// A Doom map sector.
    /// </summary>
    public readonly struct Sector
    {
        /// <summary>
        /// Height of this sector's floor.
        /// </summary>
        public int FloorHeight { get; }

        /// <summary>
        /// Height of this sector's ceiling.
        /// </summary>
        public int CeilingHeight { get; }

        /// <summary>
        /// Texture drawn on this sector's floor.
        /// </summary>
        public string FloorTexture { get; }

        /// <summary>
        /// Texture drawn on this sector's ceiling.
        /// </summary>
        public string CeilingTexture { get; }
        
        /// <summary>
        /// Light level in this sector (0-255).
        /// </summary>
        public int LightLevel { get; }
        
        /// <summary>
        /// Sector special type.
        /// </summary>
        public int Special { get; }
        
        /// <summary>
        /// Sector special tag.
        /// </summary>
        public int Tag { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="floorHeight"></param>
        /// <param name="ceilingHeight"></param>
        /// <param name="floorTexture"></param>
        /// <param name="ceilingTexture"></param>
        /// <param name="lightLevel"></param>
        /// <param name="special"></param>
        /// <param name="tag"></param>
        public Sector(int floorHeight, int ceilingHeight, string floorTexture, string ceilingTexture, int lightLevel, int special = 0, int tag = 0)
        {
            FloorHeight = Math.Max(-32768, Math.Min(32767, floorHeight));
            CeilingHeight = Math.Max(FloorHeight, Math.Min(32767, ceilingHeight));
            FloorTexture = floorTexture;
            CeilingTexture = ceilingTexture;
            LightLevel = Math.Max(0, Math.Min(255, lightLevel));
            Special = Math.Max(0, Math.Min(32767, special));
            Tag = Math.Max(0, Math.Min(32767, tag));
        }

        /// <summary>
        /// Gets an array of bytes descripting this sector to add to the SECTORS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes((short)FloorHeight));
            bytes.AddRange(BitConverter.GetBytes((short)CeilingHeight));
            bytes.AddRange(WadFile.GetBytesFromString(FloorTexture));
            bytes.AddRange(WadFile.GetBytesFromString(CeilingTexture));
            bytes.AddRange(BitConverter.GetBytes((short)LightLevel));
            bytes.AddRange(BitConverter.GetBytes((short)Special));
            bytes.AddRange(BitConverter.GetBytes((short)Tag));
            return bytes.ToArray();
        }
    }
}
