using GrapplingHookAlternatives.Common.Loaders;
using GrapplingHookAlternatives.Common.RenderTargets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

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
		}
	}

	public override void UpdateEquips() {
		if (timer == TeleportTimerMax / 2) {
			Vector2 oldPosition = Player.position;
			Player.Teleport(teleportPosition, -1);
			teleportPosition = oldPosition; // Hack to draw the player where we were, TODO: Make this not hacky? i guess?

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

	public override void Load() {
		// TODO: Move into IL edit to prevent mod compat issues
		On_LegacyPlayerRenderer.DrawPlayerInternal += static (orig, self, camera, player, position, rotation, origin, shadow, alpha, scale, only) => {
			int timer = player.GetModPlayer<TeleporterPlayer>().timer;
			if (timer <= 0 || PlayerRenderTarget.ForceDrawPlayerHack) {
				orig(self, camera, player, position, rotation, origin, shadow, alpha, scale, only);
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

			ShaderLoader.TeleporterShader.UseOpacity(opacity);
			ShaderLoader.TeleporterShader.Apply();
			Main.spriteBatch.Draw(PlayerRenderTarget.Target, Vector2.Zero, null, Color.White);
			Main.spriteBatch.Draw(PlayerRenderTarget.Target, player.GetModPlayer<TeleporterPlayer>().teleportPosition - player.position + new Vector2(0f, 8f * player.gravDir), null, Color.White);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		};
	}
}
