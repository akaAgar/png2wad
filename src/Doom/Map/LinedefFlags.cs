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

namespace PNG2WAD.Doom.Map
{
    /// <summary>
    /// Special flags for a Doom map linedef.
    /// </summary>
    [Flags]
    public enum LinedefFlags
    {
        /// <summary>
        /// Cannot be crossed.
        /// </summary>
        Impassible = 1,
        /// <summary>
        /// Blocks monsters.
        /// </summary>
        BlocksMonsters = 2,
        /// <summary>
        /// Has two sides.
        /// </summary>
        TwoSided = 4,
        /// <summary>
        /// Draw lower texture from the bottom.
        /// </summary>
        UpperUnpegged = 8,
        /// <summary>
        /// Draw upper texture from the top.
        /// </summary>
        LowerUnpegged = 16,
        /// <summary>
        /// Show as a wall on the automap, used to hide secret passages.
        /// </summary>
        Secret = 32,
        /// <summary>
        /// Sound can only cross one linedef with this flag (not two).
        /// </summary>
        BlocksSound = 64,
        /// <summary>
        /// Does not appear on the automap.
        /// </summary>
        NotOnMap = 128,
        /// <summary>
        /// Drawn on the automap at the beginning of the level.
        /// </summary>
        AlreadyOnMap = 126
    }
}
