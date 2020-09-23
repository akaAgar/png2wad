using PixelsOfDoom.Config;
using PixelsOfDoom.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelsOfDoom.Generator
{
    public sealed class ThingsGenerator : IDisposable
    {
        /// <summary>
        /// Default angle is North.
        /// </summary>
        private const int DEFAULT_ANGLE = 90;

        private const int DEATHMATCH_STARTS_COUNT = 8;

        private readonly Preferences Preferences;
        private readonly PreferencesTheme Theme;

        private readonly List<Point> FreeTiles;
        private float MapSizeMultiplier;

        public ThingsGenerator(Preferences preferences, PreferencesTheme theme)
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
            { 
                AddPlayerStart(map, subTiles); // Single-player and coop starts (spawned on entrances, or next to each other is none found)
                AddThings(map, DEATHMATCH_STARTS_COUNT, ThingSkillVariation.None, 11); // Deathmatch starts (spawned anywhere on the map)
            }

            if (Preferences.GenerateThings)
            {
                AddThings(map, ThingCategory.MonstersVeryHard, 2, 5);
                AddThings(map, ThingCategory.MonstersHard, 5, 10);
                AddThings(map, ThingCategory.MonstersAverage, 15, 25);
                AddThings(map, ThingCategory.MonstersEasy, 15, 25);

                AddThings(map, ThingCategory.PowerUps, 0, 2);

                AddThings(map, ThingCategory.WeaponsHigh, 1, 3);
                AddThings(map, ThingCategory.WeaponsLow, 2, 4);

                AddThings(map, ThingCategory.Health, 8, 10);

                AddThings(map, ThingCategory.AmmoLarge, 4, 8);
                AddThings(map, ThingCategory.AmmoSmall, 8, 12);
                AddThings(map, ThingCategory.Armor, 2, 4);
            }
        }

        private void AddThings(DoomMap map, ThingCategory thingCategory, int minCount, int maxCount, ThingSkillVariation skillVariation = ThingSkillVariation.None)
        {
            int count = Toolbox.RandomInt(minCount, maxCount + 1);
            count = (int)(count * MapSizeMultiplier);
            AddThings(map, count, skillVariation, Preferences.Things[(int)thingCategory]);
        }

        private void AddThings(DoomMap map, int count, ThingSkillVariation skillVariation, params int[] thingTypes)
        {
            if ((count < 1) || (thingTypes.Length == 0)) return;

            for (int i = 0; i < count; i++)
            {
                if (FreeTiles.Count == 0) return;

                ThingOptions options = ThingOptions.AllSkills;

                switch (skillVariation)
                {
                    case ThingSkillVariation.MoreThingsInEasyMode:
                        if (Toolbox.RandomInt(4) == 0) options = ThingOptions.Skill12 | ThingOptions.Skill3;
                        else if (Toolbox.RandomInt(3) == 0) options = ThingOptions.Skill12;
                        break;
                    case ThingSkillVariation.MoreThingsInHardMode:
                        if (Toolbox.RandomInt(3) == 0) options = ThingOptions.Skill3 | ThingOptions.Skill45;
                        else if (Toolbox.RandomInt(2) == 0) options = ThingOptions.Skill45;
                        break;
                }

                int thingType = Toolbox.RandomFromArray(thingTypes);
                Point pt = Toolbox.RandomFromList(FreeTiles);
                AddThing(map, pt.X, pt.Y, thingType, Toolbox.RandomInt(360), options);
                FreeTiles.Remove(pt);
            }
        }

        private void AddPlayerStart(DoomMap map, TileType[,] subTiles)
        {
            int x, y;
            List<Point> entrances = new List<Point>();

            for (int player = 1; player <= 4; player++)
            {
                bool foundAnEntrance = false;

                for (x = 0; x < subTiles.GetLength(0); x+= MapGenerator.SUBTILE_DIVISIONS)
                    for (y = 0; y < subTiles.GetLength(1); y += MapGenerator.SUBTILE_DIVISIONS)
                    {
                        if (!foundAnEntrance && (subTiles[x, y] == TileType.Entrance))
                        {
                            Point entranceTile = new Point(x / MapGenerator.SUBTILE_DIVISIONS, y / MapGenerator.SUBTILE_DIVISIONS);
                            if (entrances.Contains(entranceTile)) continue; // Entrance already in use
                            AddThing(map, x / MapGenerator.SUBTILE_DIVISIONS, y / MapGenerator.SUBTILE_DIVISIONS, player);
                            entrances.Add(entranceTile);
                            foundAnEntrance = true;
                        }
                    }

                if (foundAnEntrance) continue;

                // No "entrance" tile found, put player start in a random free tile
                if (FreeTiles.Count > 0)
                {
                    Point pt = Toolbox.RandomFromList(FreeTiles);
                    FreeTiles.Remove(pt);
                    AddThing(map, pt.X, pt.Y, player);
                    entrances.Add(pt);
                }

                if (foundAnEntrance) continue;

                // No free spot, put player start in the northwest map corner
                AddThing(map, player - 1, 0, player);
                entrances.Add(new Point(player - 1, 0));
            }
        }

        private void AddThing(DoomMap map, int x, int y, int thingType, int angle = (int)DEFAULT_ANGLE, ThingOptions options = ThingOptions.AllSkills)
        {
            map.Things.Add(
                new Thing(
                    (int)((x + .5f) * MapGenerator.TILE_SIZE),
                    (int)((y + .5f) * -MapGenerator.TILE_SIZE),
                    thingType, angle, options));
        }

        public void Dispose()
        {

        }
    }
}
 