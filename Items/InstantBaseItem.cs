using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Text;
using InstantBase.Structures;
using InstantBase.UI;

namespace InstantBase.Items
{
    public class InstantBaseItem : ModItem
    {
        private string[] frameBlueprint;
        private string[] wallBlueprint;
        private string[] foregroundBlueprint;

        private BuildMaterials materials = new BuildMaterials()
        {
            FrameTile = TileID.GrayBrick,
            WallType = WallID.GrayBrick,
            PlatformTile = TileID.Platforms
        };

        private Item frameSelectionItem = new Item();
        private Item wallSelectionItem = new Item();
        private Item platformSelectionItem = new Item();

        private ushort? houseEnablerTileId;

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

            Item.maxStack = 1;
            Item.consumable = false;

            Item.rare = ItemRarityID.White;
            Item.value = Item.buyPrice(silver:50);

            Item.UseSound = SoundID.Item1;

            frameSelectionItem.TurnToAir();
            wallSelectionItem.TurnToAir();
            platformSelectionItem.TurnToAir();
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                ModContent.GetInstance<InstantBaseUISystem>().ShowUI(this);
                return true;
            }

            frameBlueprint ??= LoadBlueprint("Assets/Structures/Frame.txt");
            wallBlueprint ??= LoadBlueprint("Assets/Structures/Walls.txt");
            foregroundBlueprint ??= LoadBlueprint("Assets/Structures/Foreground.txt");

            if (frameBlueprint == null)
                {
                    Main.NewText("Frame blueprint is NULL!");
                    return false;
                }

            if (wallBlueprint == null)
                {
                    Main.NewText("Wall blueprint is NULL!");
                    return false;
                }

            if (foregroundBlueprint == null)
                {
                    Main.NewText("Foreground blueprint is NULL!");
                    return false;
                }

            Point tilePosition = Main.MouseWorld.ToTileCoordinates();

            var (structureWidth, structureHeight) = GetStructureBounds();

            ClearArea(
                tilePosition.X,
                tilePosition.Y
            );

            PlaceFrame(tilePosition);
            PlaceWalls(tilePosition);
            PlaceForeground(tilePosition);

            WorldGen.RangeFrame(
                tilePosition.X,
                tilePosition.Y,
                tilePosition.X + structureWidth,
                tilePosition.Y + structureHeight
            );

