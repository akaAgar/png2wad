﻿/*
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

using PNG2WAD.INI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PNG2WAD.Config
{

    /// <summary>
    /// Stores the preferences loaded from Preferences.ini.
    /// </summary>
    public readonly struct Preferences
    {
        /// <summary>
        /// Number of things categories.
        /// </summary>
        public static readonly int THINGS_CATEGORY_COUNT = Enum.GetValues(typeof(ThingCategory)).Length;

        /// <summary>
        /// Color to used for the default theme.
        /// </summary>
        public static readonly int DEFAULT_THEME_COLOR = Color.White.ToArgb();

        /// <summary>
        /// Should nodes be built using bsp-w32 once the map(s) are generated?
        /// </summary>
        public bool BuildNodes { get; }

        /// <summary>
        /// Should map names be generated in the Doom 1 (ExMy) format?
        /// </summary>
        public bool Doom1Format { get; }
        
        /// <summary>
        /// What episode do the maps belong to? Only used when Doom1Format is true.
        /// </summary>
        public int Episode { get; }

        /// <summary>
        /// Should entrances and exits be generated?
        /// </summary>
        public bool GenerateEntranceAndExit { get; }
        
        /// <summary>
        /// Should things (monsters, items...) be generated?
        /// </summary>
        public bool GenerateThings { get; }

        /// <summary>
        /// The map themes.
        /// </summary>
        private readonly Dictionary<int, PreferencesTheme> Themes;
        
        /// <summary>
        /// Array of thing types to pick from for each thing category.
        /// </summary>
        public int[][] ThingsTypes { get; }

                /// <summary>
        /// Min/Max amount of things to spawn for each thing category.
        /// </summary>
        public int[][] ThingsCount { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path to the Preferences.ini file.</param>
        public Preferences(string filePath)
        {
            using INIFile ini = new(filePath);

            // Load common settings
            BuildNodes = ini.GetValue("Options", "BuildNodes", false);
            Doom1Format = ini.GetValue("Options", "Doom1Format", false);
            Episode = Math.Max(1, Math.Min(9, ini.GetValue("Options", "Episode", 1)));
            GenerateEntranceAndExit = ini.GetValue("Options", "GenerateEntranceAndExit", true);
            GenerateThings = ini.GetValue("Options", "GenerateThings", true);

            // Load things
            ThingsTypes = new int[THINGS_CATEGORY_COUNT][];
            ThingsCount = new int[THINGS_CATEGORY_COUNT][];
            for (int i = 0; i < THINGS_CATEGORY_COUNT; i++)
            {
                ThingsTypes[i] = ini.GetValueArray<int>("Things", $"Types.{(ThingCategory)i}");
                ThingsCount[i] = ini.GetValueArray<int>("Things", $"Count.{(ThingCategory)i}");
                Array.Resize(ref ThingsCount[i], 2);
                ThingsCount[i] = new int[] { Math.Min(ThingsCount[i][0], ThingsCount[i][1]), Math.Max(ThingsCount[i][0], ThingsCount[i][1]) };
            }

            // Load themes. Default theme is loaded first so it can't be overriden.
            Themes = new Dictionary<int, PreferencesTheme>
                {
                    { DEFAULT_THEME_COLOR, new PreferencesTheme(ini, "Theme.Default") }
                };

            foreach (string theme in ini.GetAllKeysInSection("Themes"))
            {
                Color? c = Toolbox.GetColorFromString(ini.GetValue<string>("Themes", theme));
                if (!c.HasValue) continue;
                if (Themes.ContainsKey(c.Value.ToArgb())) continue;

                Themes.Add(c.Value.ToArgb(), new PreferencesTheme(ini, $"Theme.{theme}"));
            }
        }

        /// <summary>
        /// Gets a theme from the RGB color of the top-left pixel of a map PNG.
        /// If no theme use this color, returns the default theme.
        /// </summary>
        /// <param name="color">Color of the "theme pixel"</param>
        /// <returns>A theme</returns>
        public PreferencesTheme GetTheme(Color color)
        {
            int colorInt = color.ToArgb();
            return Themes.ContainsKey(colorInt) ? Themes[colorInt] : Themes[DEFAULT_THEME_COLOR];
        }
    }
}
