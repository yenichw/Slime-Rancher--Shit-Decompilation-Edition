using System;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnImpact : BreakOnImpactBase
{
	[Serializable]
	public class SpawnOption
	{
		public GameObject spawn;

		public float weight;
	}

	public int minSpawns = 2;

	public int maxSpawns = 4;

	public List<SpawnOption> spawnOptions = new List<SpawnOption>();

	private Dictionary<SpawnOption, float> spawnWeights = new Dictionary<SpawnOption, float>();

	public override void Awake()
	{
		base.Awake();
		foreach (SpawnOption spawnOption in spawnOptions)
		{
			spawnWeights[spawnOption] = spawnOption.weight;
		}
	}

	protected override IEnumerable<GameObject> GetRewardPrefabs()
	{
		int numSpawns = Randoms.SHARED.GetInRange(minSpawns, maxSpawns);
		int ii = 0;
		while (ii < numSpawns)
		{
			SpawnOption spawnOption = Randoms.SHARED.Pick(spawnWeights, null);
			if (spawnOption != null)
			{
				yield return spawnOption.spawn;
			}
			int num = ii + 1;
			ii = num;
		}
		foreach (Identifiable.Id item in SRSingleton<SceneContext>.Instance.HolidayDirector.GetCurrOrnament())
		{
			yield return SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(item);
		}
	}
}
