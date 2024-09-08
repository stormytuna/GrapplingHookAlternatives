using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Content.Equipment.Teleporter;

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
