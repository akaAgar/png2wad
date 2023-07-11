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
    /// A Doom map linedef.
    /// </summary>
    public readonly struct Linedef
    {
        /// <summary>
        /// Index of this linedef's first vertex.
        /// </summary>
        public int Vertex1 { get; }

        /// <summary>
        /// Index of this linedef's second vertex.
        /// </summary>
        public int Vertex2 { get; }
        
        /// <summary>
        /// Linedef flags.
        /// </summary>
        public LinedefFlags Flags { get; }
        
        /// <summary>
        /// Linedef special type.
        /// </summary>
        public int Type { get; }
        
        /// <summary>
        /// Linedef action tag.
        /// </summary>
        public int Tag { get; }
        
        /// <summary>
        /// Index of this sidedef's right sidedef.
        /// </summary>
        public int SidedefRight { get; }

        /// <summary>
        /// Index of this sidedef's left sidedef.
        /// </summary>
        public int SidedefLeft { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="vertex1">Index of this linedef's first vertex</param>
        /// <param name="vertex2">Index of this linedef's second vertex</param>
        /// <param name="flags">Linedef flags</param>
        /// <param name="type">Linedef special type</param>
        /// <param name="tag">Linedef action tag</param>
        /// <param name="sidedefLeft">Index of this sidedef's right sidedef</param>
        /// <param name="sidedefRight">Index of this sidedef's left sidedef</param>
        public Linedef(int vertex1, int vertex2, LinedefFlags flags, int type, int tag, int sidedefLeft, int sidedefRight)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Flags = flags;
            Type = type;
            Tag = tag;
            SidedefRight = sidedefRight;
            SidedefLeft = sidedefLeft;
        }

        /// <summary>
        /// Gets an array of bytes descripting this linedef to add to the LINEDEFS lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes((short)Vertex1));
            bytes.AddRange(BitConverter.GetBytes((short)Vertex2));
            bytes.AddRange(BitConverter.GetBytes((short)Flags));
            bytes.AddRange(BitConverter.GetBytes((short)Type));
            bytes.AddRange(BitConverter.GetBytes((short)Tag));
            bytes.AddRange(BitConverter.GetBytes((short)SidedefRight));
            bytes.AddRange(BitConverter.GetBytes((short)SidedefLeft));
            return bytes.ToArray();
        }
    }
}
