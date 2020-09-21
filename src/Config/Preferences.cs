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
using System.Collections.Generic;
using System.Drawing;

namespace PixelsOfDoom.Config
{
    public struct Preferences
    {
        private static readonly int DEFAULT_COLOR = Color.Black.ToArgb();

        public bool BuildNodes { get; }
        public bool Doom1Format { get; }
        public int Episode { get; }

        private readonly Dictionary<int, PreferencesTheme> Themes;

        public Preferences(string filePath)
        {
            using (INIFile ini = new INIFile(filePath))
            {
                // Load common settings
                BuildNodes = ini.GetValue("Options", "BuildNodes", false);
                Doom1Format = ini.GetValue("Options", "Doom1Format", false);
                Episode = Math.Max(1, Math.Min(9, ini.GetValue("Options", "Episode", 1)));

                Themes = new Dictionary<int, PreferencesTheme>
                {
                    { DEFAULT_COLOR, new PreferencesTheme(ini, "Theme.Default") }
                };

                foreach (string theme in ini.GetKeysInSection("Themes"))
                {
                    Color? c = Toolbox.GetColorFromString(ini.GetValue<string>("Themes", theme));
                    if (!c.HasValue) continue;
                    if (Themes.ContainsKey(c.Value.ToArgb())) continue;

                    Themes.Add(c.Value.ToArgb(), new PreferencesTheme(ini, $"Theme.{theme}"));
                }
            }
        }

        public PreferencesTheme GetTheme(Color color)
        {
            int colorInt = color.ToArgb();
            return Themes.ContainsKey(colorInt) ? Themes[colorInt] : Themes[DEFAULT_COLOR];
        }
    }
}
