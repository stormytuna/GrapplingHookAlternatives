using GrapplingHookAlternatives.Interfaces;

namespace GrapplingHookAlternatives.Content.Equipment.CrystallizedCloud;

public class CrystallizedCloud : ModItem, IMovementEquipment
{
	public int CooldownTime => 3 * 60;

	public override void SetDefaults() {
		Item.width = 22;
		Item.height = 22;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(gold: 1);

		Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
	}

	public void OnGrapple(Player player) {
		Vector2 launchDirection = player.DirectionFrom(Main.MouseWorld);
		float launchSpeed = 9f;
		player.velocity += launchDirection * launchSpeed;

		// Visuals
		Vector2 visualsCenter = player.Center - launchDirection * 25f;
		for (int numDust = 0; numDust < 8; numDust++) {
			Dust dust = Dust.NewDustPerfect(visualsCenter + Main.rand.NextVector2Circular(20f, 20f), DustID.Cloud);
			dust.alpha = 100;
			dust.scale = 1.5f;
			dust.velocity *= Main.rand.NextFloat(0.2f, 1f);
			dust.velocity += player.velocity * 0.15f;
		}

		for (int numClouds = 0; numClouds < 2; numClouds++) {
			Gore cloud = Gore.NewGorePerfect(player.GetSource_Misc(""), visualsCenter + Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextVector2Circular(0.4f, 0.4f), Main.rand.Next(11, 14));
			cloud.velocity += player.velocity * 0.05f;
		}
	}
}
