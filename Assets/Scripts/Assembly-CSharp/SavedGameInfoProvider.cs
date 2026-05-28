using UnityEngine;

public interface SavedGameInfoProvider
{
	string GetVersion();

	Vector3 GetWakeUpDestination();
}
