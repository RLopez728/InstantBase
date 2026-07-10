using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace InstantBase.UI
{
    public class UIWindowPanel : UIElement
    {
        private static readonly Color BodyColor = new Color(35, 40, 55);
        private static readonly Color BorderColor = new Color(15, 17, 24);

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float alpha = 0.65f;

            Rectangle outer = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);
            spriteBatch.Draw(pixel, outer, BorderColor * alpha);

            Rectangle inner = new Rectangle(outer.X + 1, outer.Y + 1, outer.Width - 2, outer.Height - 2);
            spriteBatch.Draw(pixel, inner, BodyColor * alpha);
        }
    }
}