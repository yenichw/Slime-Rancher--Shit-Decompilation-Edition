using UnityEngine;

public class DroneStationAnimator : SRAnimator
{
	private static readonly int STATION_ENABLED = Animator.StringToHash("STATION_ENABLED");

	public void SetEnabled(bool enabled)
	{
		base.animator.SetBool(STATION_ENABLED, enabled);
	}
}
