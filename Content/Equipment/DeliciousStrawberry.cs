using System.Collections.Generic;
using GrapplingHookAlternatives.Common.Loaders;
using GrapplingHookAlternatives.Common.RenderTargets;
using GrapplingHookAlternatives.Interfaces;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GrapplingHookAlternatives.Content.Equipment;

public class DeliciousStrawberry : ModItem, IMovementEquipment
{
	public int CooldownTime => 1 * 60;

    public bool RequiresOnGround => true;

	public override void SetDefaults() {
		Item.width = 22;
		Item.height = 22;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(gold: 1);

		Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
	}

	public void OnGrapple(Player player) {
		Vector2 dashDirection = Vector2.Zero;
		if (player.controlLeft) {
			dashDirection.X = -1f;
		}
		if (player.controlRight) {
			dashDirection.X = 1f;
		}
		if (player.controlUp) {
			dashDirection.Y = -1f;
		}
		if (player.controlDown) {
			dashDirection.Y = 1f;
		}

		if (dashDirection == Vector2.Zero) {
			dashDirection = new Vector2(player.direction, 0);
		}

		player.GetModPlayer<DeliciousStrawberryPlayer>().BeginDash(dashDirection);
	}
}

public class DeliciousStrawberryPlayer : ModPlayer
{
	private const int DashDuration = 20;
	private const int DashTimerToMakeAfterimages = -40;
	private const int AfterimageLifetime = 20;

	internal class Afterimage 
	{
		internal Vector2 position; 		
		internal int timeActive = 0;

		internal Afterimage(Vector2 position) {
			this.position = position;
		}
	}

	private Vector2 _dashDirection;
	private int _dashTimer = DashTimerToMakeAfterimages - 1;
	private Queue<Afterimage> _afterimages = new();

	private SoundStyle _dashSound = new SoundStyle($"{nameof(GrapplingHookAlternatives)}/Assets/Sounds/strawberrydash0") {
		MaxInstances = 0,
		PitchRange = (-0.2f, 0.2f),
		Variants = [1, 2],
	};

	public void BeginDash(Vector2 dashDirection) {
		_dashDirection = dashDirection.SafeNormalize(Vector2.Zero);
		_dashTimer = DashDuration;

		SoundEngine.PlaySound(_dashSound, Player.Center);
	}

    public override void UpdateEquips() {
		if (_dashTimer > 0) {
			Player.velocity = _dashDirection * 10f;

		}

		// Looks nicer if afterimages last a little longer than actual velocity change
		if (_dashTimer > DashTimerToMakeAfterimages && _dashTimer % 5 == 4) {
			_afterimages.Enqueue(new Afterimage(Player.oldPosition));
		}

		bool dequeue = false;
		foreach (var afterimage in _afterimages) {
			afterimage.timeActive++;
			if (afterimage.timeActive >= AfterimageLifetime) {
				dequeue = true;
			}
		}

		if (dequeue) {
			_afterimages.Dequeue();
		}

		_dashTimer--;
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
		if (!PlayerRenderTarget.canUseTarget || _dashTimer <= 0) {
			return;
		}

		foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
			layer.Hide();
		}
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (!PlayerRenderTarget.canUseTarget || _afterimages.Count <= 0) {
			return;
		}

		ShaderLoader.DeliciousStrawberryShader.Apply();

		Vector2 position = PlayerRenderTarget.getPlayerTargetPosition(drawInfo.drawPlayer.whoAmI);
		Rectangle sourceRect = PlayerRenderTarget.getPlayerTargetSourceRectangle(drawInfo.drawPlayer.whoAmI);
		foreach (var afterimage in _afterimages) {
			Vector2 drawOffset = drawInfo.Position - afterimage.position;
			float opacity = Utils.Remap(afterimage.timeActive, AfterimageLifetime, 0, 0f, 0.8f, true);
			ShaderLoader.DeliciousStrawberryShader.UseOpacity(opacity);
			Main.spriteBatch.Draw(PlayerRenderTarget.Target, position - drawOffset, sourceRect, Color.White);
		}

		Main.pixelShader.CurrentTechnique.Passes[0].Apply();

		Main.spriteBatch.Draw(PlayerRenderTarget.Target, position, sourceRect, Color.White);
    }
}
