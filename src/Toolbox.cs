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

    }
}
