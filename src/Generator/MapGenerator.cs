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

using PixelsOfDoom.Config;
using PixelsOfDoom.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelsOfDoom.Generator
{
    public sealed class MapGenerator : IDisposable
    {
        private const ThingAngle DEFAULT_ANGLE = ThingAngle.North;
        private const int TILE_SIZE = 64;
        private const int SUBTILE_DIVISIONS = 8;
        private const int SUBTILE_SIZE = TILE_SIZE / SUBTILE_DIVISIONS;

        private static readonly Point VERTEX_POSITION_MULTIPLIER = new Point(TILE_SIZE, -TILE_SIZE);

        private readonly Random RNG;

        private int MapWidth { get { return Things.GetLength(0); } }
        private int MapHeight { get { return Things.GetLength(1); } }

        private int MapSubWidth { get { return Sectors.GetLength(0); } }
        private int MapSubHeight { get { return Sectors.GetLength(1); } }

        private int[,] Sectors;
        private bool[,] Things;

        private readonly Config.Settings Config;

        public MapGenerator(Config.Settings config)
        {
            RNG = new Random();
            Config = config;
        }

        public DoomMap Generate(string name, Bitmap bitmap)
        {
            DoomMap map = new DoomMap(name);
            
            CreateArrays(bitmap);
            CreateSectors(bitmap, map);
            CreateLines(map);
            CreateThings(map, bitmap, 0);

            return map;
        }

        private void CreateArrays(Bitmap bitmap)
        {
            int x, y;

            Sectors = new int[bitmap.Width, bitmap.Height];
            for (x = 0; x < MapSubWidth; x++)
                for (y = 0; y < MapSubHeight; y++)
                    Sectors[x, y] = -2;

            Things = new bool[bitmap.Width, bitmap.Height];
            for (x = 0; x < MapWidth; x++)
                for (y = 0; y < MapHeight; y++)
                    Things[x, y] = false;
        }

        private void CreateLines(DoomMap map)
        {
            int x, y;

            bool[,,] linesSet = new bool[MapWidth, MapHeight, 4];

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
                        if (sector == neighborSector) continue;

                        if ((neighborSector >= 0) && ((i == (int)WallDirection.South) || (i == (int)WallDirection.East))) // Make sure two-sided lines aren't drawn twice
                            continue;

                        bool vertical = (neighborDirection.X != 0);

                        int length = AddLine(map, new Point(x, y), (WallDirection)i, vertical, sector, neighborSector);

                        for (int j = 0; j <length; j++)
                        {
                            Point segmentPosition = new Point(x, y).Add(vertical ? new Point(0, j) : new Point(j, 0));
                            if (!IsPointOnMap(segmentPosition)) continue;
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
                length++;


                if ((GetSector(position) != sector) || (GetSector(position.Add(neighborOffset)) != neighborSector)) break;
            } while (true);

            int v2 = map.AddVertex(position.Add(vertexOffset).Mult(VERTEX_POSITION_MULTIPLIER));

            if (flipVectors) { v1 += v2; v2 = v1 - v2; v1 -= v2; } // Quick hack to flip two integers without temporary variable

            if (neighborSector < 0) // neighbor is a wall
            {
                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "STARTAN2", sector));
                map.Linedefs.Add(new Linedef(v1, v2, LinedefFlags.Impassible, 0, 0, -1, map.Sidedefs.Count - 1));
            }
            else
            {
                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "-", neighborSector));
                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "-", sector));
                map.Linedefs.Add(new Linedef(v1, v2, LinedefFlags.TwoSided, 0, 0, map.Sidedefs.Count - 2, map.Sidedefs.Count - 1));
            }

            return length;
        }

        private int GetSector(Point position) { return GetSector(position.X, position.Y); }
        private int GetSector(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= MapWidth) || (y >= MapHeight) && (Sectors[x, y] < 0))
                return -1;

            return Sectors[x, y];
        }

        private bool IsPointOnMap(Point position)
        {
            return !((position.X < 0) || (position.Y < 0) || (position.X >= MapWidth) || (position.Y >= MapHeight));
        }

        private void CreateThings(DoomMap map, Bitmap bitmap, int depth)
        {
            Point position = GetRandomFreeCell();
            Things[position.X, position.Y] = true;
            ThingAngle angle = GetNonWallFacingAngle(position);

            position = new Point(
                (int)((position.X + .5f) * TILE_SIZE),
                (int)((position.Y + .5f) * TILE_SIZE));

            map.AddThing(position, 1, angle); // Player start
        }

        private ThingAngle GetNonWallFacingAngle(Point position)
        {
            if (GetSector(position) < 0) return DEFAULT_ANGLE;

            List<ThingAngle> validAngles = new List<ThingAngle>();
            if (GetSector(position.X - 1, position.Y) >= 0) validAngles.Add(ThingAngle.West);
            if (GetSector(position.X + 1, position.Y) >= 0) validAngles.Add(ThingAngle.East);
            if (GetSector(position.X, position.Y - 1) >= 0) validAngles.Add(ThingAngle.North);
            if (GetSector(position.X, position.Y + 1) >= 0) validAngles.Add(ThingAngle.South);

            if (validAngles.Count == 0) return DEFAULT_ANGLE;
            return validAngles[RNG.Next(validAngles.Count)];
        }

        private Point GetRandomFreeCell()
        {
            Point cell;

            do
            {
                cell = new Point(RNG.Next(MapWidth), RNG.Next(MapHeight));
            } while ((Sectors[cell.X, cell.Y] < 0) || Things[cell.X, cell.Y]);

            return cell;
        }

        private void CreateSectors(Bitmap bitmap, DoomMap map)
        {
            map.Sectors.Clear();

            int x, y;
            Color c;

            for (x = 0; x < MapWidth; x++)
                for (y = 0; y < MapHeight; y++)
                {
                    if (Sectors[x, y] != -2) continue; // Cell was already checked

                    c = bitmap.GetPixel(x, y);
                    if (c.IsSameRGB(Color.White))
                    {
                        Sectors[x, y] = -1;
                        continue;
                    }

                    FloodFillSector(bitmap, map, x, y, map.Sectors.Count, c);
                    map.Sectors.Add(new Sector(0, 64, "FLOOR5_4", "FLOOR4_1", 192, 0, 0));
                }
        }

        public void Dispose()
        {

        }

        private void FloodFillSector(Bitmap bitmap, DoomMap map, int x, int y, int sector, Color color)
        {
            Stack<Point> pixels = new Stack<Point>();
            Point pt = new Point(x, y);
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                
                if (a.X < MapWidth && a.X > 0 && a.Y < MapHeight && a.Y > 0)
                {
                    if ((Sectors[a.X, a.Y] == -2) && (bitmap.GetPixel(a.X, a.Y) == color))
                    {
                        Sectors[a.X, a.Y] = sector;
                        pixels.Push(new Point(a.X - 1, a.Y));
                        pixels.Push(new Point(a.X + 1, a.Y));
                        pixels.Push(new Point(a.X, a.Y - 1));
                        pixels.Push(new Point(a.X, a.Y + 1));
                    }
                }
            }
        }
    }
}
