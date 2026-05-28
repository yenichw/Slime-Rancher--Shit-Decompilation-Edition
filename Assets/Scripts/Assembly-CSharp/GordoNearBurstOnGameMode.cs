using UnityEngine;

public class GordoNearBurstOnGameMode : MonoBehaviour
{
	[Tooltip("Number of eaten counts remaining on the gordo.")]
	public uint remaining = 1u;

	public bool NearBurstForGameMode(PlayerState.GameMode currGameMode)
	{
		return currGameMode == PlayerState.GameMode.TIME_LIMIT_V2;
	}
}
