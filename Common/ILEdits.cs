using System;
using GrapplingHookAlternatives.Common.Abstracts;
using GrapplingHookAlternatives.Common.Players;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Common;

public class ILEdits : ILoadable
{
	public void Load(Mod mod) {
		IL_Player.QuickGrapple += il => {
			ILCursor cursor = new(il);

			Func<Instruction, bool>[] matches = {
				i => i.MatchCall<Player>(nameof(Player.QuickGrapple_GetItemToUse)),
				i => i.MatchStloc0(),
				i => i.MatchLdloc0(),
				i => i.MatchBrfalse(out _)
			};
			if (!cursor.TryGotoNext(MoveType.After, matches)) {
				MonoModHooks.DumpIL(mod, il);
				throw new ILPatchFailureException(mod, il, new Exception($"Failed to find IL entrypoint for {nameof(IL_Player.QuickGrapple)}!"));
			}

			cursor.Emit(OpCodes.Ldloc_0);
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Action<Item, Player>>((item, player) => {
				if (item.ModItem is not MovementEquipment movementEquipment || player.GetModPlayer<MovementEquipmentPlayer>().OnCooldown) {
					return;
				}

				movementEquipment.OnGrapple(player);
				player.GetModPlayer<MovementEquipmentPlayer>().BeginCooldown(movementEquipment.CooldownTime);
			});
		};
	}

	// Nothing to do as our patches are unloaded for us
	public void Unload() { }
}
