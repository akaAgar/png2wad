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

using PixelsOfDoom.Config;
using System;

namespace PixelsOfDoom.Generator
{
    public struct SubTile : IEquatable<SubTile>
    {
        public int Color { get; }
        public SettingsPixelType TileType { get; }

        public SubTile(int color, SettingsPixelType tileType = SettingsPixelType.Room)
        {
            Color = color;
            TileType = tileType;
        }

        public bool Equals(SubTile other)
        {
            return (Color == other.Color) && (TileType == other.TileType);
        }
    }
}
