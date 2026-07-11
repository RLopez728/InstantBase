using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using InstantBase.Items;
using InstantBase.Structures;

namespace InstantBase.Systems
{
    public class BuildPreviewSystem : ModSystem
    {
        public override void PostDrawTiles()
        {
            Player player = Main.LocalPlayer;

            if (player.HeldItem?.ModItem is not InstantBaseItem)
                return;

            Point tilePosition = Main.MouseWorld.ToTileCoordinates();
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            foreach (Point cell in StructureBuilder.GetOccupiedCells())
            {
                Vector2 worldPos = new Vector2((tilePosition.X + cell.X) * 16, (tilePosition.Y + cell.Y) * 16);
                Vector2 screenPos = worldPos - Main.screenPosition;

                Rectangle cellRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, 16, 16);

                Main.spriteBatch.Draw(pixel, cellRect, Color.Black * 0.45f);
            }

            Main.spriteBatch.End();
        }

        private void DrawBorder(Texture2D pixel, Rectangle rect, int thickness, Color color)
        {
            // Top
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}