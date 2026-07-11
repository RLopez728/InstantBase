using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using InstantBase.Items;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using InstantBase.Structures;
using Terraria.ModLoader;

namespace InstantBase.UI
{
    public class InstantBaseUI : UIState
    {
        private UIWindowPanel panel;

        private InstantBaseItem currentItem;

        private UIMaterialSlot frameSlot;
        private UIMaterialSlot wallSlot;
        private UIMaterialSlot platformSlot;

        public override void OnInitialize()
        {
        }

        public void Open(InstantBaseItem item)
        {
            currentItem = item;

            if (panel == null)
            {
                RemoveAllChildren();

                panel = new UIWindowPanel();

                panel.Width.Set(300f, 0f);
                panel.Height.Set(260f, 0f);   // bumped from 220f to fit the button

                panel.Left.Set(-150f, 0.5f);
                panel.Top.Set(-130f, 0.5f);  // adjusted to keep it centered at the new height

                Append(panel);

                var header = new UIPanelHeader(panel, "Instant Base", HideUI);
                panel.Append(header);

                (frameSlot, _) = AddLabeledSlot("Frame", 0, 3, OnFrameMaterialSelected);
                (wallSlot, _) = AddLabeledSlot("Wall", 1, 3, OnWallMaterialSelected);
                (platformSlot, _) = AddLabeledSlot("Platform", 2, 3, OnPlatformMaterialSelected);

                var undoButton = new UIText("Undo Last Build")
                {
                    TextColor = Color.White
                };
                undoButton.Left.Set(80f, 0f);
                undoButton.Top.Set(190f, 0f);
                undoButton.OnLeftClick += (evt, el) => PerformUndo();
                undoButton.OnMouseOver += (evt, el) => ((UIText)el).TextColor = Color.Orange;
                undoButton.OnMouseOut += (evt, el) => ((UIText)el).TextColor = Color.White;
                panel.Append(undoButton);
            }

            frameSlot.DisplayItem = currentItem.GetFrameSelectionItem().Clone();
            wallSlot.DisplayItem = currentItem.GetWallSelectionItem().Clone();
            platformSlot.DisplayItem = currentItem.GetPlatformSelectionItem().Clone();
        }

        private void PerformUndo()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                StructureBuilder.Undo(Main.myPlayer, out _, out _, out _);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<InstantBase>().GetPacket();
                packet.Write((byte)InstantBaseMessageType.UndoRequest);
                packet.Send();
            }
        }

        private (UIMaterialSlot slot, UIText label) AddLabeledSlot(string labelText, int index, int totalSlots, System.Action<Item> onSelected)
        {
            const float slotSize = 52f;
            const float panelWidth = 300f;

            float totalSlotWidth = slotSize * totalSlots;
            float remainingSpace = panelWidth - totalSlotWidth;
            float gap = remainingSpace / (totalSlots + 1); // equal margins + gaps

            float slotLeft = gap + index * (slotSize + gap);

            var label = new UIText(labelText)
            {
                TextColor = Color.White
            };

            // Center label text over the slot using its own measured width.
            float labelWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(labelText).X;
            label.Left.Set(slotLeft + (slotSize - labelWidth) / 2f, 0f);
            label.Top.Set(45f, 0f);
            panel.Append(label);

            var slot = new UIMaterialSlot(onSelected);
            slot.Left.Set(slotLeft, 0f);
            slot.Top.Set(68f, 0f);
            panel.Append(slot);

            return (slot, label);
        }

        private void HideUI()
        {
            Terraria.ModLoader.ModContent
                .GetInstance<InstantBaseUISystem>()
                .HideUI();
        }

        private void OnFrameMaterialSelected(Item item) => currentItem?.SetFrameMaterial(item);
        private void OnWallMaterialSelected(Item item) => currentItem?.SetWallMaterial(item);
        private void OnPlatformMaterialSelected(Item item) => currentItem?.SetPlatformMaterial(item);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (panel != null && panel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
                PlayerInput.WritingText = false;
            }
        }
    }
}