namespace GrapplingHookAlternatives.Interfaces;

public interface IMovementEquipment
{
	public int CooldownTime { get; }
	public void OnGrapple(Player player) { }
}
