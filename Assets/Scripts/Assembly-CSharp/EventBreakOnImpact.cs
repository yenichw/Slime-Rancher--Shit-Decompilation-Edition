using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EventBreakOnImpact : BreakOnImpactBase
{
	protected override IEnumerable<GameObject> GetRewardPrefabs()
	{
		yield return SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(PickOrnamentReward());
	}

	private Identifiable.Id PickOrnamentReward()
	{
		HolidayModel.EventGordo eventGordo = Randoms.SHARED.Pick(SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventGordos, null);
		if (eventGordo is HolidayModel.EventGordo.Default)
		{
			if (!Randoms.SHARED.GetProbability(HolidayModel.EventGordo.RARE_ORNAMENT_CHANCE))
			{
				return Randoms.SHARED.Pick(((HolidayModel.EventGordo.Default)eventGordo).commons);
			}
			return Randoms.SHARED.Pick(HolidayModel.EventGordo.RARE_ORNAMENTS);
		}
		if (eventGordo is HolidayModel.EventGordo.Fixed)
		{
			return ((HolidayModel.EventGordo.Fixed)eventGordo).ornament;
		}
		if (!Randoms.SHARED.GetProbability(HolidayModel.EventGordo.RARE_ORNAMENT_CHANCE))
		{
			return Identifiable.Id.PINK_ORNAMENT;
		}
		return Randoms.SHARED.Pick(HolidayModel.EventGordo.RARE_ORNAMENTS);
	}
}
