using Terraria.Audio;

namespace GrapplingHookAlternatives.Common.Players;

public class MovementEquipmentPlayer : ModPlayer
{
	public int movementEquipmentCooldown;

	public bool OffCooldown => movementEquipmentCooldown <= 0;

	public bool OnCooldown => movementEquipmentCooldown > 0;

	public void BeginCooldown(int cooldownTime) {
		if (movementEquipmentCooldown < cooldownTime) {
			movementEquipmentCooldown = cooldownTime;
		}
	}

	public override void PostUpdateMiscEffects() {
		movementEquipmentCooldown--;

		if (movementEquipmentCooldown == 0) {
			SoundEngine.PlaySound(SoundID.MaxMana);
			for (int i = 0; i < 5; i++) {
				Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.ManaRegeneration);
				dust.alpha = 255;
				dust.scale = Main.rand.NextFloat(2, 2.6f);
				dust.noLight = true;
				dust.noGravity = true;
				dust.velocity *= 0.5f;
			}
		}
	}
}
