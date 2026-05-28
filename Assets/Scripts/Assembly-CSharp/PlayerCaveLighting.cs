using UnityEngine;

public class PlayerCaveLighting : SRBehaviour, CaveTrigger.Listener
{
	private AmbianceDirector ambianceDir;

	public void Awake()
	{
		ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
	}

	public void OnCaveEnter(GameObject gameObject, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		if (affectLighting)
		{
			ambianceDir.EnterCave(caveZone);
		}
	}

	public void OnCaveExit(GameObject gameObject, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		if (affectLighting)
		{
			ambianceDir.ExitCave(caveZone);
		}
	}
}
