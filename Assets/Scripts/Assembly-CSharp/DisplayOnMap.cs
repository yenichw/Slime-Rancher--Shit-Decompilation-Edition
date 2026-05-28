using System;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DisplayOnMap : MonoBehaviour
{
	public MapMarker markerPrefab;

	public bool HideInFog;

	private MapMarker marker;

	private PlayerState playerState;

	private RegionRegistry.RegionSetId? regionSetId;

	public virtual void Awake()
	{
		SRSingleton<Map>.Instance.RegisterMarker(this);
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		marker = UnityEngine.Object.Instantiate(markerPrefab, base.transform);
		marker.gameObject.SetActive(value: false);
	}

	public virtual void Refresh()
	{
	}

	public virtual Vector3 GetCurrentPosition()
	{
		return base.gameObject.transform.position;
	}

	public virtual MapMarker GetMarker()
	{
		return marker;
	}

	public virtual ZoneDirector.Zone GetZoneId()
	{
		return GetComponentInParent<ZoneDirector>().zone;
	}

	public virtual bool ShowOnMap()
	{
		CellDirector parentCellDirector = GetParentCellDirector();
		if (parentCellDirector != null && parentCellDirector.notShownOnMap)
		{
			return false;
		}
		if (HideInFog && !playerState.HasUnlockedMap(GetZoneId()))
		{
			return false;
		}
		return true;
	}

	public virtual Quaternion GetCurrentRotation()
	{
		return Quaternion.identity;
	}

	public virtual RegionRegistry.RegionSetId GetRegionSetId()
	{
		if (regionSetId.HasValue)
		{
			return regionSetId.Value;
		}
		RegionMember component = GetComponent<RegionMember>();
		if (component != null)
		{
			regionSetId = component.setId;
			return regionSetId.Value;
		}
		Region componentInParent = GetComponentInParent<Region>();
		if (componentInParent != null)
		{
			regionSetId = componentInParent.setId;
			return regionSetId.Value;
		}
		throw new Exception($"Failed to get RegionSetId for DisplayOnMap. [name={base.gameObject.name}]");
	}

	protected CellDirector GetParentCellDirector()
	{
		return base.gameObject.GetComponentInParent<CellDirector>();
	}

	public virtual void OnDestroy()
	{
		if (SRSingleton<Map>.Instance != null)
		{
			SRSingleton<Map>.Instance.DeregisterMarker(this);
		}
		if (marker != null)
		{
			Destroyer.Destroy(marker.gameObject, "DisplayOnMap.OnDestroy");
		}
	}
}
