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
    /// <summary>
    /// Things generator. Creates player starts, items, monsters and other things.
    /// </summary>
    public sealed class ThingsGenerator : IDisposable
    {
        /// <summary>
        /// Default angle is North.
        /// </summary>
        private const int DEFAULT_ANGLE = 90;

        /// <summary>
        /// Number of deathmatch starts.
        /// </summary>
        private const int DEATHMATCH_STARTS_COUNT = 8;

        /// <summary>
        /// PNG2WAD Preferences.
        /// </summary>
        private readonly Preferences Preferences;
        
        /// <summary>
        /// Map theme to used.
        /// </summary>
        private readonly PreferencesTheme Theme;

        /// <summary>
        /// List of 64x64 where a thing can be spawned.
        /// </summary>
        private readonly List<Point> FreeTiles;

        public ThingsGenerator(Preferences preferences, PreferencesTheme theme)
        {
            Preferences = preferences;
            Theme = theme;

            FreeTiles = new List<Point>();
        }

        public void CreateThings(DoomMap map, TileType[,] subTiles)
        {
            int i, x, y;

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


            if (Preferences.GenerateEntranceAndExit)
            { 
                AddPlayerStart(map, subTiles); // Single-player and coop starts (spawned on entrances, or next to each other is none found)
                AddThings(map, DEATHMATCH_STARTS_COUNT, ThingSkillVariation.None, 11); // Deathmatch starts (spawned anywhere on the map)
            }

            float thingsCountMultiplier = FreeTiles.Count / 1000.0f; // Bigger map = more things

            if (Preferences.GenerateThings)
            {
                for (i = 0; i < Preferences.THINGS_CATEGORY_COUNT; i++)
                    AddThings(map, (ThingCategory)i,
                        (int)(Preferences.ThingsCount[i][0] * thingsCountMultiplier), (int)(Preferences.ThingsCount[i][1] * thingsCountMultiplier));
            }
        }

        private void AddThings(DoomMap map, ThingCategory thingCategory, int minCount, int maxCount, ThingSkillVariation skillVariation = ThingSkillVariation.None)
        {
            int count = Toolbox.RandomInt(minCount, maxCount + 1);
            AddThings(map, count, skillVariation, Preferences.ThingsTypes[(int)thingCategory]);
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
 