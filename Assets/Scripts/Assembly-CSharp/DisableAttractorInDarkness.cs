using System.Collections.Generic;
using UnityEngine;

public class DisableAttractorInDarkness : SRBehaviour, CaveTrigger.Listener
{
	public float startHour = 18f;

	public float endHour = 6f;

	private TimeDirector timeDir;

	private MosaicAttractor attractor;

	private HashSet<GameObject> caves = new HashSet<GameObject>();

	public void Start()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		attractor = GetComponentInChildren<MosaicAttractor>(includeInactive: true);
	}

	public void Update()
	{
		float num = timeDir.CurrHourOrStart();
		bool flag = ((!(endHour < startHour)) ? (num <= endHour && num >= startHour) : (num >= startHour || num <= endHour));
		SetAttractorActive(!flag && caves.Count <= 0);
	}

	public void OnCaveEnter(GameObject caveObj, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		caves.Add(caveObj);
	}

	public void OnCaveExit(GameObject caveObj, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		caves.Remove(caveObj);
	}

	private void SetAttractorActive(bool active)
	{
		attractor.gameObject.SetActive(active);
	}
}
