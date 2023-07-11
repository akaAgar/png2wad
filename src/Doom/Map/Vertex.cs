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
using System.Drawing;

namespace PNG2WAD.Doom.Map
{
    /// <summary>
    /// A Doom map vertex.
    /// </summary>
    public readonly struct Vertex
    {
        /// <summary>
        /// X-coordinate of this vertex.
        /// </summary>
        public int X { get; }
        
        /// <summary>
        /// Y-coordinate of this vertex.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pt">Coordinates of the vertex.</param>
        public Vertex(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        /// <summary>
        /// Gets an array of bytes descripting this vertex to add to the VERTEXES (sic) lump.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes((short)X));
            bytes.AddRange(BitConverter.GetBytes((short)Y));
            return bytes.ToArray();
        }
    }
}
