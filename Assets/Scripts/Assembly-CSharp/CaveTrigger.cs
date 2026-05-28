using UnityEngine;

public class CaveTrigger : WeatherBlockingTrigger
{
	public interface Listener
	{
		void OnCaveEnter(GameObject gameObject, bool affectLighting, AmbianceDirector.Zone caveZone);

		void OnCaveExit(GameObject gameObject, bool affectLighting, AmbianceDirector.Zone caveZone);
	}

	public CaveLightController[] lights = new CaveLightController[0];

	public bool affectsLighting = true;

	public AmbianceDirector.Zone caveZone = AmbianceDirector.Zone.CAVE;

	private PlayerCaveLighting playerListener;

	private float triggerness;

	private AmbianceDirector ambianceDir;

	public void Awake()
	{
		ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Listener interfaceComponent = col.gameObject.GetInterfaceComponent<Listener>();
		if (interfaceComponent != null)
		{
			interfaceComponent.OnCaveEnter(base.gameObject, affectsLighting, caveZone);
			if (interfaceComponent is PlayerCaveLighting)
			{
				playerListener = (PlayerCaveLighting)interfaceComponent;
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Listener interfaceComponent = col.gameObject.GetInterfaceComponent<Listener>();
		if (interfaceComponent != null)
		{
			interfaceComponent.OnCaveExit(base.gameObject, affectsLighting, caveZone);
			if (interfaceComponent == playerListener)
			{
				playerListener = null;
			}
		}
	}

	public void OnDisable()
	{
		if (playerListener != null)
		{
			playerListener.OnCaveExit(base.gameObject, affectsLighting, caveZone);
			playerListener = null;
		}
	}

	public void Update()
	{
		if (playerListener != null && triggerness < 1f)
		{
			triggerness = Mathf.Min(1f, triggerness + Time.deltaTime / ambianceDir.zoneSettingTransitionTime);
		}
		else if (playerListener == null && triggerness > 0f)
		{
			triggerness = Mathf.Max(0f, triggerness - Time.deltaTime / ambianceDir.zoneSettingTransitionTime);
		}
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].SetTriggerness(this, triggerness);
		}
	}
}
