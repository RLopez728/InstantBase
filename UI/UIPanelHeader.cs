using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.UI;
using System;
using Terraria.Audio;
using Terraria.ID;

namespace InstantBase.UI
{
    public class UIPanelHeader : UIElement
    {
        private readonly UIElement target;
        private bool dragging;
        private Vector2 dragOffset;

        private static readonly Color HeaderColor = new Color(45, 55, 75);
        private static readonly Color HeaderColorHover = new Color(55, 68, 92);

        private bool headerHover;

        public UIPanelHeader(UIElement target, string title, Action onClose)
        {
            this.target = target;

            Width.Set(0f, 1f);
            Height.Set(30f, 0f);

            var titleText = new UIText(title)
            {
                TextColor = Color.White
            };
            titleText.Left.Set(10f, 0f);
            titleText.Top.Set(6f, 0f);
            Append(titleText);

            var closeButton = new UICloseButton();
            closeButton.Left.Set(-26f, 1f);
            closeButton.Top.Set(4f, 0f);
            closeButton.OnLeftClick += (evt, el) => onClose?.Invoke();
            Append(closeButton);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            headerHover = true;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            headerHover = false;
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            dragging = true;

            Vector2 panelPos = target.GetDimensions().Position();
            dragOffset = evt.MousePosition - panelPos;
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (dragging)
            {
                Vector2 newPos = new Vector2(Main.mouseX, Main.mouseY) - dragOffset;

                target.Left.Set(newPos.X, 0f);
                target.Top.Set(newPos.Y, 0f);
                target.Parent?.Recalculate();
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();

            float alpha = 0.85f;

            Color barColor = (dragging || headerHover) ? HeaderColorHover : HeaderColor;

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height),
                barColor * alpha
            );

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)dims.X, (int)(dims.Y + dims.Height - 1), (int)dims.Width, 1),
                Color.Black * 0.4f * alpha
            );

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, 1),
                Color.White * 0.15f * alpha
            );

            int dotSpacing = 6;
            int startX = (int)(dims.X + dims.Width / 2f - dotSpacing);
            int dotY = (int)(dims.Y + dims.Height / 2f);

            for (int i = 0; i < 3; i++)
            {
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle(startX + i * dotSpacing, dotY, 2, 2),
                    Color.White * 0.35f * alpha
                );
            }
        }
    }

    public class UICloseButton : UIElement
    {
        private bool hovering;

        private static readonly Color Idle = new Color(70, 70, 70);
        private static readonly Color Hover = new Color(200, 60, 60);

        public UICloseButton()
        {
            Width.Set(20f, 0f);
            Height.Set(20f, 0f);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            hovering = true;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            hovering = false;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();

            Color boxColor = hovering ? Hover : Idle;

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height),
                boxColor
            );

            Vector2 center = dims.Position() + new Vector2(dims.Width, dims.Height) / 2f;
            DrawDiagonalLine(spriteBatch, center, 12, 2, MathHelper.PiOver4, Color.White);
            DrawDiagonalLine(spriteBatch, center, 12, 2, -MathHelper.PiOver4, Color.White);
        }

        private void DrawDiagonalLine(SpriteBatch spriteBatch, Vector2 center, int length, int thickness, float rotation, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Rectangle destRect = new Rectangle((int)center.X, (int)center.Y, length, thickness);

            spriteBatch.Draw(
                pixel,
                destRect,
                sourceRect,
                color,
                rotation,
                new Vector2(0.5f, 0.5f),
                SpriteEffects.None,
                0f
            );
        }
    }
}