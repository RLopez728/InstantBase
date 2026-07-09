using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace InstantBase.Items
{
    public class InstantBaseItem : ModItem
    {
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

            Item.maxStack = 9999;
            Item.consumable = false;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver:50);

            Item.UseSound = SoundID.Item1;
        }

        public override bool? UseItem(Player player)
        {
            Point tilePosition = Main.MouseWorld.ToTileCoordinates();

            string [] blueprint =
            {
                "##########",
                "#........#",
                "#........#",
                "#........#",
                "#........#",
                "##########"
            };

            ClearArea(
                tilePosition.X,
                tilePosition.Y,
                blueprint[0].Length,
                blueprint.Length
            );

            for (int y = 0; y < blueprint.Length; y++)
            {
                for (int x = 0; x < blueprint[y].Length; x++)
                {
                    if (blueprint[y][x] == '#')
                    {
                        WorldGen.PlaceTile(
                            tilePosition.X + x,
                            tilePosition.Y + y,
                            TileID.WoodBlock
                        );
                    }
                }
            }

            return true;
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