using PixelsOfDoom.Config;
using PixelsOfDoom.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelsOfDoom.Generator
{
    public sealed class ThingsMaker : IDisposable
    {
        /// <summary>
        /// Default angle is North.
        /// </summary>
        private const int DEFAULT_ANGLE = 90;

        private readonly Preferences Preferences;
        private readonly PreferencesTheme Theme;

        private readonly List<Point> FreeTiles;
        private float MapSizeMultiplier;

        public ThingsMaker(Preferences preferences, PreferencesTheme theme)
        {
            Preferences = preferences;
            Theme = theme;

            FreeTiles = new List<Point>();
        }

        public void CreateThings(DoomMap map, TileType[,] subTiles)
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

            if (Preferences.GenerateEntranceAndExit)
                AddPlayerStart(map, subTiles);

            if (Preferences.GenerateThings)
            {
                AddThingCategory(map, ThingCategory.MonstersVeryHard);
                AddThingCategory(map, ThingCategory.MonstersHard);
                AddThingCategory(map, ThingCategory.MonstersAverage);
                AddThingCategory(map, ThingCategory.MonstersEasy);

                AddThingCategory(map, ThingCategory.PowerUps);

                AddThingCategory(map, ThingCategory.WeaponsHigh);
                AddThingCategory(map, ThingCategory.WeaponsLow);

                AddThingCategory(map, ThingCategory.Health);

                AddThingCategory(map, ThingCategory.AmmoLarge);
                AddThingCategory(map, ThingCategory.AmmoSmall);
                AddThingCategory(map, ThingCategory.Armor);
            }
        }

        private void AddThingCategory(DoomMap map, ThingCategory thingCategory)
        {
            int count = 0;
            int chance = 100;
            switch (thingCategory)
            {
                case ThingCategory.AmmoLarge: count = Toolbox.RandomInt(4, 9); break;
                case ThingCategory.AmmoSmall: count = Toolbox.RandomInt(8, 13); break;
                case ThingCategory.Armor: count = Toolbox.RandomInt(2, 5); break;
                case ThingCategory.Health: count = Toolbox.RandomInt(8, 11); break;
                case ThingCategory.PowerUps: count = Toolbox.RandomInt(0, 3);break;
                case ThingCategory.WeaponsHigh: count = Toolbox.RandomInt(1, 3); break;
                case ThingCategory.WeaponsLow: count = Toolbox.RandomInt(2, 4); break;
                case ThingCategory.MonstersEasy: count = Toolbox.RandomInt(20, 31); break;
                case ThingCategory.MonstersAverage: count = Toolbox.RandomInt(20, 31); break;
                case ThingCategory.MonstersHard: count = Toolbox.RandomInt(5, 11); break;
                case ThingCategory.MonstersVeryHard: count = Toolbox.RandomInt(2, 6); break;
            }

            if (Toolbox.RandomInt(100) >= chance) return;

            count = (int)(count * MapSizeMultiplier);

            if (count <= 0) return;
            if (Preferences.Things[(int)thingCategory].Length == 0) return;

            for (int i = 0; i < count; i++)
            {
                if (FreeTiles.Count == 0) return;

                int thingType = Toolbox.RandomFromArray(Preferences.Things[(int)thingCategory]);
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

            // No "entrance" tile found, put player start in a random free tile
            if (FreeTiles.Count > 0)
            {
                Point pt = Toolbox.RandomFromList(FreeTiles);
                FreeTiles.Remove(pt);
                AddThing(map, pt.X, pt.Y, 1);
                return;
            }

            // No free spot, put player start in tile 0,0
            AddThing(map, 0, 0, 1);
        }

        private void AddThing(DoomMap map, int x, int y, int thingType, int angle = (int)DEFAULT_ANGLE)
        {
            map.Things.Add(
                new Thing(
                    (int)((x + .5f) * MapGenerator.TILE_SIZE),
                    (int)((y + .5f) * -MapGenerator.TILE_SIZE),
                    thingType, angle, ThingOptions.AllSkills));
        }

        public void Dispose()
        {

        }
    }
}
 