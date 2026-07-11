using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstantBase.Structures
{
    public static class StructureBuilder
    {
        private static string[] frameBlueprint;
        private static string[] wallBlueprint;
        private static string[] foregroundBlueprint;

        private static ushort? houseEnablerTileId;

        private static List<Point> occupiedCells;

        private struct TileSnapshotData
        {
            public int TileType;
            public int WallType;
            public bool HasTile;
            public bool IsHalfBlock;
            public SlopeType Slope;
            public int LiquidAmount;
            public int LiquidType;
            public int TileFrameX;
            public int TileFrameY;
            public int WallFrameX;
            public int WallFrameY;
            public int WallFrameNumber;
            public bool RedWire;
            public bool BlueWire;
            public bool GreenWire;
            public bool YellowWire;
            public int TileColor;
            public int WallColor;
        }

        private struct BuildSnapshot
        {
            public Point Origin;
            public int Width;
            public int Height;
            public TileSnapshotData[,] Tiles;
        }

        private static Dictionary<int, BuildSnapshot> lastBuildByPlayer = new Dictionary<int, BuildSnapshot>();

        private static void EnsureBlueprintsLoaded()
        {
            frameBlueprint ??= LoadBlueprint("Assets/Structures/Frame.txt");
            wallBlueprint ??= LoadBlueprint("Assets/Structures/Walls.txt");
            foregroundBlueprint ??= LoadBlueprint("Assets/Structures/Foreground.txt");
        }

        private static string[] LoadBlueprint(string path)
        {
            Mod mod = ModContent.GetInstance<InstantBase>();

            byte[] bytes = mod.GetFileBytes(path);

            if (bytes == null)
                throw new Exception($"Couldn't find blueprint: {path}");

            string text = Encoding.UTF8.GetString(bytes);

            string[] lines = text
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            int maxWidth = lines.Max(line => line.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < maxWidth)
                {
                    lines[i] = lines[i].PadRight(maxWidth, ' ');
                }
            }

            return lines;
        }

        public static (int width, int height) GetStructureBounds()
        {
            EnsureBlueprintsLoaded();

            int width = new[] { frameBlueprint[0].Length, wallBlueprint[0].Length, foregroundBlueprint[0].Length }.Max();
            int height = new[] { frameBlueprint.Length, wallBlueprint.Length, foregroundBlueprint.Length }.Max();

            return (width, height);
        }

        public static IReadOnlyList<Point> GetOccupiedCells()
        {
            EnsureBlueprintsLoaded();

            if (occupiedCells != null)
                return occupiedCells;

            var (width, height) = GetStructureBounds();
            occupiedCells = new List<Point>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsCellOccupied(x, y))
                        occupiedCells.Add(new Point(x, y));
                }
            }

            return occupiedCells;
        }

        private static bool IsCellOccupied(int x, int y)
        {
            return
                (y < frameBlueprint.Length &&
                x < frameBlueprint[y].Length &&
                frameBlueprint[y][x] == '#')
                ||
                (y < wallBlueprint.Length &&
                x < wallBlueprint[y].Length &&
                wallBlueprint[y][x] == 'W')
                ||
                (y < foregroundBlueprint.Length &&
                x < foregroundBlueprint[y].Length &&
                foregroundBlueprint[y][x] != ' ');
        }

        public static void Build(Point origin, BuildMaterials materials, int playerWhoAmI)
        {
            EnsureBlueprintsLoaded();

            var (width, height) = GetStructureBounds();

            CaptureSnapshot(origin, width, height, playerWhoAmI);

            ClearArea(origin.X, origin.Y);
            PlaceFrame(origin, materials);
            PlaceWalls(origin, materials);
            PlaceForeground(origin, materials);

            WorldGen.RangeFrame(
                origin.X,
                origin.Y,
                origin.X + width,
                origin.Y + height
            );
        }

        private static void CaptureSnapshot(Point origin, int width, int height, int playerWhoAmI)
        {
            var tiles = new TileSnapshotData[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile source = Main.tile[origin.X + x, origin.Y + y];

                    tiles[x, y] = new TileSnapshotData
                    {
                        TileType = source.TileType,
                        WallType = source.WallType,
                        HasTile = source.HasTile,
                        IsHalfBlock = source.IsHalfBlock,
                        Slope = source.Slope,
                        LiquidAmount = source.LiquidAmount,
                        LiquidType = source.LiquidType,
                        TileFrameX = source.TileFrameX,
                        TileFrameY = source.TileFrameY,
                        WallFrameX = source.WallFrameX,
                        WallFrameY = source.WallFrameY,
                        WallFrameNumber = source.WallFrameNumber,
                        RedWire = source.RedWire,
                        BlueWire = source.BlueWire,
                        GreenWire = source.GreenWire,
                        YellowWire = source.YellowWire,
                        TileColor = source.TileColor,
                        WallColor = source.WallColor
                    };
                }
            }

            lastBuildByPlayer[playerWhoAmI] = new BuildSnapshot
            {
                Origin = origin,
                Width = width,
                Height = height,
                Tiles = tiles
            };
        }

        public static bool Undo(int playerWhoAmI, out Point origin, out int width, out int height)
        {
            if (!lastBuildByPlayer.TryGetValue(playerWhoAmI, out BuildSnapshot snapshot))
            {
                origin = default;
                width = 0;
                height = 0;
                return false;
            }

            for (int x = 0; x < snapshot.Width; x++)
            {
                for (int y = 0; y < snapshot.Height; y++)
                {
                    TileSnapshotData data = snapshot.Tiles[x, y];
                    Tile dest = Main.tile[snapshot.Origin.X + x, snapshot.Origin.Y + y];

                    dest.TileType = (ushort)data.TileType;
                    dest.WallType = (ushort)data.WallType;
                    dest.HasTile = data.HasTile;
                    dest.IsHalfBlock = data.IsHalfBlock;
                    dest.Slope = data.Slope;
                    dest.LiquidAmount = (byte)data.LiquidAmount;
                    dest.LiquidType = data.LiquidType;
                    dest.TileFrameX = (short)data.TileFrameX;
                    dest.TileFrameY = (short)data.TileFrameY;
                    dest.WallFrameX = (short)data.WallFrameX;
                    dest.WallFrameY = (short)data.WallFrameY;
                    dest.WallFrameNumber = (byte)data.WallFrameNumber;
                    dest.RedWire = data.RedWire;
                    dest.BlueWire = data.BlueWire;
                    dest.GreenWire = data.GreenWire;
                    dest.YellowWire = data.YellowWire;
                    dest.TileColor = (byte)data.TileColor;
                    dest.WallColor = (byte)data.WallColor;
                }
            }

            origin = snapshot.Origin;
            width = snapshot.Width;
            height = snapshot.Height;

            lastBuildByPlayer.Remove(playerWhoAmI);
            return true;
        }

        private static void ClearArea(int startX, int startY)
        {
            foreach (Point cell in GetOccupiedCells())
            {
                int worldX = startX + cell.X;
                int worldY = startY + cell.Y;

                Tile tile = Main.tile[worldX, worldY];

                tile.ClearTile();
                tile.WallType = WallID.None;
                tile.LiquidAmount = 0;
            }
        }

        private static void PlaceFrame(Point origin, BuildMaterials materials)
        {
            for (int y = 0; y < frameBlueprint.Length; y++)
            {
                for (int x = 0; x < frameBlueprint[y].Length; x++)
                {
                    if (frameBlueprint[y][x] == '#')
                    {
                        WorldGen.PlaceTile(origin.X + x, origin.Y + y, materials.FrameTile);
                    }
                }
            }
        }

        private static void PlaceWalls(Point origin, BuildMaterials materials)
        {
            for (int y = 0; y < wallBlueprint.Length; y++)
            {
                for (int x = 0; x < wallBlueprint[y].Length; x++)
                {
                    if (wallBlueprint[y][x] == 'W')
                    {
                        int wallX = origin.X + x;
                        int wallY = origin.Y + y;

                        Main.tile[wallX, wallY].WallType = materials.WallType;

                        WorldGen.SquareWallFrame(wallX, wallY);
                    }
                }
            }
        }

        private static void PlaceForeground(Point origin, BuildMaterials materials)
        {
            for (int y = 0; y < foregroundBlueprint.Length; y++)
            {
                for (int x = 0; x < foregroundBlueprint[y].Length; x++)
                {
                    char c = foregroundBlueprint[y][x];

                    if (c == 'E')
                    {
                        WorldGen.PlaceTile(origin.X + x, origin.Y + y, GetHouseEnablerTileId());
                    }
                    else if (c == 'P')
                    {
                        WorldGen.PlaceTile(origin.X + x, origin.Y + y, materials.PlatformTile, false, false, -1, materials.PlatformStyle);
                    }
                    else if (c == 'H')
                    {
                        PlaceRightSlopePlatform(origin.X + x, origin.Y + y, materials);
                    }
                    else if (c == 'R')
                    {
                        WorldGen.PlaceTile(origin.X + x, origin.Y + y, TileID.Rope);
                    }
                }
            }
        }

        private static void PlaceRightSlopePlatform(int x, int y, BuildMaterials materials)
        {
            WorldGen.PlaceTile(x, y, materials.PlatformTile, false, false, -1, materials.PlatformStyle);

            Tile tile = Main.tile[x, y];

            tile.Slope = SlopeType.SlopeDownLeft;
            tile.IsHalfBlock = false;

            WorldGen.SquareTileFrame(x, y);
        }

        private static ushort GetHouseEnablerTileId()
        {
            if (houseEnablerTileId.HasValue)
                return houseEnablerTileId.Value;

            if (ModLoader.TryGetMod("miningcracks_take_on_luiafk", out Mod luiAfk))
            {
                ModItem match = luiAfk.GetContent<ModItem>()
                    .FirstOrDefault(modItem =>
                        modItem.DisplayName.Value.Equals("Unlimited House Enabler")
                    );

                if (match != null && match.Item.createTile >= 0)
                {
                    houseEnablerTileId = (ushort)match.Item.createTile;
                    return houseEnablerTileId.Value;
                }
            }

            houseEnablerTileId = TileID.Torches;
            return houseEnablerTileId.Value;
        }
    }
}