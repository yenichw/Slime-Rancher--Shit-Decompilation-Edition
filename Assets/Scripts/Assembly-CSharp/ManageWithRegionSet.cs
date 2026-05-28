using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ManageWithRegionSet : MonoBehaviour
{
	public RegionRegistry.RegionSetId setId;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.RegionRegistry.ManageWithRegionSet(base.gameObject, setId);
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.RegionRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.RegionRegistry.ReleaseFromRegionSet(base.gameObject, setId);
		}
	}
}
