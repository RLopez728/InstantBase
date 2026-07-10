using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace InstantBase.UI
{
    public class MaterialSlot : UIElement
    {
        public Item DisplayItem = new Item();

        public ushort SelectedTile { get; private set; }

        public MaterialSlot()
        {
            Width.Set(50f, 0f);
            Height.Set(50f, 0f);

            DisplayItem.TurnToAir();
        }

        public void SetTile(ushort tileID)
        {
            SelectedTile = tileID;

            DisplayItem.SetDefaults(tileID);
        }

        protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
}