using System.Collections.Generic;
using UnityEngine;

public class SlimeGateActivator : MonoBehaviour
{
	public AccessDoor gateDoor;

	public void Activate()
	{
		if (SRSingleton<SceneContext>.Instance.PlayerState.SpendKey())
		{
			gateDoor.CurrState = AccessDoor.State.OPEN;
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.OPENED_SLIME_GATES, 1);
			AnalyticsUtil.CustomEvent("GateOpened", new Dictionary<string, object> { { "name", base.name } });
			SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
		}
	}
}
