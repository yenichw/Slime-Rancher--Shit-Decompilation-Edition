using System;
using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public abstract class DirectedActorSpawner : SRBehaviour
{
	public enum TimeMode
	{
		ANY = 0,
		DAY = 1,
		NIGHT = 2,
		CUSTOM = 3
	}

	[Serializable]
	public class TimeWindow
	{
		public TimeMode timeMode;

		public float startHour;

		public float endHour = 24f;

		public float Start()
		{
			switch (timeMode)
			{
			case TimeMode.ANY:
				return 0f;
			case TimeMode.DAY:
				return 6f;
			case TimeMode.NIGHT:
				return 18f;
			default:
				return startHour;
			}
		}

		public float End()
		{
			switch (timeMode)
			{
			case TimeMode.ANY:
				return 24f;
			case TimeMode.DAY:
				return 18f;
			case TimeMode.NIGHT:
				return 6f;
			default:
				return endHour;
			}
		}
	}

	[Serializable]
	public class SpawnConstraint
	{
		public SlimeSet slimeset;

		public float weight = 1f;

		public TimeWindow window;

		public bool feral;

		public bool maxAgitation;

		public bool InWindow(float currHour)
		{
			float num = window.Start();
			float num2 = window.End();
			if (!(num2 >= num) || !(num <= currHour) || !(currHour <= num2))
			{
				if (num2 < num)
				{
					if (!(currHour <= num2))
					{
						return currHour >= num;
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	[Tooltip("An effect to play along with each spawn.")]
	public GameObject spawnFX;

	[Tooltip("The size of the area in which to do the spawning.")]
	public float radius = 5f;

	[Tooltip("Adjusts how much time between the actors being spawned.")]
	public float spawnDelayFactor = 1f;

	[Tooltip("Whether we should immediately enable toteming of spawned actors.")]
	public bool enableToteming;

	public SpawnConstraint[] constraints;

	public GameObject[] spawnLocs;

	public bool allowDirectedSpawns = true;

	public float directedSpawnWeight = 1f;

	protected TimeDirector timeDir;

	private Region region;

	protected const float POP_FORCE = 10f;

	protected const float POP_ROTATE_MAX = 20f;

	public virtual void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public virtual void Start()
	{
		Register(GetComponentInParent<CellDirector>());
		region = GetComponentInParent<Region>();
	}

	protected abstract void Register(CellDirector cellDir);

	protected virtual GameObject MaybeReplacePrefab(GameObject prefab)
	{
		return prefab;
	}

	public virtual bool CanSpawn(float? forHour = null)
	{
		if (region.Hibernated)
		{
			return false;
		}
		return GetEligibleConstraints(forHour).Count > 0;
	}

	protected virtual bool CanContinueSpawning()
	{
		return true;
	}

	public bool CanSpawnSomething()
	{
		float currHour = timeDir.CurrHour();
		SpawnConstraint[] array = constraints;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].InWindow(currHour))
			{
				return true;
			}
		}
		return false;
	}

	public virtual IEnumerator Spawn(int count, Randoms rand)
	{
		Dictionary<SpawnConstraint, float> dictionary = new Dictionary<SpawnConstraint, float>();
		float currHour = timeDir.CurrHour();
		SpawnConstraint[] array = constraints;
		foreach (SpawnConstraint spawnConstraint in array)
		{
			if (spawnConstraint.InWindow(currHour))
			{
				dictionary[spawnConstraint] = spawnConstraint.weight;
			}
		}
		if (dictionary.Count <= 0)
		{
			yield break;
		}
		SpawnConstraint spawnConstraint2 = rand.Pick(dictionary, null);
		SlimeSet slimeset = spawnConstraint2.slimeset;
		bool feral = spawnConstraint2.feral;
		bool maxAgitation = spawnConstraint2.maxAgitation;
		int ii = 0;
		while (ii < count && CanContinueSpawning())
		{
			Dictionary<GameObject, float> dictionary2 = new Dictionary<GameObject, float>();
			SlimeSet.Member[] members = slimeset.members;
			foreach (SlimeSet.Member member in members)
			{
				dictionary2[member.prefab] = member.weight;
			}
			GameObject gameObject = rand.Pick(dictionary2, null);
			if (gameObject == null)
			{
				Log.Warning("Spawner slimeset select with no choices? Skipping.");
				break;
			}
			gameObject = MaybeReplacePrefab(gameObject);
			GameObject gameObject2 = ((spawnLocs == null || spawnLocs.Length == 0) ? null : rand.Pick(spawnLocs));
			Vector3 vector = ((gameObject2 == null) ? (base.transform.position + base.transform.rotation * GetSpawnOffset(rand) * radius) : gameObject2.transform.position);
			Quaternion quaternion = ((gameObject2 == null) ? base.transform.rotation : gameObject2.transform.rotation);
			GameObject gameObject3 = SRBehaviour.InstantiateActor(gameObject, region.setId, vector, quaternion);
			if (feral)
			{
				SlimeFeral component = gameObject3.GetComponent<SlimeFeral>();
				if (component != null)
				{
					Vacuumable component2 = gameObject3.GetComponent<Vacuumable>();
					if (component2 != null && component2.size != 0)
					{
						component.SetFeral();
					}
					else
					{
						Log.Warning("Normal sized slimes cannot be made feral, but trying to mark as such.");
					}
				}
				else
				{
					Log.Warning("Slime has no feral behavior, but trying to mark as such.");
				}
			}
			if (maxAgitation)
			{
				SlimeEmotions component3 = gameObject3.GetComponent<SlimeEmotions>();
				if (component3 != null)
				{
					component3.Adjust(SlimeEmotions.Emotion.AGITATION, 1f);
				}
			}
			if (enableToteming)
			{
				TotemLinker componentInChildren = gameObject3.GetComponentInChildren<TotemLinker>();
				if (componentInChildren != null)
				{
					componentInChildren.SetStackReceptive(receptive: true);
				}
			}
			SpawnListener[] componentsInChildren = gameObject3.GetComponentsInChildren<SpawnListener>(includeInactive: true);
			int i;
			for (i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].DidSpawn();
			}
			SpawnFX(gameObject3, vector);
			Vector3 force = quaternion * Vector3.up * 10f;
			Vector3 torque = quaternion * Vector3.up * rand.GetInRange(-20f, 20f);
			Rigidbody component4 = gameObject3.GetComponent<Rigidbody>();
			component4.AddForce(force, ForceMode.VelocityChange);
			component4.AddTorque(torque, ForceMode.VelocityChange);
			OnActorSpawned(gameObject3);
			yield return new WaitForSeconds(rand.GetInRange(0.1f, 0.3f) * spawnDelayFactor);
			i = ii + 1;
			ii = i;
		}
	}

	protected virtual void OnActorSpawned(GameObject spawnedObj)
	{
	}

	protected virtual void SpawnFX(GameObject spawnedObj, Vector3 pos)
	{
		if (spawnFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(spawnFX, pos, Quaternion.identity);
		}
	}

	private Vector3 GetSpawnOffset(Randoms rand)
	{
		UnityEngine.Random.InitState(rand.GetInt());
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		return new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
	}

	public List<SpawnConstraint> GetEligibleConstraints(float? forHour)
	{
		List<SpawnConstraint> list = new List<SpawnConstraint>();
		SpawnConstraint[] array = constraints;
		foreach (SpawnConstraint spawnConstraint in array)
		{
			if (spawnConstraint.InWindow(forHour ?? timeDir.CurrHour()))
			{
				list.Add(spawnConstraint);
			}
		}
		return list;
	}
}
