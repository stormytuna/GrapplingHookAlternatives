using System.IO;
using GrapplingHookAlternatives.Common.Loaders;
using GrapplingHookAlternatives.Common.RenderTargets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GrapplingHookAlternatives.Content.Equipment.Teleporter;

public class TeleporterPlayer : ModPlayer
{
	private const int TeleportTimerMax = 20;

	private static readonly SoundStyle teleportSound = new($"{nameof(GrapplingHookAlternatives)}/Assets/Sounds/teleport") {
		PitchVariance = 0.1f,
		MaxInstances = 0
	};

	private Vector2 teleportPosition;
	private int timer;

	public void BeginTeleporting(Vector2 teleportPosition) {
		if (timer <= 0) {
			timer = TeleportTimerMax;
			this.teleportPosition = teleportPosition;
			SoundEngine.PlaySound(teleportSound, Player.Center);

			if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
				SendTeleporterStartTeleportSync(Player.whoAmI, teleportPosition);
			}
		}
	}

	public override void UpdateEquips() {
		if (timer == TeleportTimerMax / 2) {
			Vector2 oldPosition = Player.position;
			Player.Teleport(teleportPosition, -1);
			teleportPosition = oldPosition;

			for (int i = 0; i < 4; i++) {
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
			}
		}

		if (timer > 0) {
			Lighting.AddLight(Player.Center, Color.LimeGreen.ToVector3() * 0.5f);
			Lighting.AddLight(teleportPosition, Color.LimeGreen.ToVector3() * 0.5f);

			if (Main.rand.NextBool()) {
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
			}
		}

		timer--;
	}

	public override void HideDrawLayers(PlayerDrawSet drawInfo) {
		if (!PlayerRenderTarget.canUseTarget || timer <= 0) {
			return;
		}

		foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
			layer.Hide();
		}
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (!PlayerRenderTarget.canUseTarget || timer <= 0) {
			return;
		}


		int fadeInOutFrames = 5;
		float opacity = 1f;
		if (timer >= TeleportTimerMax - fadeInOutFrames) {
			opacity = MathHelper.Lerp(0, 1f, (TeleportTimerMax - timer) / (float)fadeInOutFrames);
		}
		else if (timer <= fadeInOutFrames) {
			opacity = MathHelper.Lerp(0, 1f, timer / (float)fadeInOutFrames);
		}

		Vector2 position = PlayerRenderTarget.getPlayerTargetPosition(drawInfo.drawPlayer.whoAmI);
		Rectangle sourceRect = PlayerRenderTarget.getPlayerTargetSourceRectangle(drawInfo.drawPlayer.whoAmI);
		Vector2 teleportOffset = drawInfo.Position - teleportPosition;

		ShaderLoader.TeleporterShader.UseOpacity(opacity);
		ShaderLoader.TeleporterShader.Apply();
		Main.spriteBatch.Draw(PlayerRenderTarget.Target, position, sourceRect, Color.White);
		Main.spriteBatch.Draw(PlayerRenderTarget.Target, position - teleportOffset, sourceRect, Color.White);
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	}

	public static void HandleStartTeleportSync(BinaryReader reader, int whoAmI) {
		int player = reader.Read7BitEncodedInt();
		if (Main.netMode == NetmodeID.Server) {
			player = whoAmI;
		}

		Vector2 teleportPos = reader.ReadVector2();
		if (player != Main.myPlayer) {
			Main.player[player].GetModPlayer<TeleporterPlayer>().BeginTeleporting(teleportPos);
		}

		if (Main.netMode == NetmodeID.Server) {
			SendTeleporterStartTeleportSync(player, teleportPos);
		}
	}

	private static void SendTeleporterStartTeleportSync(int player, Vector2 teleportPos) {
		ModPacket packet = GrapplingHookAlternatives.Instance.GetPacket();
		packet.Write((byte)PacketType.TeleporterStartTeleportSync);
		packet.Write7BitEncodedInt(player);
		packet.WriteVector2(teleportPos);
		packet.Send(ignoreClient: player);
	}
}
