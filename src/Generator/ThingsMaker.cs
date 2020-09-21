using PixelsOfDoom.Config;
using PixelsOfDoom.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelsOfDoom.Generator
{
    public sealed class ThingsMaker : IDisposable
    {
        private const ThingAngle DEFAULT_ANGLE = ThingAngle.North;

        private readonly PreferencesTheme Theme;
        private readonly List<Point> FreeTiles;
        private float MapSizeMultiplier;

        public ThingsMaker(PreferencesTheme theme)
        {
            Theme = theme;
            FreeTiles = new List<Point>();
        }

        public void CreateThings(DoomMap map, TileType[,] subTiles, int depth)
        {
            int x, y;

            FreeTiles.Clear();
            for (x = 0; x < subTiles.GetLength(0); x += MapGenerator.SUBTILE_DIVISIONS)
                for (y = 0; y < subTiles.GetLength(1); y += MapGenerator.SUBTILE_DIVISIONS)
                {
                    switch (subTiles[x,y])
                    {
                        case TileType.Door:
                        case TileType.DoorSide:
                        case TileType.Entrance:
                        case TileType.Exit:
                        case TileType.Secret:
                        case TileType.Wall:
                            continue;
                    }

                    FreeTiles.Add(new Point(x / MapGenerator.SUBTILE_DIVISIONS, y / MapGenerator.SUBTILE_DIVISIONS));
                }

            MapSizeMultiplier = FreeTiles.Count / 1000.0f;

            AddPlayerStart(map, subTiles);

            AddThingCategory(map, ThemeThing.MonstersVeryHard, depth);
            AddThingCategory(map, ThemeThing.MonstersHard, depth);
            AddThingCategory(map, ThemeThing.MonstersAverage, depth);
            AddThingCategory(map, ThemeThing.MonstersEasy, depth);

            AddThingCategory(map, ThemeThing.PowerUps, depth);

            AddThingCategory(map, ThemeThing.WeaponsHigh, depth);
            AddThingCategory(map, ThemeThing.WeaponsLow, depth);

            AddThingCategory(map, ThemeThing.Health, depth);

            AddThingCategory(map, ThemeThing.AmmoLarge, depth);
            AddThingCategory(map, ThemeThing.AmmoSmall, depth);
            AddThingCategory(map, ThemeThing.Armor, depth);
        }

        private void AddThingCategory(DoomMap map, ThemeThing thingCategory, int depth)
        {
            int count = 0;
            int chance = 100;
            switch (thingCategory)
            {
                case ThemeThing.AmmoLarge: count = Toolbox.RandomInt(4, 9); break;
                case ThemeThing.AmmoSmall: count = Toolbox.RandomInt(8, 13); break;
                case ThemeThing.Armor: count = Toolbox.RandomInt(2, 5); break;
                case ThemeThing.Health: count = Toolbox.RandomInt(8, 11); break;
                case ThemeThing.PowerUps: count = Toolbox.RandomInt(0, 3); chance = 70 + depth * 5; break;
                case ThemeThing.WeaponsHigh: count = Toolbox.RandomInt(1, 3); chance = 20 * depth; break;
                case ThemeThing.WeaponsLow: count = Toolbox.RandomInt(2, 4); break;
                case ThemeThing.MonstersEasy: count = Toolbox.RandomInt(20, 31); break;
                case ThemeThing.MonstersAverage: count = Toolbox.RandomInt(20 + Math.Min(depth, 9), 31 + Math.Min(depth, 9)); break;
                case ThemeThing.MonstersHard: count = Toolbox.RandomInt(Math.Min(depth * 2, 9), 10 + Math.Min(depth * 4, 16)); break;
                case ThemeThing.MonstersVeryHard: count = Toolbox.RandomInt(Math.Min(depth, 9), Math.Min(depth * 2, 9)); chance = 25 * depth; break;
            }

            if (Toolbox.RandomInt(100) >= chance) return;

            count = (int)(count * MapSizeMultiplier);

            if (count <= 0) return;
            if (Theme.Things[(int)thingCategory].Length == 0) return;

            for (int i = 0; i < count; i++)
            {
                if (FreeTiles.Count == 0) return;

                int thingType = Toolbox.RandomFromArray(Theme.Things[(int)thingCategory]);
                Point pt = Toolbox.RandomFromList(FreeTiles);
                AddThing(map, pt.X, pt.Y, thingType);
                FreeTiles.Remove(pt);
            }
        }

        private void AddPlayerStart(DoomMap map, TileType[,] subTiles)
        {
            int x, y;

            for (x = 0; x < subTiles.GetLength(0); x++)
                for (y = 0; y < subTiles.GetLength(1); y++)
                {
                    if (subTiles[x, y] == TileType.Entrance)
                    {
                        AddThing(map, x / MapGenerator.SUBTILE_DIVISIONS, y / MapGenerator.SUBTILE_DIVISIONS, 1);
                        return;
                    }
                }
        }

        private void AddThing(DoomMap map, int x, int y, int thingType, int angle = (int)DEFAULT_ANGLE)
        {
            map.Things.Add(
                new Thing(
                    (int)((x + .5f) * MapGenerator.TILE_SIZE),
                    (int)((y + .5f) * -MapGenerator.TILE_SIZE),
                    angle, thingType, ThingOptions.AllSkills));
        }

        public void Dispose()
        {

        }
    }
}
 