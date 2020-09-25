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

using PNG2WAD.Config;
using System;

namespace PNG2WAD.Generator
{
    /// <summary>
    /// Information about a sector. Includes all data required by the SECTORS lump in the wad file, but also about the wall textures.
    /// </summary>
    public struct SectorInfo
    {
        /// <summary>
        /// Type of sector.
        /// </summary>
        public TileType Type { get; }

        /// <summary>
        /// Ceiling height.
        /// </summary>
        public int CeilingHeight { get; private set; }

        /// <summary>
        /// Ceiling texture.
        /// </summary>
        public string CeilingTexture { get; private set; }

        /// <summary>
        /// Floor height.
        /// </summary>
        public int FloorHeight { get; private set; }
        
        /// <summary>
        /// Floor texture.
        /// </summary>
        public string FloorTexture { get; private set; }
        
        /// <summary>
        /// Light level.
        /// </summary>
        public int LightLevel { get; private set; }
        
        /// <summary>
        /// Special type of linedefs facing this sector.
        /// </summary>
        public int LinedefSpecial { get; private set; }

        /// <summary>
        /// Special type of this sector.
        /// </summary>
        public int SectorSpecial { get; private set; }
        
        /// <summary>
        /// Wall texture.
        /// </summary>
        public string WallTexture { get; private set; }
        
        /// <summary>
        /// Upper wall texture.
        /// </summary>
        public string WallTextureUpper { get; private set; }
        
        /// <summary>
        /// Lower wall texture.
        /// </summary>
        public string WallTextureLower { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type of sector</param>
        /// <param name="theme">Map theme</param>
        /// <param name="themeTextures">Selected "default" textures for this map</param>
        public SectorInfo(TileType type, PreferencesTheme theme, string[] themeTextures)
        {
            Type = type;

            FloorHeight = theme.Height[(int)ThemeSector.Default][0];
            CeilingHeight = theme.Height[(int)ThemeSector.Default][1];
            LinedefSpecial = 0;
            SectorSpecial = 0;

            LightLevel = theme.LightLevel[(int)ThemeSector.Default];

            CeilingTexture = themeTextures[(int)ThemeTexture.Ceiling];
            FloorTexture = themeTextures[(int)ThemeTexture.Floor];
            WallTexture = Toolbox.RandomFromArray(theme.Textures[(int)ThemeTexture.Wall]);
            WallTextureUpper = null;
            WallTextureLower = null;

            switch (type)
            {
                case TileType.Door:
                    CeilingHeight = FloorHeight;
                    LinedefSpecial = 1; // DR Door Open Wait Close
                    CeilingTexture = "CRATOP1";
                    WallTexture = "DOORTRAK";
                    WallTextureUpper = themeTextures[(int)ThemeTexture.Door];
                    break;

                case TileType.DoorSide:
                    WallTexture = themeTextures[(int)ThemeTexture.DoorSide];
                    break;

                case TileType.Entrance:
                    ApplySectorSpecial(theme, ThemeSector.Entrance);
                    FloorTexture = themeTextures[(int)ThemeTexture.FloorEntrance];
                    break;

                case TileType.Exit:
                    ApplySectorSpecial(theme, ThemeSector.Exit);
                    LinedefSpecial = 52; // W1 Exit Level
                    FloorTexture = themeTextures[(int)ThemeTexture.FloorExit];
                    break;

                case TileType.RoomExterior:
                    ApplySectorSpecial(theme, ThemeSector.Exterior);
                    CeilingTexture = "F_SKY1";
                    if (theme.Textures[(int)ThemeTexture.FloorExterior].Length > 0)
                        FloorTexture = themeTextures[(int)ThemeTexture.FloorExterior];
                    if (theme.Textures[(int)ThemeTexture.WallExterior].Length > 0)
                        WallTexture = Toolbox.RandomFromArray(theme.Textures[(int)ThemeTexture.WallExterior]);
                    break;

                case TileType.RoomSpecialCeiling:
                    ApplySectorSpecial(theme, ThemeSector.SpecialCeiling);
                    CeilingTexture = themeTextures[(int)ThemeTexture.CeilingSpecial];
                    break;

                case TileType.RoomSpecialFloor:
                    ApplySectorSpecial(theme, ThemeSector.SpecialFloor);
                    FloorTexture = themeTextures[(int)ThemeTexture.FloorSpecial];
                    break;

                case TileType.Secret:
                    CeilingHeight = FloorHeight;
                    LinedefSpecial = 31; // D1 Door Open Stay
                    SectorSpecial = 9; // Secret room
                    WallTexture = "DOORTRAK";
                    break;
            }

            CeilingHeight = Math.Max(FloorHeight, CeilingHeight);

            WallTextureUpper = WallTextureUpper ?? WallTexture;
            WallTextureLower = WallTextureLower ?? WallTexture;
        }

        /// <summary>
        /// Applies parameters read from a ThemeSector.
        /// </summary>
        /// <param name="theme">The map theme</param>
        /// <param name="themeSector">Type of ThemeSector to copy parameters from</param>
        private void ApplySectorSpecial(PreferencesTheme theme, ThemeSector themeSector)
        {
            CeilingHeight = theme.Height[(int)themeSector][1];
            FloorHeight = theme.Height[(int)themeSector][0];
            LightLevel = theme.LightLevel[(int)themeSector];
            SectorSpecial = theme.SectorSpecial[(int)themeSector];
        }
    }
}
