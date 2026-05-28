using System;
using System.Collections.Generic;
using UnityEngine;

public class GordoRewards : GordoRewardsBase
{
	[Serializable]
	public class RewardOverride
	{
		public PlayerState.GameMode gameMode;

		public GameObject[] rewardPrefabs;
	}

	[Tooltip("The default rewards to provide on popping the gordo")]
	public GameObject[] rewardPrefabs;

	[Tooltip("A set of overrides for different game modes on popping the gordo")]
	public RewardOverride[] rewardOverrides;

	protected override IEnumerable<GameObject> SelectActiveRewardPrefabs()
	{
		PlayerState.GameMode currGameMode = SRSingleton<SceneContext>.Instance.GameModel.currGameMode;
		RewardOverride[] array = rewardOverrides;
		foreach (RewardOverride rewardOverride in array)
		{
			if (rewardOverride.gameMode == currGameMode)
			{
				return rewardOverride.rewardPrefabs;
			}
		}
		return rewardPrefabs;
	}
}
