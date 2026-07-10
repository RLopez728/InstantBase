using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using InstantBase.Items;

namespace InstantBase.UI
{
    public class InstantBaseUI : UIState
    {
        private UIPanel panel;

        private InstantBaseItem currentItem;

        public override void OnInitialize()
        {
            panel = new UIPanel();

            panel.Width.Set(300f, 0f);
            panel.Height.Set(200f, 0f);

            panel.Left.Set(100f, 0f);
            panel.Top.Set(100f, 0f);

            Append(panel);
        }

        public void Open(InstantBaseItem item)
        {
            currentItem = item;
        }
    }
}