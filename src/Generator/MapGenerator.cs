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
using System.Collections.Generic;
using System.Drawing;
using ToolsOfDoom.Map;

namespace PNG2WAD.Generator
{
    /// <summary>
    /// Map generator. Turns a PNG image into a Doom map.
    /// </summary>
    public sealed class MapGenerator : IDisposable
    {
        /// <summary>
        /// Size of each tile (one tile = one pixel in the PNG image) in Doom map units.
        /// </summary>
        public const int TILE_SIZE = 64;

        /// <summary>
        /// Number of subdivisions of each tile.
        /// Required to generate doors, as doors are divided into 3 sectors: 3 "doorstep" subtiles, 2 "door" subtiles and then 3 "doorstep" subtiles again.
        /// </summary>
        public const int SUBTILE_DIVISIONS = 8;

        /// <summary>
        /// The size of each subtile, in Doom map units.
        /// </summary>
        public const int SUBTILE_SIZE = TILE_SIZE / SUBTILE_DIVISIONS;

        /// <summary>
        /// Multiplier to get the position of a vertex from the map subtiles coordinates.
        /// Y axis has to be inverted because of the way Doom maps are made.
        /// </summary>
        private static readonly Point VERTEX_POSITION_MULTIPLIER = new Point(SUBTILE_SIZE, -SUBTILE_SIZE);

        /// <summary>
        /// Number of subtiles on the X-axis (equals to "source PNG width × SUBTILE_SIZE)
        /// </summary>
        private int MapSubWidth { get { return SubTiles.GetLength(0); } }

        /// <summary>
        /// Number of subtiles on the Y-axis (equals to "source PNG height × SUBTILE_SIZE)
        /// </summary>
        private int MapSubHeight { get { return SubTiles.GetLength(1); } }

        /// <summary>
        /// Map theme to use.
        /// </summary>
        private PreferencesTheme Theme;

        /// <summary>
        /// Selected texture for each ThemeTexture category.
        /// </summary>
        private string[] ThemeTextures;

        /// <summary>
        /// Type of each map sub-tile.
        /// </summary>
        private TileType[,] SubTiles;

        /// <summary>
        /// Sector index of each map sub-tile.
        /// </summary>
        private int[,] Sectors;

        /// <summary>
        /// List to sector info from which to generate sectors and linedefs.
        /// </summary>
        private List<SectorInfo> SectorsInfo;

        /// <summary>
        /// PNG2WAD preferences.
        /// </summary>
        private readonly Preferences Preferences;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preferences">PNG2WAD preferences</param>
        public MapGenerator(Preferences preferences)
        {
            Preferences = preferences;
        }

        /// <summary>
        /// Generates the map.
        /// </summary>
        /// <param name="name">Map name (MAP01, E1M1, etc)</param>
        /// <param name="bitmap">PNG to generate the file from</param>
        /// <returns>A Doom map</returns>
        public DoomMap Generate(string name, Bitmap bitmap)
        {
            if (bitmap == null) return null;
            
            DoomMap map = new DoomMap(name);

            CreateTheme(bitmap);
            CreateArrays(bitmap);
            CreateSectors(map);
            CreateLines(map);

            using (ThingsGenerator thingsGenerator = new ThingsGenerator(Preferences))
            {
                thingsGenerator.CreateThings(map, SubTiles);
            }

            return map;
        }

        /// <summary>
        /// Selects the proper theme from the PNG's upper-leftmost pixel, and picks random textures.
        /// </summary>
        /// <param name="bitmap">The PNG image</param>
        private void CreateTheme(Bitmap bitmap)
        {
            Theme = Preferences.GetTheme(bitmap.GetPixel(0, 0));
            bitmap.SetPixel(0, 0, Color.White);
            ThemeTextures = new string[PreferencesTheme.THEME_TEXTURES_COUNT];

            for (int i = 0; i < PreferencesTheme.THEME_TEXTURES_COUNT; i++)
                ThemeTextures[i] = Toolbox.RandomFromArray(Theme.Textures[i]);
        }

        /// <summary>
        /// Get a tile type from a pixel.
        /// </summary>
        /// <param name="pixel">A pixel on the PNG image</param>
        /// <returns>A tile type</returns>
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

