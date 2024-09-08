using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Common.RenderTargets;

// TODO: Fix in MP!
public class PlayerRenderTarget : ILoadable
{
	public static RenderTarget2D Target;
	public static bool ForceDrawPlayerHack;

	public void Load(Mod mod) {
		Main.QueueMainThreadAction(() => { Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight); });

		// Not sure why this method specifically, SLRs player RT uses it and I couldn't get it working without using this
		On_Main.CheckMonoliths += static orig => {
			orig();

			if (Main.gameMenu) {
				return;
			}

			for (int i = 0; i < Main.maxPlayers; i++) {
				if (Main.player[i].active) {
					break;
				}

				if (i == Main.maxPlayers - 1) {
					return;
				}
			}

			RenderTargetBinding[] oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

			Main.graphics.GraphicsDevice.SetRenderTarget(Target);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active) {
					continue;
				}

				ForceDrawPlayerHack = true;
				Main.PlayerRenderer.DrawPlayer(Main.Camera, player, player.position, player.fullRotation, player.fullRotationOrigin);
				ForceDrawPlayerHack = false;
			}

			Main.spriteBatch.End();

			Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
		};

		On_Main.SetDisplayMode += (orig, width, height, fullscreen) => {
			orig(width, height, fullscreen);

			// Need to reset RT width and height when window size changes
			Target.Dispose();
			Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
		};
	}

	public void Unload() {
		Main.QueueMainThreadAction(() => { Target.Dispose(); });
	}
}
