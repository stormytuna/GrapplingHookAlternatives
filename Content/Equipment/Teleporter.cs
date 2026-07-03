using System.Collections.Generic;
using System.IO;
using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using GrapplingHookAlternatives.Common.Loaders;
using GrapplingHookAlternatives.Common.RenderTargets;
using GrapplingHookAlternatives.Interfaces;
using Terraria.Audio;
using Terraria.DataStructures;

namespace GrapplingHookAlternatives.Content.Equipment;

public class Teleporter : ModItem, IMovementEquipment
{
	private const int TeleportBoxWidth = 16;
	private const int TeleportBoxHeight = 12;
	private const int TeleportBoxHorizontalOffset = 25;
	private const int TeleportBoxVerticalOffset = 20;

	public int CooldownTime => 0; //5 * 60;

    public bool RequiresOnGround => true;

	public override void SetDefaults() {
		Item.width = 14;
		Item.height = 34;
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 15);

		Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
	}

	[GlobalNPCHooks.ModifyShop]
	public void ModifyShop(NPCShop shop) {
		if (shop.NpcType == NPCID.SkeletonMerchant) {
			shop.Add(Type, Condition.Hardmode);
		}
	}

	public void OnGrapple(Player player) {
		int teleportBoxWidth = 16;
		int teleportBoxHeight = 12;

		Point teleportBoxOffset = Point.Zero;
		if (player.controlLeft) {
			teleportBoxOffset.X -= TeleportBoxHorizontalOffset;
		}
		if (player.controlRight) {
			teleportBoxOffset.X += TeleportBoxHorizontalOffset;
		}
		if (player.controlUp) {
			teleportBoxOffset.Y -= TeleportBoxVerticalOffset;
		}
		if (player.controlDown) {
			teleportBoxOffset.Y += TeleportBoxVerticalOffset;
		}

		if (teleportBoxOffset == Point.Zero) {
			teleportBoxOffset = new Point(TeleportBoxHorizontalOffset * player.direction, 0);
		}

		Point teleportBoxTopLeft = player.Center.ToTileCoordinates() + teleportBoxOffset - new Point(TeleportBoxWidth / 2, TeleportBoxHeight / 2);
		teleportBoxTopLeft = teleportBoxTopLeft.Clamp(0, Main.maxTilesX, 0, Main.maxTilesY);

		// Width -1 and height -2 so we don't query tiles outside of our box
		List<Point> validPositions = new();
		List<Point> goodPositions = new();
		for (int i = 0; i < teleportBoxWidth - 1; i++) {
			for (int j = 0; j < teleportBoxHeight - 2; j++) {
				Point tilePosition = teleportBoxTopLeft + new Point(i, j);

				bool emptyAir = !Collision.SolidTilesVersatile(tilePosition.X, tilePosition.X + 1, tilePosition.Y, tilePosition.Y + 2);
				bool solidFloor = WorldGen.SolidTile2(tilePosition.X, tilePosition.Y + 3) && WorldGen.SolidTile2(tilePosition.X + 1, tilePosition.Y + 3);
				if (emptyAir && solidFloor) {
					goodPositions.Add(tilePosition);
				}
				else if (emptyAir && !solidFloor) {
					validPositions.Add(tilePosition);
				}
			}
		}

		Point chosenPosition;
		if (goodPositions.Count == 0 && validPositions.Count == 0) {
			chosenPosition = teleportBoxTopLeft + new Point(teleportBoxWidth / 2, teleportBoxHeight / 2);
			player.GetModPlayer<TeleporterPlayer>().KillMeOnTeleport = true;
			//return;
		} else {
			chosenPosition = goodPositions.Count > 0 ? Main.rand.Next(goodPositions) : Main.rand.Next(validPositions);
		}

		Vector2 teleportPosition = chosenPosition.ToVector2() * 16f;
		player.GetModPlayer<TeleporterPlayer>().BeginTeleporting(teleportPosition);
	}
}

public class TeleporterDust : ModDust
{
	public override void OnSpawn(Dust dust) {
		dust.frame = new Rectangle(0, Main.rand.Next(4) * 18, 18, 18);
		dust.velocity *= Main.rand.NextFloat(0.1f, 0.2f);
		dust.noGravity = true;
		dust.noLight = true;
		dust.scale = Main.rand.NextFloat(0.3f, 1f);
	}

