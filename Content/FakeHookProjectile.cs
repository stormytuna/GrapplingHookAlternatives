namespace GrapplingHookAlternatives.Content;

public class FakeHookProjectile : ModProjectile
{
	// Required just so our equipment have `Main.projHook[item.shoot]` set to true
	public override string Texture => "Terraria/Images/Item_0";

	public override void SetStaticDefaults() {
		Main.projHook[Type] = true;
	}
}
