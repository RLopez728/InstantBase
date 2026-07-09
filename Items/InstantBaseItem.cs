using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
    }
}