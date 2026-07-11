using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using InstantBase.Structures;
using InstantBase.UI;

namespace InstantBase.Items
{
    public class InstantBaseItem : ModItem
    {
        private BuildMaterials materials = new BuildMaterials()
        {
            FrameTile = TileID.GrayBrick,
            WallType = WallID.GrayBrick,
            PlatformTile = TileID.Platforms,
            PlatformStyle = 0
        };

        private Item frameSelectionItem = new Item();
        private Item wallSelectionItem = new Item();
        private Item platformSelectionItem = new Item();

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

            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(silver: 50);

            Item.UseSound = SoundID.Item1;

            frameSelectionItem.TurnToAir();
            wallSelectionItem.TurnToAir();
            platformSelectionItem.TurnToAir();
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                ModContent.GetInstance<InstantBaseUISystem>().ShowUI(this);
                return true;
            }

            Point tilePosition = Main.MouseWorld.ToTileCoordinates();

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                StructureBuilder.Build(tilePosition, materials, player.whoAmI);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<InstantBase>().GetPacket();

                packet.Write((byte)InstantBaseMessageType.BuildRequest);
                packet.Write(tilePosition.X);
                packet.Write(tilePosition.Y);
                packet.Write(materials.FrameTile);
                packet.Write(materials.WallType);
                packet.Write(materials.PlatformTile);
                packet.Write(materials.PlatformStyle);

                packet.Send();
            }

            return true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                return true;

            return base.CanUseItem(player);
        }

        public void SetFrameMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.FrameTile = TileID.GrayBrick;
                frameSelectionItem = new Item();
                frameSelectionItem.TurnToAir();
                return;
            }

            if (item.createTile < 0)
                return;

            materials.FrameTile = (ushort)item.createTile;
            frameSelectionItem = item.Clone();
        }

        public Item GetFrameSelectionItem() => frameSelectionItem;

        public void SetWallMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.WallType = WallID.GrayBrick;
                wallSelectionItem = new Item();
                wallSelectionItem.TurnToAir();
                return;
            }

            if (item.createWall < 0)
                return;

            materials.WallType = (ushort)item.createWall;
            wallSelectionItem = item.Clone();
        }

        public Item GetWallSelectionItem() => wallSelectionItem;

        public void SetPlatformMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.PlatformTile = TileID.Platforms;
                materials.PlatformStyle = 0;
                platformSelectionItem = new Item();
                platformSelectionItem.TurnToAir();
                return;
            }

            if (item.createTile < 0)
                return;

            materials.PlatformTile = (ushort)item.createTile;
            materials.PlatformStyle = item.placeStyle;
            platformSelectionItem = item.Clone();
        }

        public Item GetPlatformSelectionItem() => platformSelectionItem;

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("miningcracks_take_on_luiafk", out Mod luiAfk))
            {
                ModItem houseEnabler = luiAfk.GetContent<ModItem>()
                    .FirstOrDefault(modItem =>
                        modItem.DisplayName.Value.Equals("Unlimited House Enabler")
                    );

                if (houseEnabler != null)
                {
                    Recipe recipe = CreateRecipe();
                    recipe.AddIngredient(houseEnabler.Type, 1);
                    recipe.AddIngredient(ItemID.GrayBrick, 20);
                    recipe.AddTile(TileID.Anvils);
                    recipe.Register();
                    return;
                }
            }

            Recipe fallbackRecipe = CreateRecipe();
            fallbackRecipe.AddIngredient(ItemID.GrayBrick, 20);
            fallbackRecipe.AddIngredient(ItemID.Torch, 10);
            fallbackRecipe.AddIngredient(ItemID.FallenStar, 1);
            fallbackRecipe.AddTile(TileID.Anvils);
            fallbackRecipe.Register();
        }
    }
}