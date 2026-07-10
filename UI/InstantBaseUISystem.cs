using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using InstantBase.Items;

namespace InstantBase.UI
{
    public class InstantBaseUISystem : ModSystem
    {
        internal InstantBaseUI BaseUI;
        private UserInterface _userInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                BaseUI = new InstantBaseUI();

                _userInterface = new UserInterface();
                _userInterface.SetState(null);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                _userInterface.SetState(null);
            }
            
            _userInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(
                layer => layer.Name.Equals("Vanilla: Inventory")
            );

            if (inventoryIndex != -1)
            {
                layers.Insert(
                    inventoryIndex,
                    new LegacyGameInterfaceLayer(
                        "InstantBase: UI",
                        delegate
                        {
                            _userInterface.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI
                    )
                );
            }
        }

        public void ShowUI(InstantBaseItem item)
        {
            BaseUI.Open(item);
            _userInterface.SetState(BaseUI);
        }

        public void HideUI()
        {
            _userInterface.SetState(null);
        }
    }
}