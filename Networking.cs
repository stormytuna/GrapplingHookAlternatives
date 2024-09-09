using System.IO;
using GrapplingHookAlternatives.Content.Equipment.Teleporter;

namespace GrapplingHookAlternatives;

public enum PacketType : byte
{
	TeleporterStartTeleportSync
}

public partial class GrapplingHookAlternatives : Mod
{
	public override void HandlePacket(BinaryReader reader, int whoAmI) {
		PacketType packetType = (PacketType)reader.ReadByte();
		switch (packetType) {
			case PacketType.TeleporterStartTeleportSync:
				TeleporterPlayer.HandleStartTeleportSync(reader, whoAmI);
				break;
			default:
				Logger.Warn($"Unknown message type: {packetType}");
				break;
		}
	}
}
