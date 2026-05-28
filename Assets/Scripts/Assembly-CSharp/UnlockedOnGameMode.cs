using UnityEngine;

public class UnlockedOnGameMode : MonoBehaviour
{
	public bool IsUnlockedFor(PlayerState.GameMode currGameMode)
	{
		return currGameMode == PlayerState.GameMode.TIME_LIMIT_V2;
	}
}
