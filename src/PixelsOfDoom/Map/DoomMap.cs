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
    public sealed class DoomMap : IDisposable
    {
        public List<Linedef> Linedefs { get; } = new List<Linedef>();
        public List<Sector> Sectors { get; } = new List<Sector>();
        public List<Sidedef> Sidedefs { get; } = new List<Sidedef>();
        public List<Thing> Things { get; } = new List<Thing>();
        public List<Vertex> Vertices { get; } = new List<Vertex>();

        public DoomMap()
        {

        }

        public void Dispose()
        {

        }
    }
}
