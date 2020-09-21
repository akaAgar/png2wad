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

using INIPlusPlus;
using System;

namespace PixelsOfDoom.Config
{
    public struct PreferencesTheme
    {
        public static readonly int THEME_SECTORS_COUNT = Enum.GetValues(typeof(ThemeSector)).Length;
        public static readonly int THEME_TEXTURES_COUNT = Enum.GetValues(typeof(ThemeTexture)).Length;
        public static readonly int THEME_THINGS_COUNT = Enum.GetValues(typeof(ThemeThing)).Length;

        public int[][] Height { get; }
        public int[] LightLevel { get; }
        public int[] SectorSpecial { get; }
        public string[][] Textures { get; }
        public int[][] Things { get; }

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

            Things = new int[THEME_THINGS_COUNT][];
            for (i = 0; i < THEME_THINGS_COUNT; i++)
                Things[i] = ini.GetValueArray<int>(section, $"Things.{(ThemeThing)i}");
        }
    }
}
