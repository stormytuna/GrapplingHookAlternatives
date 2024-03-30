using Terraria;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Content;

public class FakeHookProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Item_0";

    public override void SetStaticDefaults()
    {
        Main.projHook[Type] = true;
    }
}