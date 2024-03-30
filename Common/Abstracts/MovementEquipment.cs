using Terraria;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Common.Abstracts;

public abstract class MovementEquipment : ModItem
{
    public abstract int CooldownTime { get; }

    public virtual void OnGrapple(Player player) { }
}
