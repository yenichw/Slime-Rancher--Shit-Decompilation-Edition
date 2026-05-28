using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EventGordoRewards : GordoRewardsBase
{
	[Tooltip("Fashion to attach to spawned slimes. (optional)")]
	public Fashion slimeFashion;

	[Tooltip("Number of EventGordo crates to spawn on break.")]
	public int cratesToSpawn;

	protected override IEnumerable<GameObject> SelectActiveRewardPrefabs()
	{
		LookupDirector lookupDirector = SRSingleton<GameContext>.Instance.LookupDirector;
		List<GameObject> list = new List<GameObject>();
		list.Add(lookupDirector.GetPrefab(PickOrnamentReward()));
		list.AddRange(Enumerable.Repeat(lookupDirector.GetPrefab(HolidayModel.EventGordo.CRATE), cratesToSpawn));
		return list;
	}

	protected override void OnInstantiatedReward(GameObject instance)
	{
		base.OnInstantiatedReward(instance);
		if (slimeFashion != null)
		{
			AttachFashions component = instance.GetComponent<AttachFashions>();
			if (component != null)
			{
				component.Attach(slimeFashion, skipFX: true);
			}
		}
	}

	private Identifiable.Id PickOrnamentReward()
	{
		string id = GetComponent<IdHandler>().id;
		HolidayModel.EventGordo eventGordo = SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventGordos.FirstOrDefault((HolidayModel.EventGordo e) => e.objectId == id);
		if (eventGordo is HolidayModel.EventGordo.Fixed)
		{
			return ((HolidayModel.EventGordo.Fixed)eventGordo).ornament;
		}
		return Randoms.SHARED.Pick(HolidayModel.EventGordo.RARE_ORNAMENTS);
	}
}
