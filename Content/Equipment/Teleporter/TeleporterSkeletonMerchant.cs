namespace GrapplingHookAlternatives.Content.Equipment.Teleporter;

public class TeleporterSkeletonMerchant : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.SkeletonMerchant;

	public override void ModifyShop(NPCShop shop) {
		shop.Add<Teleporter>(Condition.MoonPhaseNew, Condition.Hardmode);
	}
}
