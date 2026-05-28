using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData : DataModule<WorldData>
{
	[Serializable]
	public class ResourceWater : IEquatable<ResourceWater>
	{
		public float spawn;

		public float water;

		public ResourceWater(float spawn, float water)
		{
			this.spawn = spawn;
			this.water = water;
		}

		public bool Equals(ResourceWater that)
		{
			if (spawn == that.spawn)
			{
				return water == that.water;
			}
			return false;
		}

		public override bool Equals(object o)
		{
			if (!(o is ResourceWater))
			{
				return false;
			}
			return Equals((ResourceWater)o);
		}

		public override int GetHashCode()
		{
			return spawn.GetHashCode() ^ water.GetHashCode();
		}
	}

	public const int CURR_FORMAT_ID = 6;

	public float worldTime;

	public float econSeed;

	public Dictionary<Identifiable.Id, float> econSaturations;

	public Dictionary<Vector3, ResourceWater> resourceSpawnerWater;

	public Dictionary<Vector3, float> spawnerTriggerTimes;

	public Dictionary<string, bool> teleportNodeActivations;

	public Dictionary<Vector3, float> animalSpawnerTimes;

	public ExchangeDirector.Offer offer;

	public float dailyOfferCreateTime;

	public string lastRancherOfferId;

	public Dictionary<Vector3, float> liquidSourceUnits;

	public AmbianceDirector.Weather weather;

	public float weatherUntil;

	public Dictionary<Vector3, int> gordoEatenCounts;

	private const float MAX_DIST_MATCH = 5f;

	private const float MAX_DIST_MATCH_SQR = 25f;

	private const float MAX_DIST_CLOSE_MATCH = 0.1f;

	private const float MAX_DIST_CLOSE_MATCH_SQR = 0.010000001f;

	public static void AssertEquals(WorldData dataA, WorldData dataB)
	{
	}

	private static string PrintResourceSpawnerWater(Dictionary<Vector3, ResourceWater> resourceSpawnerWater)
	{
		string text = "ResourceSpawnerWater: ";
		foreach (KeyValuePair<Vector3, ResourceWater> item in resourceSpawnerWater)
		{
			text = string.Concat(text, item.Key, ":", item.Value.spawn, ":", item.Value.water, ",");
		}
		return text;
	}
}
