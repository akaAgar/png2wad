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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PixelsOfDoom.Config
{
    public sealed class GeneratorConfig : IDisposable
    {
        private static readonly int DEFAULT_COLOR = Color.Black.ToArgb();

        private Dictionary<int, PixelSetting> Pixels { get; }

        public bool Doom1Format { get; private set; } = false;
        public int Episode { get; private set; } = 1;
        public int TileSize { get; private set; } = 64;

        public GeneratorConfig(string filePath)
        {
            Pixels = new Dictionary<int, PixelSetting>();

            ParseFile(filePath);
        }

        private void ParseFile(string filePath)
        {

            if (!File.Exists(filePath)) return;

            string[] lines = File.ReadAllLines(filePath);

            List<string[]> lineValues = new List<string[]>();

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];

                if (l.Contains("//")) l = l.Substring(0, l.IndexOf("//")); // Remove comments
                l = l.Trim();
                if (!l.Contains(" ")) continue;


                string[] values = (from v in l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) select v.Trim().ToLowerInvariant()).ToArray();
                if (values.Length < 2) continue;
                lineValues.Add(values);
            }

            ParseValues(lineValues);
        }

        private void ParseValues(List<string[]> lineValues)
        {
            Pixels.Clear();
            int currentColor = -1;

            foreach (string[] values in lineValues)
            {
                switch (values[0])
                {
                    case "doom1format":
                        Doom1Format = ParseArgumentsAsBoolean(values)[0];
                        continue;

                    case "episode":
                        Episode = Math.Min(9, Math.Max(1, ParseArgumentsAsInt32(values)[0]));
                        continue;

                    case "tileSize":
                        TileSize = Math.Max(1, ParseArgumentsAsInt32(values)[0]);
                        continue;

                    case "color":
                        Color? sectionColor = Toolbox.GetColorFromString(values[1]);
                        if (sectionColor.HasValue)
                        {
                            currentColor = sectionColor.Value.ToArgb();
                            if (!Pixels.ContainsKey(currentColor)) Pixels.Add(currentColor, new PixelSetting());
                        }
                        else
                            currentColor = -1;
                        continue;

                    case "floorheight":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].FloorHeight = ParseArgumentsAsInt32(values);
                        continue;

                    case "ceilingheight":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].CeilingHeight = ParseArgumentsAsInt32(values);
                        continue;

                    case "linespecial":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].LineSpecial = ParseArgumentsAsInt32(values);
                        continue;

                    case "sectorspecial":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].SectorSpecial = ParseArgumentsAsInt32(values);
                        continue;

                    case "floortexture":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].FloorTexture = ParseArgumentsAsString(values);
                        continue;

                    case "ceilingtexture":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].CeilingTexture = ParseArgumentsAsString(values);
                        continue;

                    case "walltexture":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].WallTexture = ParseArgumentsAsString(values);
                        continue;

                    case "walltexturealt":
                        if (currentColor < 0) continue;
                        Pixels[currentColor].WallTextureAlt = ParseArgumentsAsString(values);
                        continue;
                }
            }

            if (!Pixels.ContainsKey(DEFAULT_COLOR))
                Pixels.Add(DEFAULT_COLOR, new PixelSetting());
        }

        private static bool[] ParseArgumentsAsBoolean(params string[] values)
        {
            if ((values == null) || (values.Length < 2)) return new bool[] { false };

            bool[] boolValues = new bool[values.Length - 1];

            for (int i = 0; i < boolValues.Length; i++)
                try
                {
                    boolValues[i] = Convert.ToBoolean(values[i + 1]);
                }
                catch (Exception)
                {
                    boolValues[i] = false;
                }

            return boolValues;
        }

        private static string[] ParseArgumentsAsString(params string[] values)
        {
            if ((values == null) || (values.Length < 2)) return new string[] { "" };
            return values.Skip(1).ToArray();
        }

        private static int[] ParseArgumentsAsInt32(params string[] values)
        {
            if ((values == null) || (values.Length < 2)) return new int[] { 0 };

            int[] intValues = new int[values.Length - 1];
            
            for (int i = 0; i < intValues.Length; i++)
                try
                {
                    intValues[i] = Convert.ToInt32(values[i + 1]);
                }
                catch (Exception)
                {
                    intValues[i] = 0;
                }

            return intValues;
        }

        public void Dispose()
        {

        }
    }
}
