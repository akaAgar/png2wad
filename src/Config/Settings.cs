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
    public struct Settings
    {
        public static readonly int DEFAULT_COLOR = Color.Black.ToArgb();
        public static readonly int WALL_COLOR = Color.White.ToArgb();

        public bool BuildNodes { get; }
        public bool Doom1Format { get; }
        public int Episode { get; }

        private readonly Dictionary<int, SettingsPixel> Pixels;

        public Settings(string filePath)
        {
            using (INIFile ini = new INIFile(filePath))
            {
                // Load common settings
                BuildNodes = ini.GetValue("Options", "BuildNodes", true);
                Doom1Format = ini.GetValue("Options", "Doom1Format", false);
                Episode = Math.Max(1, Math.Min(9, ini.GetValue("Options", "Episode", 1)));

                Pixels = new Dictionary<int, SettingsPixel>
                {
                    { WALL_COLOR, new SettingsPixel(SettingsPixelType.Wall) } // Wall pixel color is constant
                };

                foreach (string s in ini.GetSections()) // Load each color
                {
                    Color? color = Toolbox.GetColorFromString(s);
                    if (!color.HasValue) continue;
                    int colorInt = color.Value.ToArgb();
                    if (Pixels.ContainsKey(colorInt)) continue;

                    Pixels.Add(colorInt, new SettingsPixel(ini, s));
                }

                if (!Pixels.ContainsKey(DEFAULT_COLOR)) // If default color is not set, create a default SettingPixel
                    Pixels.Add(DEFAULT_COLOR, new SettingsPixel(SettingsPixelType.Room));
            }
        }

        /// <summary>
        /// Returns SettingsPixel info for a color.
        /// </summary>
        /// <param name="color">The pixel color</param>
        /// <returns>A SettingsPixel struct</returns>
        public SettingsPixel this[Color color]
        {
            get { return this[color.ToArgb()]; }
        }


        /// <summary>
        /// Returns SettingsPixel info for a color.
        /// </summary>
        /// <param name="colorARGB">The pixel color as an ARGB Int32</param>
        /// <returns>A SettingsPixel struct</returns>
        public SettingsPixel this[int colorARGB]
        {
            get
            {
                if (colorARGB == WALL_COLOR) { } // TODO

                return Pixels.ContainsKey(colorARGB) ? Pixels[colorARGB] : Pixels[DEFAULT_COLOR];
            }
        }
    }
}
