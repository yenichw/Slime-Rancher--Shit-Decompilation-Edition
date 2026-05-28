using System;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PinkSlimeFoodTypeTracker : MonoBehaviour
{
	public void Start()
	{
		if (!(SRSingleton<SceneContext>.Instance != null))
		{
			return;
		}
		AchievementsDirector achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		RegionMember member = GetComponent<RegionMember>();
		SlimeEat component = GetComponent<SlimeEat>();
		component.onEat = (SlimeEat.OnEatDelegate)Delegate.Combine(component.onEat, (SlimeEat.OnEatDelegate)delegate(Identifiable.Id eatId)
		{
			if (Identifiable.IsFood(eatId) && CellDirector.IsOnRanch(member))
			{
				achieveDir.AddToStat(AchievementsDirector.EnumStat.PINK_SLIMES_FOOD_TYPES, eatId);
			}
		});
	}
}
