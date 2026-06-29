using GrapplingHookAlternatives.Interfaces;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.Audio;

namespace GrapplingHookAlternatives.Common;

public class MovementEquipmentPlayer : ModPlayer
{
	public bool OffCooldown => _movementEquipmentCooldown <= 0;

	public bool OnCooldown => _movementEquipmentCooldown > 0;

	public void BeginCooldown(int cooldownTime, bool requiresOnGround) {
		if (_movementEquipmentCooldown < cooldownTime) {
			_movementEquipmentCooldown = cooldownTime;
			_requiresOnGround = requiresOnGround;
		}
	}

	private int _movementEquipmentCooldown;
	private bool _requiresOnGround;

	public override void Load() {
		IL_Player.QuickGrapple += il => {
			ILCursor cursor = new(il);

			Func<Instruction, bool>[] matches = {
				i => i.MatchCall<Player>(nameof(Player.QuickGrapple_GetItemToUse)),
				i => i.MatchStloc0(),
				i => i.MatchLdloc0(),
				i => i.MatchBrfalse(out _)
			};
			if (!cursor.TryGotoNext(MoveType.After, matches)) {
				MonoModHooks.DumpIL(Mod, il);
				throw new ILPatchFailureException(Mod, il, new Exception($"Failed to find IL entrypoint for {nameof(IL_Player.QuickGrapple)}!"));
			}

			cursor.Emit(OpCodes.Ldloc_0);
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Action<Item, Player>>((item, player) => {
				if (item.ModItem is not IMovementEquipment movementEquipment || player.GetModPlayer<MovementEquipmentPlayer>().OnCooldown) {
					return;
				}

				movementEquipment.OnGrapple(player);
				player.GetModPlayer<MovementEquipmentPlayer>().BeginCooldown(movementEquipment.CooldownTime, movementEquipment.RequiresOnGround);
			});
		};
	}

	public override void PostUpdateMiscEffects() {
		_movementEquipmentCooldown--;

		if (_requiresOnGround && _movementEquipmentCooldown == 0 && Player.velocity.Y != 0f) {
			_movementEquipmentCooldown = 1;
			return;
		}

		if (_movementEquipmentCooldown == 0) {
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
