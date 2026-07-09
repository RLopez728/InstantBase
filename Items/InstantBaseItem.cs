using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace InstantBase.Items
{
    public class InstantBaseItem : ModItem
    {
        private readonly string[] frameBlueprint =
            {
                "###########",
                "#.........#",
                "#.........#",
                "#.........#",
                "#.........#",
                "###########"
            };

        private readonly string[] wallBlueprint =
            {
                "...........",
                ".WWWWWWWWW.",
                ".WWWWWWWWW.",
                ".WWWWWWWWW.",
                ".WWWWWWWWW.",
                "..........."
            };

        private readonly string[] foregroundBlueprint =
            {
                "           ",
                "           ",
                "     T     ",
                "           ",
                "           ",
                "           "
            };

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
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
                            TileID.WoodBlock
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

                        Main.tile[wallX, wallY].WallType = WallID.Wood;

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
                    if (foregroundBlueprint[y][x] == 'T')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            TileID.Torches
                        );
                    }
                }
            }
        }

        private void ClearArea(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    WorldGen.KillTile(x,y);

                    Main.tile[x,y].WallType = 0;
                    Main.tile[x,y].LiquidAmount = 0;
                }
            }
        }
    }
}