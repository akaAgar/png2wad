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

using System;
using System.Drawing;

namespace PixelsOfDoom
{
    public static class Toolbox
    {
        /// <summary>
        /// (Private) Random number generator.
        /// </summary>
        private static readonly Random RNG = new Random();

        public static bool IsSameRGB(this Color color, Color other)
        {
            return (color.R == other.R) && (color.G == other.G) && (color.B == other.B);
        }

        public static Point Add(this Point point, Point other)
        {
            return new Point(point.X + other.X, point.Y + other.Y);
        }

        public static Point Mult(this Point point, Point mutiplier)
        {
            return new Point(point.X * mutiplier.X, point.Y * mutiplier.Y);
        }

        public static Color? GetColorFromString(string colorString)
        {
            int r, g, b;

            try
            {
                colorString = colorString.Trim();


                if (colorString.StartsWith("#")) // HTML format
                {
                    colorString = colorString.Substring(1);

                    if (colorString.Length == 6)
                    {
                        r = Convert.ToInt32(colorString.Substring(0, 2), 16);
                        g = Convert.ToInt32(colorString.Substring(2, 2), 16);
                        b = Convert.ToInt32(colorString.Substring(4, 2), 16);

                        return Color.FromArgb(255, r, g, b);
                    }
                    else if (colorString.Length == 3)
                    {
                        string xR = colorString.Substring(0, 1); xR += xR;
                        string xG = colorString.Substring(1, 1); xG += xG;
                        string xB = colorString.Substring(2, 1); xB += xB;

                        r = Convert.ToInt32(xR, 16);
                        g = Convert.ToInt32(xG, 16);
                        b = Convert.ToInt32(xB, 16);

                        return Color.FromArgb(255, r, g, b);
                    }
                }
                else if (colorString.Contains(",")) // R,G,B format
                {
                    string[] rgbStrings = colorString.Split(',');

                    if (rgbStrings.Length >= 3)
                    {
                        r = Convert.ToInt32(rgbStrings[0].Trim());
                        g = Convert.ToInt32(rgbStrings[1].Trim());
                        b = Convert.ToInt32(rgbStrings[2].Trim());

                        return Color.FromArgb(255, r, g, b);
                    }
                }
                else if (Enum.TryParse(colorString, true, out KnownColor knownColor))
                    return Color.FromKnownColor(knownColor);
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Returns a random integer between 0 (inclusive) and maxValue (exclusive).
        /// </summary>
        /// <param name="maxValue">The maximum possible value, EXCLUSIVE</param>
        /// <returns>An integer</returns>
        public static int RandomInt(int maxValue)
        {
            return RNG.Next(maxValue);
        }

        /// <summary>
        /// Returns a random integer between minValue (inclusive) and maxValue (exclusive).
        /// </summary>
        /// <param name="minValue">The maximum possible value, INCLUSIVE</param>
        /// <param name="maxValue">The maximum possible value, EXCLUSIVE</param>
        /// <returns>An integer</returns>
        public static int RandomInt(int minValue, int maxValue)
        {
            return RNG.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random element from a array.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">The array</param>
        /// <returns>An element from the array</returns>
        public static T RandomFromArray<T>(T[] array)
        {
            if ((array == null) || (array.Length == 0)) return default;
            return array[RNG.Next(array.Length)];
        }

        /// <summary>
        /// Clamps the value between minValue and maxValue.
        /// </summary>
        /// <param name="value">A value</param>
        /// <param name="minValue">Minimum acceptable value</param>
        /// <param name="maxValue">Maximum acceptable value</param>
        /// <returns>The clamped value</returns>
        public static int Clamp(int value, int minValue, int maxValue)
        {
            return Math.Max(minValue, Math.Min(maxValue, value));
        }
    }
}
