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

namespace PixelsOfDoom.Config
{
    public struct SettingsPixel
    {
        private const int DEFAULT_CEILING_HEIGHT = 64;
        private const int DEFAULT_FLOOR_HEIGHT = 0;
        private const int DEFAULT_LIGHT_LEVEL = 164;
        private const string DEFAULT_CEILING_TEXTURE = "FLOOR4_1";
        private const string DEFAULT_FLOOR_TEXTURE = "FLOOR5_4";
        private const string DEFAULT_WALL_TEXTURE = "STARTAN2";
        private const string DEFAULT_WALLALT_TEXTURE = "COMPWERD";

        public SettingsPixelType PixelType { get; }

        public int[] CeilingHeight { get; }
        public int[] FloorHeight { get; }
        public int[] LineSpecial { get; }
        public int[] SectorSpecial { get; }

        public int[] LightLevel { get; }

        public string[] CeilingTexture { get; }
        public string[] FloorTexture { get; }
        public string[] SpecialFlatTexture { get; }
        public string[] WallTexture { get; }
        public string[] WallTextureAlt { get; }
        public string[] WallTextureAlt2 { get; }

        public SettingsPixel(INIFile ini, string section)
        {
            PixelType = ini.GetValue(section, "Type", SettingsPixelType.Room);

            CeilingHeight = ini.GetValueArray<int>(section, "CeilingHeight");
            FloorHeight = ini.GetValueArray<int>(section, "FloorHeight");
            LineSpecial = ini.GetValueArray<int>(section, "LineSpecial");
            SectorSpecial = ini.GetValueArray<int>(section, "SectorSpecial");

            LightLevel = ini.GetValueArray<int>(section, "LightLevel");

            CeilingTexture = ini.GetValueArray<string>(section, "CeilingTexture");
            FloorTexture = ini.GetValueArray<string>(section, "FloorTexture");

            if (PixelType == SettingsPixelType.Door)
            {
                SpecialFlatTexture = ini.GetValueArray<string>(section, "DoorFlatTexture");
                WallTexture = ini.GetValueArray<string>(section, "DoorTexture");
                WallTextureAlt = ini.GetValueArray<string>(section, "DoorSideTexture");
                WallTextureAlt2 = ini.GetValueArray<string>(section, "DoorTrackTexture");
            }
            else
            {
                SpecialFlatTexture = new string[0];
                WallTexture = ini.GetValueArray<string>(section, "WallTexture");
                WallTextureAlt = ini.GetValueArray<string>(section, "WallTextureAlt");
                WallTextureAlt2 = new string[0];
            }

            if (CeilingHeight.Length == 0) CeilingHeight = new int[] { DEFAULT_CEILING_HEIGHT };
            if (FloorHeight.Length == 0) FloorHeight = new int[] { DEFAULT_FLOOR_HEIGHT };
            if (LineSpecial.Length == 0) LineSpecial = new int[] { 0 };
            if (SectorSpecial.Length == 0) SectorSpecial = new int[] { 0 };

            if (LightLevel.Length == 0) LightLevel = new int[] { DEFAULT_LIGHT_LEVEL };

            if (CeilingTexture.Length == 0) CeilingTexture = new string[] { DEFAULT_CEILING_TEXTURE };
            if (FloorTexture.Length == 0) FloorTexture = new string[] { DEFAULT_FLOOR_TEXTURE };
            if (SpecialFlatTexture.Length == 0) SpecialFlatTexture = new string[] { DEFAULT_FLOOR_TEXTURE };
            if (WallTexture.Length == 0) WallTexture = new string[] { DEFAULT_WALL_TEXTURE };
            if (WallTextureAlt.Length == 0) WallTextureAlt = new string[] { DEFAULT_WALLALT_TEXTURE };
            if (WallTextureAlt2.Length == 0) WallTextureAlt2 = new string[] { DEFAULT_WALLALT_TEXTURE };
        }

        public SettingsPixel(SettingsPixelType type)
        {
            PixelType = type;

            CeilingHeight = new int[] { DEFAULT_CEILING_HEIGHT };
            FloorHeight = new int[] { DEFAULT_FLOOR_HEIGHT };
            LineSpecial = new int[] { 0 };
            SectorSpecial = new int[] { 0 };

            LightLevel = new int[] { DEFAULT_LIGHT_LEVEL };

            CeilingTexture = new string[] { DEFAULT_CEILING_TEXTURE };
            FloorTexture = new string[] { DEFAULT_FLOOR_TEXTURE };
            SpecialFlatTexture = new string[] { DEFAULT_FLOOR_TEXTURE };
            WallTexture = new string[] { DEFAULT_WALL_TEXTURE };
            WallTextureAlt = new string[] { DEFAULT_WALLALT_TEXTURE };
            WallTextureAlt2 = new string[] { DEFAULT_WALLALT_TEXTURE };
        }
    }
}
