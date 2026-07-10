using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;
using System;

namespace InstantBase.UI
{
    public class UIMaterialSlot : UIElement
    {
        public Item DisplayItem = new Item();

        private readonly Action<Item> onItemSelected;

        public UIMaterialSlot(Action<Item> onItemSelected)
        {
            Width.Set(52f, 0f);
            Height.Set(52f, 0f);

            DisplayItem.TurnToAir();

            this.onItemSelected = onItemSelected;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);

            if (Main.mouseItem == null || Main.mouseItem.IsAir)
                return;

            // Peek only — never modify Main.mouseItem.
            DisplayItem = Main.mouseItem.Clone();
            DisplayItem.stack = 1;

            onItemSelected?.Invoke(DisplayItem);

            SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
        }

        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);

            DisplayItem = new Item();
            DisplayItem.TurnToAir();

            onItemSelected?.Invoke(DisplayItem);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();

            Texture2D backTexture = TextureAssets.InventoryBack.Value;
            spriteBatch.Draw(
                backTexture,
                dims.Position(),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                dims.Width / backTexture.Width,
                SpriteEffects.None,
                0f
            );

            if (DisplayItem.IsAir)
                return;

            Main.instance.LoadItem(DisplayItem.type);
            Texture2D itemTexture = TextureAssets.Item[DisplayItem.type].Value;

            float scale = 1f;
            float maxSize = 32f;
            if (itemTexture.Width > maxSize || itemTexture.Height > maxSize)
            {
                scale = itemTexture.Width > itemTexture.Height
                    ? maxSize / itemTexture.Width
                    : maxSize / itemTexture.Height;
            }

            Vector2 drawPos = dims.Position() + new Vector2(dims.Width, dims.Height) / 2f
                - new Vector2(itemTexture.Width, itemTexture.Height) * scale / 2f;

            spriteBatch.Draw(
                itemTexture,
                drawPos,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );

            if (IsMouseHovering)
            {
                Main.hoverItemName = DisplayItem.Name;
            }
        }
    }
}