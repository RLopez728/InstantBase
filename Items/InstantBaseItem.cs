using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Text;
using InstantBase.Structures;

namespace InstantBase.Items
{
    public class InstantBaseItem : ModItem
    {
        private string[] frameBlueprint;
        private string[] wallBlueprint;
        private string[] foregroundBlueprint;

        private BuildMaterials materials = new BuildMaterials()
        {
            FrameTile = TileID.GrayBrick,
            WallType = WallID.GrayBrick,
            PlatformTile = TileID.Platforms
        };

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

            // frameBlueprint = LoadBlueprint("Assets/Structures/Frame.txt");
            // wallBlueprint = LoadBlueprint("Assets/Structures/Walls.txt");
            // foregroundBlueprint = LoadBlueprint("Assets/Structures/Foreground.txt");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.autoReuse = false;

            Item.maxStack = 1;
            Item.consumable = false;

            Item.rare = ItemRarityID.White;
            Item.value = Item.buyPrice(silver:50);

            Item.UseSound = SoundID.Item1;
        }

        public override bool? UseItem(Player player)
        {
            frameBlueprint ??= LoadBlueprint("Assets/Structures/Frame.txt");
            wallBlueprint ??= LoadBlueprint("Assets/Structures/Walls.txt");
            foregroundBlueprint ??= LoadBlueprint("Assets/Structures/Foreground.txt");

            if (frameBlueprint == null)
                {
                    Main.NewText("Frame blueprint is NULL!");
                    return false;
                }

            if (wallBlueprint == null)
                {
                    Main.NewText("Wall blueprint is NULL!");
                    return false;
                }

            if (foregroundBlueprint == null)
                {
                    Main.NewText("Foreground blueprint is NULL!");
                    return false;
                }

            Point tilePosition = Main.MouseWorld.ToTileCoordinates();

            ClearArea(
                tilePosition.X,
                tilePosition.Y,
                frameBlueprint[0].Length,
                frameBlueprint.Length
            );

            PlaceFrame(tilePosition);
            PlaceWalls(tilePosition);
            PlaceForeground(tilePosition);

            WorldGen.RangeFrame(
                tilePosition.X,
                tilePosition.Y,
                tilePosition.X + frameBlueprint[0].Length,
                tilePosition.Y + frameBlueprint.Length
            );

            return true;
        }

        private string[] LoadBlueprint(String path)
        {
            byte[] bytes = Mod.GetFileBytes(path);

            if (bytes == null)
                throw new Exception($"Couldn't find blueprint: {path}");

            string text = Encoding.UTF8.GetString(bytes);

            return text
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        private void PlaceFrame(Point origin)
        {
            for (int y = 0; y < frameBlueprint.Length; y++)
            {
                for (int x = 0; x < frameBlueprint[y].Length; x++)
                {
                    if (frameBlueprint[y][x] == '#')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            materials.FrameTile
                        );
                    }
                }
            }
        }

        private void PlaceWalls(Point origin)
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

        private void PlaceForeground(Point origin)
        {
            for (int y = 0; y < foregroundBlueprint.Length; y++)
            {
                for (int x = 0; x < foregroundBlueprint[y].Length; x++)
                {
                    if (foregroundBlueprint[y][x] == 'E')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            TileID.Torches
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'P')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            materials.PlatformTile
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'H')
                    {
                        PlaceRightSlopePlatform(
                            origin.X + x,
                            origin.Y + y
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'R')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            TileID.Rope
                        );
                    }
                }
            }
        }

        private void PlaceRightSlopePlatform(int x, int y)
        {
            WorldGen.PlaceTile(
                x,
                y,
                materials.PlatformTile
            );

            Tile tile = Main.tile[x, y];

            tile.Slope = SlopeType.SlopeDownLeft;
            tile.IsHalfBlock = false;

            WorldGen.SquareTileFrame(x, y);
        }

        private void ClearArea(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    WorldGen.KillTile(x,y);

                    Main.tile[x,y].WallType = WallID.None;
                    Main.tile[x,y].LiquidAmount = 0;
                }
            }
        }
    }
}