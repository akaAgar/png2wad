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
    /// A Doom map sidedef.
    /// </summary>
    public readonly struct Sidedef
    {
        /// <summary>
        /// Texture X-offset.
        /// </summary>
		public int XOffset { get; }
		
        /// <summary>
        /// Texture Y-offset.
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
        /// <param name="xOffset">Texture X-offset</param>
        /// <param name="yOffset">Texture Y-offset</param>
        /// <param name="upperTexture">Upper (above neighboring sector) texture</param>
        /// <param name="lowerTexture">Lower (below neighboring sector) texture</param>
        /// <param name="middleTexture">Middle (over neighboring sector, or wall) texture</param>
        /// <param name="sector">Sector this sidedef faces</param>
        public Sidedef(int xOffset, int yOffset, string upperTexture, string lowerTexture, string middleTexture, int sector)
        {
            XOffset = xOffset;
            YOffset = yOffset;
            UpperTexture = upperTexture;
            LowerTexture = lowerTexture;
            MiddleTexture = middleTexture;
            Sector = sector;
        }

        /// <summary>
        /// Gets an array of bytes descripting this sidedef to add to the SIDEDEFS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
		{
			List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes((short)XOffset));
			bytes.AddRange(BitConverter.GetBytes((short)YOffset));
			bytes.AddRange(WadFile.GetBytesFromString(UpperTexture));
			bytes.AddRange(WadFile.GetBytesFromString(LowerTexture));
			bytes.AddRange(WadFile.GetBytesFromString(MiddleTexture));
			bytes.AddRange(BitConverter.GetBytes((short)Sector));
			return bytes.ToArray();
		}
	}
}
