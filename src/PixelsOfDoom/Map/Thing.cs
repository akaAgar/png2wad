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
using System.Collections.Generic;

namespace PixelsOfDoom.Map
{
    public struct Thing
    {
        public int X { get; }
        public int Y { get; }
        public int Angle { get; }
        public int Type { get; }
        public ThingOptions Options { get; }

        public Thing(int x, int y, int angle, int type, ThingOptions options)
        {
            X = x;
            Y = y;
            Angle = angle;
            Type = type;
            Options = options;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes((short)X));
            bytes.AddRange(BitConverter.GetBytes((short)Y));
            bytes.AddRange(BitConverter.GetBytes((short)Angle));
            bytes.AddRange(BitConverter.GetBytes((short)Type));
            bytes.AddRange(BitConverter.GetBytes((short)Options));
            return bytes.ToArray();
        }
    }
}
