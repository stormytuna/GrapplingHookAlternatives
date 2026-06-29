namespace GrapplingHookAlternatives.Interfaces;

public interface IMovementEquipment
{
	/// <summary>
	/// The amount of time in frames it takes for this equipment to recharge
	/// </summary>
	public int CooldownTime { get; }

	/// <summary>
	/// Whether this equipment requires the player to be on solid ground to go off cooldown
	/// </summary>
	public bool RequiresOnGround { get; }

	/// <summary>
	/// A hook provided for when the player uses this equipment
	/// </summary>
	public void OnGrapple(Player player) { }
}
