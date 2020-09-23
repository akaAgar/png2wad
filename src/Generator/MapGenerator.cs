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
using ToolsOfDoom.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PNG2WAD.Generator
{
    public sealed class MapGenerator : IDisposable
    {
        public const int TILE_SIZE = 64;
        public const int SUBTILE_SIZE = TILE_SIZE / SUBTILE_DIVISIONS;
        public const int SUBTILE_DIVISIONS = 8;

        private static readonly Point VERTEX_POSITION_MULTIPLIER = new Point(SUBTILE_SIZE, -SUBTILE_SIZE);

        private readonly Random RNG;

        private int MapSubWidth { get { return SubTiles.GetLength(0); } }
        private int MapSubHeight { get { return SubTiles.GetLength(1); } }

        private PreferencesTheme Theme;
        private string[] ThemeTextures;
        private TileType[,] SubTiles;
        private int[,] Sectors;
        private List<SectorInfo> SectorsInfo;

        private readonly Preferences Preferences;

        public MapGenerator(Preferences preferences)
        {
            RNG = new Random();
            Preferences = preferences;
        }

        public DoomMap Generate(string name, Bitmap bitmap)
        {
            if (bitmap == null) return null;
            
            DoomMap map = new DoomMap(name);

            CreateTheme(bitmap);
            CreateArrays(bitmap);
            CreateSectors(map);
            CreateLines(map);

            using (ThingsGenerator thingsGenerator = new ThingsGenerator(Preferences, Theme))
            {
                thingsGenerator.CreateThings(map, SubTiles);
            }

            return map;
        }

        private void CreateTheme(Bitmap bitmap)
        {
            Theme = Preferences.GetTheme(bitmap.GetPixel(0, 0));
            bitmap.SetPixel(0, 0, Color.White);
            ThemeTextures = new string[PreferencesTheme.THEME_TEXTURES_COUNT];

            for (int i = 0; i < PreferencesTheme.THEME_TEXTURES_COUNT; i++)
                ThemeTextures[i] = Toolbox.RandomFromArray(Theme.Textures[i]);
        }

        private TileType GetTileTypeFromPixel(Color pixel)
        {
            if (pixel.IsSameRGB(Color.White)) return TileType.Wall;

            if (pixel.IsSameRGB(Color.FromArgb(0, 0, 255))) return TileType.RoomExterior;
            if (pixel.IsSameRGB(Color.FromArgb(0, 128, 0))) return TileType.RoomSpecialCeiling;
            if (pixel.IsSameRGB(Color.FromArgb(255, 0, 0))) return TileType.RoomSpecialFloor;

            if (pixel.IsSameRGB(Color.FromArgb(128, 128, 0))) return TileType.Door;
            if (pixel.IsSameRGB(Color.Magenta)) return TileType.Secret;
            
            if (pixel.IsSameRGB(Color.Yellow)) return TileType.Entrance;
            if (pixel.IsSameRGB(Color.Lime)) return TileType.Exit;

            return TileType.Room;
        }

        private void CreateArrays(Bitmap bitmap)
        {
            int x, y, sX, sY;
            TileType tileType, subTileType;

            SectorsInfo = new List<SectorInfo>();

            SubTiles = new TileType[bitmap.Width * SUBTILE_DIVISIONS, bitmap.Height * SUBTILE_DIVISIONS];
            for (x = 0; x < bitmap.Width; x++)
                for (y = 0; y < bitmap.Height; y++)
                {
                    tileType = GetTileTypeFromPixel(bitmap.GetPixel(x, y));

                    if (!Preferences.GenerateEntranceAndExit) // Entrance and exit generation disabled, do not create entrance/exit tiles
                    {
                        if ((tileType == TileType.Entrance) || (tileType == TileType.Exit))
                            tileType = TileType.Room;
                    }

                    for (sX = 0; sX < SUBTILE_DIVISIONS; sX++)
                        for (sY = 0; sY < SUBTILE_DIVISIONS; sY++)
                        {
                            subTileType = tileType;

                            if (tileType == TileType.Door)
                            {
                                subTileType = TileType.DoorSide;

                                if ((x > 0) && (x < bitmap.Width - 1) &&
                                    (GetTileTypeFromPixel(bitmap.GetPixel(x - 1, y)) != TileType.Room) &&
                                    (GetTileTypeFromPixel(bitmap.GetPixel(x + 1, y)) != TileType.Room))
                                {
                                    if ((sY == 3) || (sY == 4))
                                        subTileType = TileType.Door;
                                }
                                else
                                {
                                    if ((sX == 3) || (sX == 4))
                                        subTileType = TileType.Door;
                                }
                            }

                            SubTiles[x * SUBTILE_DIVISIONS + sX, y * SUBTILE_DIVISIONS + sY] = subTileType;
                        }
                }

            Sectors = new int[bitmap.Width * SUBTILE_DIVISIONS, bitmap.Height * SUBTILE_DIVISIONS];
            for (x = 0; x < bitmap.Width * SUBTILE_DIVISIONS; x++)
                for (y = 0; y < bitmap.Height * SUBTILE_DIVISIONS; y++)
                    Sectors[x, y] = -2;
        }

        private void CreateLines(DoomMap map)
        {
            int x, y;

            bool[,,] linesSet = new bool[MapSubWidth, MapSubHeight, 4];

            for (x = 0; x < MapSubWidth; x++)
                for (y = 0; y < MapSubHeight; y++)
                {
                    int sector = GetSector(x, y);
                    if (sector < 0) continue; // Tile is a wall, do nothing

                    for (int i = 0; i < 4; i++)
                    {
                        if (linesSet[x, y, i]) continue; // Line already drawn

                        Point neighborDirection = GetDirectionOffset((WallDirection)i);

                        int neighborSector = GetSector(x + neighborDirection.X, y + neighborDirection.Y);
                        if (sector == neighborSector) continue; // Same sector on both sides, no need to add a line

                        if ((neighborSector >= 0) && ((i == (int)WallDirection.South) || (i == (int)WallDirection.East)))
                            continue; // Make sure two-sided lines aren't drawn twice

                        bool vertical = (neighborDirection.X != 0);

                        int length = AddLine(map, new Point(x, y), (WallDirection)i, vertical, sector, neighborSector);

                        for (int j = 0; j < length; j++)
                        {
                            Point segmentPosition = new Point(x, y).Add(vertical ? new Point(0, j) : new Point(j, 0));
                            if (!IsSubPointOnMap(segmentPosition)) continue;
                            linesSet[segmentPosition.X, segmentPosition.Y, i] = true;
                        }
                    }
                }
        }

        private static Point GetDirectionOffset(WallDirection direction)
        {
            switch (direction)
            {
                default: return new Point(0, -1); // case WallDirection.North
                case WallDirection.East: return new Point(1, 0);
                case WallDirection.South: return new Point(0, 1);
                case WallDirection.West: return new Point(-1, 0);
            }
        }

        private int AddLine(DoomMap map, Point position, WallDirection neighborDirection, bool vertical, int sector, int neighborSector)
        {
            bool flipVectors = false;
            Point vertexOffset = Point.Empty;
            Point neighborOffset = GetDirectionOffset(neighborDirection);
            Point neighborPosition;

            bool needsFlipping = SectorsInfo[sector].LinedefSpecial > 0;

            switch (neighborDirection)
            {
                case WallDirection.West:
                    flipVectors = true;
                    break;
                case WallDirection.East:
                    vertexOffset = new Point(1, 0);
                    break;
                case WallDirection.South:
                    flipVectors = true;
                    vertexOffset = new Point(0, 1);
                    break;
            }

            int v1 = map.AddVertex(position.Add(vertexOffset).Mult(VERTEX_POSITION_MULTIPLIER));
            int length = 0;

            Point direction = vertical ? new Point(0, 1) : new Point(1, 0);

            do
            {
                position = position.Add(direction);
                neighborPosition = position.Add(neighborOffset);
                length++;


                if ((GetSector(position) != sector) || (GetSector(neighborPosition) != neighborSector)) break;
            } while (true);

            int v2 = map.AddVertex(position.Add(vertexOffset).Mult(VERTEX_POSITION_MULTIPLIER));

            if (flipVectors) { v1 += v2; v2 = v1 - v2; v1 -= v2; } // Quick hack to flip two integers without temporary variable

            if (neighborSector < 0) // neighbor is a wall, create an impassible linedef
            {
                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", SectorsInfo[sector].WallTexture, sector));
                map.Linedefs.Add(new Linedef(v1, v2, LinedefFlags.Impassible | LinedefFlags.LowerUnpegged, 0, 0, -1, map.Sidedefs.Count - 1));
            }
            else // neighbor is another sector, create a two-sided linedef
            {
                int lineSpecial = Math.Max(SectorsInfo[sector].LinedefSpecial, SectorsInfo[neighborSector].LinedefSpecial);

                map.Sidedefs.Add(CreateTwoSidedSidedef(neighborSector, SectorsInfo[neighborSector], SectorsInfo[sector]));
                map.Sidedefs.Add(CreateTwoSidedSidedef(sector, SectorsInfo[sector], SectorsInfo[neighborSector]));

                if (needsFlipping)
                    map.Linedefs.Add(new Linedef(v2, v1, LinedefFlags.TwoSided, lineSpecial, 0, map.Sidedefs.Count - 1, map.Sidedefs.Count - 2));
                else
                    map.Linedefs.Add(new Linedef(v1, v2, LinedefFlags.TwoSided, lineSpecial, 0, map.Sidedefs.Count - 2, map.Sidedefs.Count - 1));
            }

            return length;
        }

        /// <summary>
        /// Creates a sidedef from two SectorInfo
        /// </summary>
        /// <param name="sectorID">Sector this sidedef faces</param>
        /// <param name="sector">Info about the sector this sidedef faces</param>
        /// <param name="neighborSector">Info about the sector this sidedef's opposing sector</param>
        private Sidedef CreateTwoSidedSidedef(int sectorID, SectorInfo sector, SectorInfo neighborSector)
        {
            string lowerTexture, upperTexture;

            if (neighborSector.Type == TileType.Door)
            {
                upperTexture = (neighborSector.CeilingHeight < sector.CeilingHeight) ? neighborSector.WallTextureUpper : "-";
                lowerTexture = (neighborSector.FloorHeight > sector.FloorHeight) ? neighborSector.WallTextureLower : "-";
            }
            else
            {
                upperTexture = (neighborSector.CeilingHeight < sector.CeilingHeight) ? sector.WallTexture : "-";
                lowerTexture = (neighborSector.FloorHeight > sector.FloorHeight) ? sector.WallTexture : "-";
            }

            return new Sidedef(0, 0, upperTexture, lowerTexture, "-", sectorID);
        }

        private int GetSector(Point position) { return GetSector(position.X, position.Y); }
        private int GetSector(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= MapSubWidth) || (y >= MapSubHeight) || (Sectors[x, y] < 0))
                return -1;

            return Sectors[x, y];
        }

        private TileType GetSubTile(Point position) { return GetSubTile(position.X, position.Y); }
        private TileType GetSubTile(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= MapSubWidth) || (y >= MapSubHeight))
                return TileType.Wall;

            return SubTiles[x, y];
        }

        private bool IsSubPointOnMap(Point position)
        {
            return !((position.X < 0) || (position.Y < 0) || (position.X >= MapSubWidth) || (position.Y >= MapSubHeight));
        }

        

        private void CreateSectors(DoomMap map)
        {
            int x, y;

            map.Sectors.Clear();

            for (x = 0; x < MapSubWidth; x++)
                for (y = 0; y < MapSubHeight; y++)
                {
                    if (Sectors[x, y] != -2) continue; // Cell was already checked

                    if (SubTiles[x, y] == TileType.Wall)
                    {
                        Sectors[x, y] = -1;
                        continue;
                    }

                    Stack<Point> pixels = new Stack<Point>();
                    Point pt = new Point(x, y);
                    pixels.Push(pt);

                    while (pixels.Count > 0)
                    {
                        Point a = pixels.Pop();

                        if (a.X < MapSubWidth && a.X > 0 && a.Y < MapSubHeight && a.Y > 0)
                        {
                            if ((Sectors[a.X, a.Y] == -2) && (SubTiles[a.X, a.Y].Equals(SubTiles[x, y])))
                            {
                                Sectors[a.X, a.Y] = SectorsInfo.Count;
                                pixels.Push(new Point(a.X - 1, a.Y));
                                pixels.Push(new Point(a.X + 1, a.Y));
                                pixels.Push(new Point(a.X, a.Y - 1));
                                pixels.Push(new Point(a.X, a.Y + 1));
                            }
                        }
                    }

                    SectorInfo sectorInfo = new SectorInfo(SubTiles[x, y], Theme, ThemeTextures);
                    SectorsInfo.Add(sectorInfo);

                    map.Sectors.Add(
                        new Sector(
                            sectorInfo.FloorHeight, sectorInfo.CeilingHeight,
                            sectorInfo.FloorTexture, sectorInfo.CeilingTexture,
                            sectorInfo.LightLevel, sectorInfo.SectorSpecial, 0));
                }
        }

        public void Dispose()
        {

        }
    }
}
