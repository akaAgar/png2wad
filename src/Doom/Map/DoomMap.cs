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
using System.Drawing;
using System.Linq;

namespace PNG2WAD.Doom.Map
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
        /// Constructor. Creates a new, empty map.
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
            wad.AddLump(Name, Array.Empty<byte>());
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
        /// Returns the coordinates the the edges of the map.
        /// </summary>
        /// <param name="minX">Minimum X coordinate of any vertex</param>
        /// <param name="minY">Minimum Y coordinate of any vertex</param>
        /// <param name="maxX">Maximum X coordinate of any vertex</param>
        /// <param name="maxY">Maximum Y coordinate of any vertex</param>
        public void GetMapBoundaries(out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = 0; minY = 0; maxX = 0; maxY = 0;
            if (Vertices.Count == 0) return;

            minX = Vertices[0].X; maxX = Vertices[0].X;
            minY = Vertices[0].Y; maxY = Vertices[0].Y;

            foreach (Vertex v in Vertices)
            {
                if (v.X < minX) minX = v.X;
                if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
            }
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
