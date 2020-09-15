/*
==========================================================================
This file is part of Pixels of Doom, a tool to create Doom maps from PNG files
by @akaAgar (https://github.com/akaAgar/one-bit-of-engine)
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
        private const int CELL_SIZE = 64;

        private readonly Random RNG;

        private int MapWidth { get { return Sectors.GetLength(0); } }
        private int MapHeight { get { return Sectors.GetLength(1); } }

        private int[,] Sectors;
        private int[,] Things;

        public MapGenerator()
        {
            RNG = new Random();
        }

        public DoomMap Generate(string name, GeneratorConfig config, Bitmap bitmap)
        {
            DoomMap map = new DoomMap(name);

            int x, y;
            Sectors = new int[bitmap.Width, bitmap.Height];
            Things = new int[bitmap.Width, bitmap.Height];

            for (x = 0; x < MapWidth; x++)
                for (y = 0; y < MapHeight; y++)
                {
                    Sectors[x, y] = -2;
                    Things[x, y] = 0;
                }

            CreateSectors(bitmap, map);
            CreateLines(map);
            CreateThings(map, bitmap, config, 0);

            return map;
        }

        private void CreateLines(DoomMap map)
        {
            int x, y;
            int dX, dY;
            Point v1, v2; // vertices positions
            int v1i, v2i; // indices of the vertices

            int sector, neighborSector;

            for (x = 0; x < MapWidth; x++)
                for (y = 0; y < MapHeight; y++)
                {
                    sector = GetSector(x, y);
                    if (sector < 0) continue; // Tile is a wall, do nothing

                    for (dX = -1; dX <= 1; dX++)
                        for (dY = -1; dY <= 1; dY++)
                        {
                            if ((dX == 0) && (dY == 0)) continue;
                            if ((dX != 0) && (dY != 0)) continue;
                            neighborSector = GetSector(x + dX, y + dY);

                            if (neighborSector == sector) continue; // Same sector, no need to add a line

                            GetVertices(x, y, dX, dY, out v1, out v2);
                            v1i = map.AddVertex(v1);
                            v2i = map.AddVertex(v2);

                            if (neighborSector < 0) // neighbor is a wall
                            {
                                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "STARTAN2", sector));
                                map.Linedefs.Add(new Linedef(v1i, v2i, LinedefFlags.Impassible, 0, 0, -1, map.Sidedefs.Count - 1));
                            }
                            else
                            {
                                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "-", neighborSector));
                                map.Sidedefs.Add(new Sidedef(0, 0, "-", "-", "-", sector));
                                map.Linedefs.Add(new Linedef(v1i, v2i, LinedefFlags.TwoSided, 0, 0, map.Sidedefs.Count - 2, map.Sidedefs.Count - 1));
                            }
                        }
                }
        }

        private void GetVertices(int x, int y, int dX, int dY, out Point v1, out Point v2)
        {
            v1 = Point.Empty; v2 = Point.Empty;

            if (dX == -1)
            {
                v1 = new Point(x * CELL_SIZE, (y + 1) * -CELL_SIZE);
                v2 = new Point(x * CELL_SIZE, y * -CELL_SIZE);
            }
            else if (dX == 1)
            {
                v1 = new Point((x + 1) * CELL_SIZE, y * -CELL_SIZE);
                v2 = new Point((x + 1) * CELL_SIZE, (y + 1) * -CELL_SIZE);
            }
            else if (dY == -1)
            {
                v1 = new Point(x * CELL_SIZE, y * -CELL_SIZE);
                v2 = new Point((x + 1) * CELL_SIZE, y * -CELL_SIZE);
            }
            else if (dY == 1)
            {
                v1 = new Point((x + 1) * CELL_SIZE, (y + 1) * -CELL_SIZE);
                v2 = new Point(x * CELL_SIZE, (y + 1) * -CELL_SIZE);
            }
        }

        private int GetSector(Point position) { return GetSector(position.X, position.Y); }
        private int GetSector(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= MapWidth) || (x >= MapHeight) && (Sectors[x, y] < 0))
                return -1;

            return Sectors[x, y];
        }

        private void CreateThings(DoomMap map, Bitmap bitmap, GeneratorConfig config, int depth)
        {
            Point position = GetRandomFreeCell();
            ThingAngle angle = GetNonWallFacingAngle(position);

            AddThing(map, position, 1, angle); // Player start
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

        private void AddThing(DoomMap map, Point position, int type, ThingAngle angle = DEFAULT_ANGLE, ThingOptions options = ThingOptions.AllSkills)
        {
            map.Things.Add(new Thing(position.X * CELL_SIZE + CELL_SIZE / 2, position.Y * -CELL_SIZE + CELL_SIZE / -2, (int)angle, type, options));
        }

        private Point GetRandomFreeCell()
        {
            Point cell;

            do
            {
                cell = new Point(RNG.Next(MapWidth), RNG.Next(MapHeight));
            } while ((Sectors[cell.X, cell.Y] < 0) || (Things[cell.X, cell.Y] > 0));

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
