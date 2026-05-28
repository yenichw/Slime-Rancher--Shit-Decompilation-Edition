using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DirectedCrateSpawner : SRBehaviour
{
	private ZoneDirector zoneDirector;

	public void Start()
	{
		zoneDirector = GetComponentInParent<ZoneDirector>();
		zoneDirector.Register(this);
	}

	public GameObject Spawn(GameObject zoneCratePrefab)
	{
		RegionRegistry.RegionSetId regionSetId = zoneDirector.regionSetId;
		if (SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventGordos.Any() && Randoms.SHARED.GetProbability(HolidayModel.EventGordo.CRATE_CHANCE))
		{
			return SRBehaviour.InstantiateActor(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(HolidayModel.EventGordo.CRATE), regionSetId, base.transform.position, base.transform.rotation);
		}
		return SRBehaviour.InstantiateActor(zoneCratePrefab, regionSetId, base.transform.position, base.transform.rotation);
	}
}