	public override bool Update(Dust dust) {
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.15f;
		dust.scale *= 0.97f;

		if (dust.scale < 0.2f) {
			dust.active = false;
		}

		return false;
	}
}

public class TeleporterPlayer : ModPlayer
{
	private const int TeleportTimerMax = 20;

	private static readonly SoundStyle teleportSound = new($"{nameof(GrapplingHookAlternatives)}/Assets/Sounds/teleport") {
		PitchVariance = 0.1f,
		MaxInstances = 0
	};

	public bool KillMeOnTeleport = false;

	private Vector2 _teleportPosition;
	private int _teleportTimer;

	public void BeginTeleporting(Vector2 teleportPosition) {
		if (_teleportTimer <= 0) {
			_teleportTimer = TeleportTimerMax;
			_teleportPosition = teleportPosition;
			SoundEngine.PlaySound(teleportSound, Player.Center);

			if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
				SendTeleporterStartTeleportSync(Player.whoAmI, teleportPosition);
			}
		}
	}

	public override void UpdateEquips() {
		if (_teleportTimer == TeleportTimerMax / 2) {
			if (KillMeOnTeleport) {
				KillMeOnTeleport = false;

				// One of rod of discord's death messages
				Player.KillMe(PlayerDeathReason.ByOther(Main.rand.Next(13, 15)), 1, 0);
				_teleportTimer = 0;
				return;
			}

			Vector2 oldPosition = Player.position;
			Player.Teleport(_teleportPosition, -1);
			_teleportPosition = oldPosition;

			for (int i = 0; i < 4; i++) {
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
			}
		}

		if (_teleportTimer > 0) {
			Lighting.AddLight(Player.Center, Color.LimeGreen.ToVector3() * 0.5f);
			Lighting.AddLight(_teleportPosition, Color.LimeGreen.ToVector3() * 0.5f);

			if (Main.rand.NextBool()) {
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
			}
		}

		_teleportTimer--;
	}

	public override void HideDrawLayers(PlayerDrawSet drawInfo) {
		if (!PlayerRenderTarget.canUseTarget || _teleportTimer <= 0) {
			return;
		}

		foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
			layer.Hide();
		}
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (!PlayerRenderTarget.canUseTarget || _teleportTimer <= 0) {
			return;
		}

		int fadeInOutFrames = 5;
		float intensity = 1f;
		if (_teleportTimer >= TeleportTimerMax - fadeInOutFrames) {
			intensity = MathHelper.Lerp(0, 1f, (TeleportTimerMax - _teleportTimer) / (float)fadeInOutFrames);
		}
		else if (_teleportTimer <= fadeInOutFrames) {
			intensity = MathHelper.Lerp(0, 1f, _teleportTimer / (float)fadeInOutFrames);
		}

		Main.spriteBatch.End(out var snapshot);

		var shader = Assets.TeleporterShader.Value;
		shader.Parameters["intensity"].SetValue((intensity) / Main.CurrentFrameFlags.ActivePlayersCount);
		shader.Parameters["opacity"].SetValue(intensity);
		shader.Parameters["brightness"].SetValue(intensity * 1.8f);
		shader.Parameters["textureSize"].SetValue(PlayerRenderTarget.Target.Size());
		shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);

		Main.graphics.GraphicsDevice.Textures[1] = Assets.Noise01.Value;

		Main.spriteBatch.Begin(snapshot with { CustomEffect = shader });

		Vector2 position = PlayerRenderTarget.getPlayerTargetPosition(drawInfo.drawPlayer.whoAmI);
		Rectangle sourceRect = PlayerRenderTarget.getPlayerTargetSourceRectangle(drawInfo.drawPlayer.whoAmI);
		Vector2 teleportOffset = drawInfo.Position - _teleportPosition;
		Main.spriteBatch.Draw(PlayerRenderTarget.Target, position, sourceRect, Color.White);
		Main.spriteBatch.Draw(PlayerRenderTarget.Target, position - teleportOffset, sourceRect, Color.White);

		Main.spriteBatch.Restart(snapshot);
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
