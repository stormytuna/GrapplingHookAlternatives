using System.Linq;
using GrapplingHookAlternatives.Interfaces;
using Daybreak.Common.Features.Hooks;

namespace GrapplingHookAlternatives.Content.Equipment;

public class WhaleboneRune : ModItem, IMovementEquipment
{
    public override string Texture => "Terraria/Images/Item_50";

	public int CooldownTime => 0; //5 * 60;

    public bool RequiresOnGround => true;

    public override void SetDefaults() {
		Item.width = 22;
		Item.height = 22;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(gold: 1);

		Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
	}

	public void OnGrapple(Player player) {
		player.GetModPlayer<WhaleboneRunePlayer>().HoldingBlink = true;
	}

	[GlobalNPCHooks.ModifyShop]
	public void ModifyShop(NPCShop shop) {
		if (shop.NpcType == NPCID.Steampunker) {
			shop.Add(Type, Condition.DownedMechBossAll);
		}
	}
}

public class WhaleboneRunePlayer : ModPlayer
{
	public bool HoldingBlink;

	private Vector2 BlinkPoint {
		get {
			float maxBlinkDistance = 21f * 16f;
			float[] laserScanResults = new float[3];
			Collision.LaserScan(Player.Center, Player.DirectionTo(Main.MouseWorld), 0f, maxBlinkDistance, laserScanResults);
			float length = laserScanResults.Average();
			// TODO: Can clip into the floor lol
			return Player.Center + (Player.DirectionTo(Main.MouseWorld) * (length - 16f));
		}
	}

    public override void PostUpdateMiscEffects() {
		if (!HoldingBlink) {
			return;
		}

		Vector2 blinkPoint = BlinkPoint;
		if (HoldingBlink && Player.controlHook) {
			for (int i = 0; i < 1; i++) {
				Dust.NewDustPerfect(BlinkPoint, DustID.BlueFairy);
			}

			return;
		}

		HoldingBlink = false;
		Player.Teleport(blinkPoint);
    }
}