        /// <summary>
        /// Creates/clears SectorInfo list and Subtiles and Sectors array before generating a map.
        /// </summary>
        /// <param name="bitmap">The map PNG image</param>
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

        /// <summary>
        /// Creates the map linedefs and sidedefs
        /// </summary>
        /// <param name="map">Doom map in which to write linedefs and sidedefs</param>
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

                        Point neighborDirection = GetTileSideOffset((TileSide)i);

                        int neighborSector = GetSector(x + neighborDirection.X, y + neighborDirection.Y);
                        if (sector == neighborSector) continue; // Same sector on both sides, no need to add a line

                        if ((neighborSector >= 0) && ((i == (int)TileSide.South) || (i == (int)TileSide.East)))
                            continue; // Make sure two-sided lines aren't drawn twice

                        bool vertical = (neighborDirection.X != 0);

                        int length = AddLine(map, new Point(x, y), (TileSide)i, vertical, sector, neighborSector);

                        for (int j = 0; j < length; j++)
                        {
                            Point segmentPosition = new Point(x, y).Add(vertical ? new Point(0, j) : new Point(j, 0));
                            if (!IsSubPointOnMap(segmentPosition)) continue;
                            linesSet[segmentPosition.X, segmentPosition.Y, i] = true;
                        }
                    }
                }
        }

        /// <summary>
        /// Gets the proper offset according to the side of the tile (N, S, E or W)
        /// </summary>
        /// <param name="side">One of the four sides of a tile</param>
        /// <returns>An offset</returns>
        private static Point GetTileSideOffset(TileSide side)
        {
            switch (side)
            {
                default: return new Point(0, -1); // case WallDirection.North
                case TileSide.East: return new Point(1, 0);
                case TileSide.South: return new Point(0, 1);
                case TileSide.West: return new Point(-1, 0);
            }
        }

        /// <summary>
        /// Adds a new linedef (and its sidedef(s)) to the map.
        /// </summary>
        /// <param name="map">The Doom map</param>
        /// <param name="position">Line start position</param>
        /// <param name="neighborDirection">Direction to the line's neighbor sector</param>
        /// <param name="vertical">Is the line vertical (on the Y-axis) or horizontal (on the X-axis)</param>
        /// <param name="sector">Index of the sector this line faces</param>
        /// <param name="neighborSector">Index of the sector this line is turned against</param>
        /// <returns></returns>
        private int AddLine(DoomMap map, Point position, TileSide neighborDirection, bool vertical, int sector, int neighborSector)
        {
            bool flipVectors = false;
            Point vertexOffset = Point.Empty;
            Point neighborOffset = GetTileSideOffset(neighborDirection);
            Point neighborPosition;

            bool needsFlipping = SectorsInfo[sector].LinedefSpecial > 0;

            switch (neighborDirection)
            {
                case TileSide.West:
                    flipVectors = true;
                    break;
                case TileSide.East:
                    vertexOffset = new Point(1, 0);
                    break;
                case TileSide.South:
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

        /// <summary>
        /// Gets the sector index at a given position.
        /// </summary>
        /// <param name="position">Coordinates of a sub-tile</param>
        /// <returns>A sector index or -1 if none</returns>
        private int GetSector(Point position) { return GetSector(position.X, position.Y); }

        /// <summary>
        /// Gets the sector index at a given position.
        /// </summary>
        /// <param name="x">X coordinate of a sub-tile</param>
        /// <param name="y">Y coordinate of a sub-tile</param>
        /// <returns>A sector index or -1 if none</returns>
        private int GetSector(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= MapSubWidth) || (y >= MapSubHeight) || (Sectors[x, y] < 0))
                return -1;

            return Sectors[x, y];
        }

        /// <summary>
        /// Is a sub-tile coordinate on the map?
        /// </summary>
        /// <param name="position">Coordinates of a sub-tile</param>
        /// <returns>True if sub-tile on the map, false if out of bounds</returns>
        private bool IsSubPointOnMap(Point position)
        {
            return !((position.X < 0) || (position.Y < 0) || (position.X >= MapSubWidth) || (position.Y >= MapSubHeight));
        }

        /// <summary>
        /// Creates the sectors on the Doom map.
        /// </summary>
        /// <param name="map">The Doom map</param>
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

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {

        }
    }
}
