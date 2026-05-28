using System;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.UI;

public class RadarTrackedObject : MonoBehaviour
{
	public Image radarImage;

	public bool isOptional;

	[NonSerialized]
	public RegionRegistry.RegionSetId regionSetId = RegionRegistry.RegionSetId.UNSET;

	public void Start()
	{
		if (regionSetId == RegionRegistry.RegionSetId.UNSET)
		{
			regionSetId = GetComponentInParent<Region>().setId;
			SRSingleton<RadarPanelUI>.Instance.RegisterTracked(base.gameObject, regionSetId, radarImage, isOptional);
		}
		base.transform.SetParent(SRSingleton<DynamicObjectContainer>.Instance.transform, worldPositionStays: true);
	}

	public void OnEnable()
	{
		if (regionSetId != RegionRegistry.RegionSetId.UNSET)
		{
			SRSingleton<RadarPanelUI>.Instance.RegisterTracked(base.gameObject, regionSetId, radarImage, isOptional);
		}
	}

	public void OnDisable()
	{
		if (SRSingleton<RadarPanelUI>.Instance != null)
		{
			SRSingleton<RadarPanelUI>.Instance.UnregisterTracked(base.gameObject);
		}
	}
}
