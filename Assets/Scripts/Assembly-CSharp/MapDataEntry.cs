using UnityEngine;

public class MapDataEntry : SRBehaviour, TechActivator
{
	[Tooltip("The zone for which we are giving map data.")]
	public ZoneDirector.Zone zone;

	public GameObject hologram;

	public GameObject activeFx;

	private Collider collider;

	public void Start()
	{
		collider = GetRequiredComponent<Collider>();
		UpdateHologramState();
	}

	public void Activate()
	{
		if (IsZoneLocked())
		{
			SRSingleton<SceneContext>.Instance.PlayerState.UnlockMap(zone);
			UpdateHologramState();
			SRSingleton<SceneContext>.Instance.TutorialDirector.OnMapDataGained();
			SRSingleton<Map>.Instance.OpenMap(zone);
		}
	}

	private void UpdateHologramState()
	{
		bool active = IsZoneLocked();
		hologram.SetActive(active);
		activeFx.SetActive(active);
		collider.enabled = active;
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}

	private bool IsZoneLocked()
	{
		return !SRSingleton<SceneContext>.Instance.PlayerState.HasUnlockedMap(zone);
	}
}
