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

using INIPlusPlus;
using System;

namespace PNG2WAD.Config
{
    /// <summary>
    /// Stores information for a theme in the preferences
    /// </summary>
    public struct PreferencesTheme
    {
        /// <summary>
        /// Total number of values in the ThemeSector enum.
        /// </summary>
        public static readonly int THEME_SECTORS_COUNT = Enum.GetValues(typeof(ThemeSector)).Length;

        /// <summary>
        /// Total number of values in the ThemeTexture enum.
        /// </summary>
        public static readonly int THEME_TEXTURES_COUNT = Enum.GetValues(typeof(ThemeTexture)).Length;

        /// <summary>
        /// Floor/Ceiling heights. Two int32s (floor, ceiling) per ThemeSector.
        /// </summary>
        public int[][] Height { get; }

        /// <summary>
        /// Light levels. One int32 per ThemeSector.
        /// </summary>
        public int[] LightLevel { get; }

        /// <summary>
        /// Sector special types. One int32 per ThemeSector.
        /// </summary>
        public int[] SectorSpecial { get; }

        /// <summary>
        /// Textures. A string array per ThemeTexture.
        /// </summary>
        public string[][] Textures { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ini">Ini file to load from</param>
        /// <param name="section">Theme section in the ini file</param>
        public PreferencesTheme(INIFile ini, string section)
        {
            int i;

            Height = new int[THEME_SECTORS_COUNT][];
            LightLevel = new int[THEME_SECTORS_COUNT];
            SectorSpecial = new int[THEME_SECTORS_COUNT];

            for (i = 0; i < THEME_SECTORS_COUNT; i++)
            {
                Height[i] = ini.GetValueArray<int>(section, $"Height.{(ThemeSector)i}");
                if (Height[i].Length == 0) Height[i] = new int[] { 0, 64 };
                else if (Height[i].Length == 1) Height[i] = new int[] { 0, Height[i][1] };
                Height[i] = new int[] { Math.Min(Height[i][0], Height[i][1]), Math.Max(Height[i][0], Height[i][1]) };

                LightLevel[i] = ini.GetValue(section, $"LightLevel.{(ThemeSector)i}", -1);
                LightLevel[i] = Toolbox.Clamp(LightLevel[i], 0, 255);

                SectorSpecial[i] = ini.GetValue(section, $"SectorSpecial.{(ThemeSector)i}", 0);
                SectorSpecial[i] = Math.Max(0, SectorSpecial[i]);
            }

            Textures = new string[THEME_TEXTURES_COUNT][];
            for (i = 0; i < THEME_TEXTURES_COUNT; i++)
                Textures[i] = ini.GetValueArray<string>(section, $"Textures.{(ThemeTexture)i}");
        }
    }
}
