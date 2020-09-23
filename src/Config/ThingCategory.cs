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

namespace PNG2WAD.Config
{
    /// <summary>
    /// A category of thing for the thing generator.
    /// </summary>
    public enum ThingCategory
    {
        /// <summary>
        /// Large ammo pickup (box of bullets, box of shells...)
        /// </summary>
        AmmoLarge,
        /// <summary>
        /// Small ammo pickup (clip, shells...)
        /// </summary>
        AmmoSmall,
        /// <summary>
        /// Armor pickup
        /// </summary>
        Armor,
        /// <summary>
        /// Health pickup
        /// </summary>
        Health,
        /// <summary>
        /// Easy monsters
        /// </summary>
        MonstersEasy,
        /// <summary>
        /// Average difficulty monsters
        /// </summary>
        MonstersAverage,
        /// <summary>
        /// Hard monsters
        /// </summary>
        MonstersHard,
        /// <summary>
        /// Very hard monsters
        /// </summary>
        MonstersVeryHard,
        /// <summary>
        /// Power-ups (invisibility, invulnerability...)
        /// </summary>
        PowerUps,
        /// <summary>
        /// Powerful weapons
        /// </summary>
        WeaponsHigh,
        /// <summary>
        /// Weak weapons
        /// </summary>
        WeaponsLow,
    }
}
