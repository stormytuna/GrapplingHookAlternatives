using System.Collections.Generic;
using GrapplingHookAlternatives.Interfaces;
using Terraria.DataStructures;

namespace GrapplingHookAlternatives.Content.Equipment.Teleporter;

public class Teleporter : ModItem, IMovementEquipment
{
	private const int TeleportBoxWidth = 16;
	private const int TeleportBoxHeight = 12;
	private const int TeleportBoxHorizontalOffset = 25;
	private const int TeleportBoxVerticalOffset = 20;

	public int CooldownTime => 0 * 60;

	public override void SetDefaults() {
		Item.width = 14;
		Item.height = 34;
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 15);

		Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
	}

	public void OnGrapple(Player player) {
		// TODO: Visualise and finalise this!
		int teleportBoxWidth = 16;
		int teleportBoxHeight = 12;

		Point teleportBoxOffset;
		if (player.controlLeft) {
			teleportBoxOffset = new Point(-TeleportBoxHorizontalOffset, 0);
		}
		else if (player.controlRight) {
			teleportBoxOffset = new Point(TeleportBoxHorizontalOffset, 0);
		}
		else if (player.controlUp) {
			teleportBoxOffset = new Point(0, -TeleportBoxVerticalOffset);
		}
		else if (player.controlDown) {
			teleportBoxOffset = new Point(0, TeleportBoxVerticalOffset);
		}
		else {
			teleportBoxOffset = new Point(TeleportBoxHorizontalOffset * player.direction, 0);
		}

		Point teleportBoxTopLeft = player.Center.ToTileCoordinates() + teleportBoxOffset - new Point(TeleportBoxWidth / 2, TeleportBoxHeight / 2);
		teleportBoxTopLeft = teleportBoxTopLeft.Clamp(0, Main.maxTilesX, 0, Main.maxTilesY);

		// width -1 and height -1 so we don't query tiles outside of our box
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

		// TODO: This glitches and places camera in hell for some reason, figure out why!
		if (goodPositions.Count == 0 && validPositions.Count == 0) {
			Point randomPoint = new(Main.rand.Next(teleportBoxTopLeft.X, teleportBoxTopLeft.X + teleportBoxWidth), Main.rand.Next(teleportBoxTopLeft.Y, teleportBoxTopLeft.X + teleportBoxHeight));
			Vector2 randomPosition = randomPoint.ToVector2() * 16f;
			player.Teleport(randomPosition);
			// One of rod of discord's death messages
			player.KillMe(PlayerDeathReason.ByOther(Main.rand.Next(13, 15)), 1, 0);
			return;
		}

		Point chosenPosition = goodPositions.Count > 0 ? Main.rand.Next(goodPositions) : Main.rand.Next(validPositions);
		Vector2 teleportPosition = chosenPosition.ToVector2() * 16f;
		player.GetModPlayer<TeleporterPlayer>().BeginTeleporting(teleportPosition);
	}
}
