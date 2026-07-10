using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using InstantBase.Items;
using Terraria;
using Terraria.ID;

namespace InstantBase.UI
{
    public class InstantBaseUI : UIState
    {
        private UIPanel panel;

        private InstantBaseItem currentItem;

        private MaterialSlot frameSlot;

        public override void OnInitialize()
        {
        }

        public void Open(InstantBaseItem item)
        {
            currentItem = item;

            RemoveAllChildren();

            panel = new UIPanel();

            panel.Width.Set(300f, 0f);
            panel.Height.Set(200f, 0f);

            panel.Left.Set(100f, 0f);
            panel.Top.Set(100f, 0f);

            Append(panel);

            frameSlot = new MaterialSlot();

            frameSlot.SetTile(TileID.WoodBlock);

            frameSlot.Left.Set(120f, 0f);
            frameSlot.Top.Set(70f, 0f);

            panel.Append(frameSlot);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (currentItem != null && frameSlot != null)
            {
                currentItem.SetFrameTile(frameSlot.SelectedTile);
            }
        }
    }
}