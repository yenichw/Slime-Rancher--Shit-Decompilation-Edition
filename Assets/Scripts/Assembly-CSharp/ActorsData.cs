using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorsData : DataModule<ActorsData>
{
	[Serializable]
	public class ActorData
	{
		public Vector3 pos;

		public Vector3 rot;

		public Identifiable.Id id;

		public SlimeEmotionData emotions;

		public float transformTime;

		public float reproduceTime;

		public ResourceCycle.CycleData cycleData;

		public float? disabledAtTime;
	}

	public const int CURR_FORMAT_ID = 1;

	private ActorData[] actors;

	public ActorData[] GetActors()
	{
		return actors;
	}

	public static void AssertEquals(ActorsData dataA, ActorsData dataB)
	{
		TestUtil.Vector3Comparer vector3Comparer = new TestUtil.Vector3Comparer(0.1f);
		if (dataA.actors.Length != dataB.actors.Length)
		{
			return;
		}
		List<ActorData> list = new List<ActorData>(dataB.actors);
		ActorData[] array = dataA.actors;
		foreach (ActorData actorData in array)
		{
			foreach (ActorData item in list)
			{
				if (vector3Comparer.Equals(actorData.pos, item.pos) && actorData.id == item.id)
				{
					AssertEqualActors(actorData, item);
					list.Remove(item);
					break;
				}
			}
		}
	}

	private static void AssertEqualActors(ActorData actorA, ActorData actorB)
	{
	}
}
