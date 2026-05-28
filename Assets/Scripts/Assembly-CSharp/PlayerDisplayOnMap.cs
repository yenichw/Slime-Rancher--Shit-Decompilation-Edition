using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PlayerDisplayOnMap : DisplayOnMap
{
	public MapMarker playerUnknownLocationMarkerPrefab;

	private MapMarker playerUnknownLocationMarker;

	private bool isInHiddenCell;

	private PlayerZoneTracker playerZoneTracker;

	public override void Awake()
	{
		base.Awake();
		playerUnknownLocationMarker = Object.Instantiate(playerUnknownLocationMarkerPrefab);
		playerZoneTracker = GetComponent<PlayerZoneTracker>();
	}

	public override ZoneDirector.Zone GetZoneId()
	{
		return playerZoneTracker.GetCurrentZone();
	}

	public bool IsInUnknownArea()
	{
		return IsInHiddenCell();
	}

	public override RegionRegistry.RegionSetId GetRegionSetId()
	{
		if (!isInHiddenCell)
		{
			return SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId();
		}
		return RegionRegistry.RegionSetId.HOME;
	}

	private bool IsInHiddenCell()
	{
		return GetComponent<RegionMember>().regions.Where((Region r) => r.cellDir.notShownOnMap).Count() > 0;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Destroyer.Destroy(playerUnknownLocationMarker, "PlayerDisplayOnMap.OnDestroy");
	}

	public override Vector3 GetCurrentPosition()
	{
		if (!isInHiddenCell)
		{
			return base.GetCurrentPosition();
		}
		return Vector3.zero;
	}

	public override void Refresh()
	{
		base.Refresh();
		isInHiddenCell = IsInHiddenCell();
		if (isInHiddenCell)
		{
			playerUnknownLocationMarker.gameObject.SetActive(value: true);
			base.GetMarker().gameObject.SetActive(value: false);
		}
		else
		{
			playerUnknownLocationMarker.gameObject.SetActive(value: false);
			base.GetMarker().gameObject.SetActive(value: true);
		}
	}

	public override bool ShowOnMap()
	{
		return true;
	}

	public override MapMarker GetMarker()
	{
		if (!isInHiddenCell)
		{
			return base.GetMarker();
		}
		return playerUnknownLocationMarker;
	}

	public override Quaternion GetCurrentRotation()
	{
		if (!isInHiddenCell)
		{
			return base.gameObject.transform.rotation;
		}
		return Quaternion.identity;
	}
}
