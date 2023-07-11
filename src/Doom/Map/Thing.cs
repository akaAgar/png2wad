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

using System;
using System.Collections.Generic;

namespace PNG2WAD.Doom.Map
{
    /// <summary>
    /// A Doom map thing.
    /// </summary>
    public readonly struct Thing
    {
        /// <summary>
        /// X-coordinate of this thing.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y-coordinate of this thing.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Direction this thing is facing (east is 0, north is 90, west is 180, south is 270)
        /// </summary>
        public int Angle { get; }
        
        /// <summary>
        /// Type of thing.
        /// </summary>
        public int Type { get; }
        
        /// <summary>
        /// Thing options.
        /// </summary>
        public ThingOptions Options { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="x">X-coordinate of this thing</param>
        /// <param name="y">Y-coordinate of this thing</param>
        /// <param name="type">Type of thing</param>
        /// <param name="angle">Direction this thing is facing (east is 0, north is 90, west is 180, south is 270)</param>
        /// <param name="options">Thing options</param>
        public Thing(int x, int y, int type, int angle = 0, ThingOptions options = ThingOptions.AllSkills)
        {
            X = x;
            Y = y;
            Angle = angle;
            Type = type;
            Options = options;
        }

        /// <summary>
        /// Gets an array of bytes descripting this thing to add to the THINGS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes((short)X));
            bytes.AddRange(BitConverter.GetBytes((short)Y));
            bytes.AddRange(BitConverter.GetBytes((short)Angle));
            bytes.AddRange(BitConverter.GetBytes((short)Type));
            bytes.AddRange(BitConverter.GetBytes((short)Options));
            return bytes.ToArray();
        }
    }
}
