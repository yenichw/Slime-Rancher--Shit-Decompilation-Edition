using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class WakeUpDestination : SRBehaviour
{
	[Tooltip("Region associated with the WakeUpDestination. (unique)")]
	public RegionRegistry.RegionSetId deathRegionSetId;

	private SceneContext sceneContext;

	private RegionRegistry.RegionSetId? regionSetId;

	public void Awake()
	{
		sceneContext = SRSingleton<SceneContext>.Instance;
		sceneContext.Register(this);
	}

	public void OnDestroy()
	{
		sceneContext.Deregister(this);
	}

	public RegionRegistry.RegionSetId GetRegionSetId()
	{
		if (!regionSetId.HasValue)
		{
			regionSetId = GetRequiredComponentInParent<Region>().setId;
		}
		return regionSetId.Value;
	}
}
