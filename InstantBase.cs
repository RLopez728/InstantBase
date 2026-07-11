using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using InstantBase.Structures;

namespace InstantBase
{
    public class InstantBase : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            InstantBaseMessageType messageType = (InstantBaseMessageType)reader.ReadByte();

            switch (messageType)
            {
                case InstantBaseMessageType.BuildRequest:
                    HandleBuildRequest(reader, whoAmI);
                    break;

                case InstantBaseMessageType.UndoRequest:
                    HandleUndoRequest(whoAmI);
                    break;
            }
        }

        private void HandleBuildRequest(BinaryReader reader, int whoAmI)
        {
            int tileX = reader.ReadInt32();
            int tileY = reader.ReadInt32();
            ushort frameTile = reader.ReadUInt16();
            ushort wallType = reader.ReadUInt16();
            ushort platformTile = reader.ReadUInt16();
            int platformStyle = reader.ReadInt32();

            if (Main.netMode != NetmodeID.Server)
                return;

            var materials = new BuildMaterials
            {
                FrameTile = frameTile,
                WallType = wallType,
                PlatformTile = platformTile,
                PlatformStyle = platformStyle
            };

            var origin = new Point(tileX, tileY);

            StructureBuilder.Build(origin, materials, whoAmI);

            var (width, height) = StructureBuilder.GetStructureBounds();

            NetMessage.SendTileSquare(-1, tileX, tileY, width, height);
        }

        private void HandleUndoRequest(int whoAmI)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            if (StructureBuilder.Undo(whoAmI, out Point origin, out int width, out int height))
            {
                NetMessage.SendTileSquare(-1, origin.X, origin.Y, width, height);
            }
        }
    }
}