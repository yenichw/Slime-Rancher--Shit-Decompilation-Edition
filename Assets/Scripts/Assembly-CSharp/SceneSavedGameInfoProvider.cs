using UnityEngine;

public class SceneSavedGameInfoProvider : SavedGameInfoProvider
{
	public string GetVersion()
	{
		return SRSingleton<GameContext>.Instance.MessageDirector.Get("build", "m.version");
	}

	public Vector3 GetWakeUpDestination()
	{
		return SRSingleton<SceneContext>.Instance.GetWakeUpDestination().transform.position;
	}
}
