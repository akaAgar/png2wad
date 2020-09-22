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
    /// A Doom map sidedef.
    /// </summary>
    public struct Sidedef
    {
        /// <summary>
        /// Texture X-Offset.
        /// </summary>
		public int XOffset { get; }
		
        /// <summary>
        /// Texture Y-Offset.
        /// </summary>
        public int YOffset { get; }

        /// <summary>
        /// Upper (above neighboring sector) texture.
        /// </summary>
        public string UpperTexture { get; }

        /// <summary>
        /// Lower (below neighboring sector) texture.
        /// </summary>
        public string LowerTexture { get; }

        /// <summary>
        /// Middle (over neighboring sector, or wall) texture.
        /// </summary>
        public string MiddleTexture { get; }
		
        /// <summary>
        /// Sector this sidedef faces.
        /// </summary>
        public int Sector { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="upperTexture">Upper (above neighboring sector) texture</param>
        /// <param name="lowerTexture">Lower (below neighboring sector) texture</param>
        /// <param name="middleTexture">Middle (over neighboring sector, or wall) texture</param>
        /// <param name="sector">Sector this sidedef faces</param>
        public Sidedef(string upperTexture, string lowerTexture, string middleTexture, int sector)
        {
            XOffset = 0;
            YOffset = 0;
            UpperTexture = upperTexture;
            LowerTexture = lowerTexture;
            MiddleTexture = middleTexture;
            Sector = sector;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sectorID">Sector this sidedef faces</param>
        /// <param name="sector">Info about the sector this sidedef faces</param>
        /// <param name="neighborSector">Info about the sector this sidedef's opposing sector</param>
        public Sidedef(int sectorID, SectorInfo sector, SectorInfo neighborSector)
        {
            XOffset = 0; YOffset = 0;
            MiddleTexture = "-";
            
            if (neighborSector.Type == TileType.Door)
            {
                UpperTexture = (neighborSector.CeilingHeight < sector.CeilingHeight) ? neighborSector.WallTextureUpper : "-";
                LowerTexture = (neighborSector.FloorHeight > sector.FloorHeight) ? neighborSector.WallTextureLower : "-";
            }
            else
            {
                UpperTexture = (neighborSector.CeilingHeight < sector.CeilingHeight) ? sector.WallTexture : "-";
                LowerTexture = (neighborSector.FloorHeight > sector.FloorHeight) ? sector.WallTexture : "-";
            }

            Sector = sectorID;
        }

        /// <summary>
        /// Gets an array of bytes descripting this sidedef to add to the SIDEDEFS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
		{
			List<byte> bytes = new List<byte>();
			bytes.AddRange(BitConverter.GetBytes((short)XOffset));
			bytes.AddRange(BitConverter.GetBytes((short)YOffset));
			bytes.AddRange(WadFile.GetStringBytes(UpperTexture));
			bytes.AddRange(WadFile.GetStringBytes(LowerTexture));
			bytes.AddRange(WadFile.GetStringBytes(MiddleTexture));
			bytes.AddRange(BitConverter.GetBytes((short)Sector));
			return bytes.ToArray();
		}
	}
}