            WorldGen.RangeFrame(
                tilePosition.X,
                tilePosition.Y,
                tilePosition.X + frameBlueprint[0].Length,
                tilePosition.Y + frameBlueprint.Length
            );

            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                return true;
            }

            return base.CanUseItem(player);
        }

        private string[] LoadBlueprint(String path)
        {
            byte[] bytes = Mod.GetFileBytes(path);

            if (bytes == null)
                throw new Exception($"Couldn't find blueprint: {path}");

            string text = Encoding.UTF8.GetString(bytes);

            string[] lines = text
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            int maxWidth = lines.Max(line => line.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < maxWidth)
                {
                    lines[i] = lines[i].PadRight(maxWidth, ' ');
                }
            }

            return lines;
        }

        private (int width, int height) GetStructureBounds()
        {
            int width = new[] { frameBlueprint[0].Length, wallBlueprint[0].Length, foregroundBlueprint[0].Length }.Max();
            int height = new[] { frameBlueprint.Length, wallBlueprint.Length, foregroundBlueprint.Length }.Max();

            return (width, height);
        }

        private void PlaceFrame(Point origin)
        {
            for (int y = 0; y < frameBlueprint.Length; y++)
            {
                for (int x = 0; x < frameBlueprint[y].Length; x++)
                {
                    if (frameBlueprint[y][x] == '#')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            materials.FrameTile
                        );
                    }
                }
            }
        }

        private void PlaceWalls(Point origin)
        {
            for (int y = 0; y < wallBlueprint.Length; y++)
            {
                for (int x = 0; x < wallBlueprint[y].Length; x++)
                {
                    if (wallBlueprint[y][x] == 'W')
                    {
                        int wallX = origin.X + x;
                        int wallY = origin.Y + y;

                        Main.tile[wallX, wallY].WallType = materials.WallType;

                        WorldGen.SquareWallFrame(wallX, wallY);
                    }
                }
            }
        }

        private void PlaceForeground(Point origin)
        {
            for (int y = 0; y < foregroundBlueprint.Length; y++)
            {
                for (int x = 0; x < foregroundBlueprint[y].Length; x++)
                {
                    if (foregroundBlueprint[y][x] == 'E')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            GetHouseEnablerTileId()
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'P')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            materials.PlatformTile
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'H')
                    {
                        PlaceRightSlopePlatform(
                            origin.X + x,
                            origin.Y + y
                        );
                    }
                    else if (foregroundBlueprint[y][x] == 'R')
                    {
                        WorldGen.PlaceTile(
                            origin.X + x,
                            origin.Y + y,
                            TileID.Rope
                        );
                    }
                }
            }
        }

        private ushort GetHouseEnablerTileId()
        {
            if (houseEnablerTileId.HasValue)
                return houseEnablerTileId.Value;

            if (ModLoader.TryGetMod("miningcracks_take_on_luiafk", out Mod luiAfk))
            {
                ModItem match = luiAfk.GetContent<ModItem>()
                .FirstOrDefault(modItem =>
                    modItem.DisplayName.Value.Equals("Unlimited House Enabler")
                );

                if (match != null && match.Item.createTile >= 0)
                {
                    houseEnablerTileId = (ushort)match.Item.createTile;
                    return houseEnablerTileId.Value;
                }
            }

            Main.NewText("LuiAFK Reborn's Unlimited House Enabler not found - falling back to torches.");
            houseEnablerTileId = TileID.Torches;
            return houseEnablerTileId.Value;
        }

        private void PlaceRightSlopePlatform(int x, int y)
        {
            WorldGen.PlaceTile(
                x,
                y,
                materials.PlatformTile
            );

            Tile tile = Main.tile[x, y];

            tile.Slope = SlopeType.SlopeDownLeft;
            tile.IsHalfBlock = false;

            WorldGen.SquareTileFrame(x, y);
        }

        public void SetFrameMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.FrameTile = TileID.GrayBrick;
                frameSelectionItem = new Item();
                frameSelectionItem.TurnToAir();
                return;
            }

            if (item.createTile < 0)
                return;

            materials.FrameTile = (ushort)item.createTile;
            frameSelectionItem = item.Clone();
        }

        public Item GetFrameSelectionItem() => frameSelectionItem;

        public void SetWallMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.WallType = WallID.GrayBrick;
                wallSelectionItem = new Item();
                wallSelectionItem.TurnToAir();
                return;
            }

            if (item.createWall < 0)
                return;

            materials.WallType = (ushort)item.createWall;
            wallSelectionItem = item.Clone();
        }

        public Item GetWallSelectionItem() => wallSelectionItem;

        public void SetPlatformMaterial(Item item)
        {
            if (item == null || item.IsAir)
            {
                materials.PlatformTile = TileID.Platforms;
                platformSelectionItem = new Item();
                platformSelectionItem.TurnToAir();
                return;
            }

            if (item.createTile < 0)
                return;

            materials.PlatformTile = (ushort)item.createTile;
            platformSelectionItem = item.Clone();
        }

        public Item GetPlatformSelectionItem() => platformSelectionItem;

        private void ClearArea(int startX, int startY)
        {
            int width = GetStructureBounds().width;
            int height = GetStructureBounds().height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool occupied =
                        (y < frameBlueprint.Length &&
                        x < frameBlueprint[y].Length &&
                        frameBlueprint[y][x] == '#')
                        ||
                        (y < wallBlueprint.Length &&
                        x < wallBlueprint[y].Length &&
                        wallBlueprint[y][x] == 'W')
                        ||
                        (y < foregroundBlueprint.Length &&
                        x < foregroundBlueprint[y].Length &&
                        foregroundBlueprint[y][x] != ' ');

                    if (!occupied)
                        continue;

                    int worldX = startX + x;
                    int worldY = startY + y;

                    Tile tile = Main.tile[worldX, worldY];

                    // Instantly remove tile, wall, and liquid
                    tile.ClearTile();
                    tile.WallType = WallID.None;
                    tile.LiquidAmount = 0;
                }
            }
        }
    }
}