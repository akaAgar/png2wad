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

using PixelsOfDoom.Generator;
using PixelsOfDoom.Wad;
using System;
using System.Collections.Generic;

namespace PixelsOfDoom.Map
{
    /// <summary>
    /// A Doom map sector.
    /// </summary>
    public struct Sector
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
        /// <param name="info">Sector info from which to create this sector.</param>
        public Sector(SectorInfo info)
        {
            FloorHeight = info.FloorHeight;
            CeilingHeight = info.CeilingHeight;
            FloorTexture = info.FloorTexture;
            CeilingTexture = info.CeilingTexture;
            LightLevel = info.LightLevel;
            Special = info.SectorSpecial;
            Tag = 0;
        }

        /// <summary>
        /// Gets an array of bytes descripting this sector to add to the SECTORS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
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
