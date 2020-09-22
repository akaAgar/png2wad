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

using PixelsOfDoom.Wad;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelsOfDoom.Map
{
    /// <summary>
    /// A Doom map.
    /// </summary>
    public sealed class DoomMap : IDisposable
    {
        /// <summary>
        /// Name of the map in the wad file (E1M1, MAP01, etc.)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Map linedefs.
        /// </summary>
        public List<Linedef> Linedefs { get; } = new List<Linedef>();
        
        /// <summary>
        /// Map sectors.
        /// </summary>
        public List<Sector> Sectors { get; } = new List<Sector>();
        
        /// <summary>
        /// Map sidedefs.
        /// </summary>
        public List<Sidedef> Sidedefs { get; } = new List<Sidedef>();
        
        /// <summary>
        /// Map things.
        /// </summary>
        public List<Thing> Things { get; } = new List<Thing>();
        
        /// <summary>
        /// Map vertices.
        /// </summary>
        public List<Vertex> Vertices { get; } = new List<Vertex>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the map in the wad file (E1M1, MAP01...)</param>
        public DoomMap(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Adds the map lumps to a Doom wad file.
        /// </summary>
        /// <param name="wad">The wad file to which the map should be added</param>
        public void AddToWad(WadFile wad)
        {
            wad.AddLump(Name, new byte[0]);
            wad.AddLump("LINEDEFS", Linedefs.SelectMany(x => x.ToBytes()).ToArray());
            wad.AddLump("SECTORS", Sectors.SelectMany(x => x.ToBytes()).ToArray());
            wad.AddLump("SIDEDEFS", Sidedefs.SelectMany(x => x.ToBytes()).ToArray());
            wad.AddLump("THINGS", Things.SelectMany(x => x.ToBytes()).ToArray());
            wad.AddLump("VERTEXES", Vertices.SelectMany(x => x.ToBytes()).ToArray());
        }

        /// <summary>
        /// Adds a new vertex to the map if no vertex with these coordinates exist, or return the index of the vertex with these coordinates.
        /// </summary>
        /// <param name="coordinates">Coordinates of the vertex</param>
        /// <returns>Index of the vertex with these coordinates</returns>
        public int AddVertex(Point coordinates)
        {
            for (int i = 0; i < Vertices.Count; i++)
                if ((Vertices[i].X == coordinates.X) && (Vertices[i].Y == coordinates.Y))
                    return i;

            Vertices.Add(new Vertex(coordinates));
            return Vertices.Count - 1;
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
