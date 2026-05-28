using UnityEngine;

public class vp_HealthPickup : vp_Pickup
{
	public float Health = 1f;

	protected override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (player.Health.Get() < 0f)
		{
			return false;
		}
		if (player.Health.Get() >= player.MaxHealth.Get())
		{
			return false;
		}
		player.Health.Set(Mathf.Min(player.MaxHealth.Get(), player.Health.Get() + Health));
		return true;
	}
}
