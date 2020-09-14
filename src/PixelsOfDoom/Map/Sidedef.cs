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
    public struct Sidedef
    {
		public int XOffset { get; }
		public int YOffset { get; }
		public string UpperTexture { get; }
		public string LowerTexture { get; }
		public string MiddleTexture { get; }
		public int Sector { get; }

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
