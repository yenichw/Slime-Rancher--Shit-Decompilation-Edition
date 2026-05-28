using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.Events;

public class SpawnResource : SRBehaviour, LiquidConsumer, DestroyAfterTimeListener, DestroyAfterTimeCondition, SpawnResourceModel.Participant
{
	public enum Id
	{
		NONE = 0,
		CUBERRY_TREE = 1,
		MANGO_TREE = 2,
		CARROT_PATCH = 3,
		OCAOCA_PATCH = 4,
		PEAR_TREE = 5,
		POGO_TREE = 6,
		PARSNIP_PATCH = 7,
		BEET_PATCH = 8,
		ONION_PATCH = 9,
		LEMON_TREE = 10,
		GINGER_PATCH = 11,
		CUBERRY_TREE_DLX = 12,
		MANGO_TREE_DLX = 13,
		CARROT_PATCH_DLX = 14,
		OCAOCA_PATCH_DLX = 15,
		PEAR_TREE_DLX = 16,
		POGO_TREE_DLX = 17,
		PARSNIP_PATCH_DLX = 18,
		BEET_PATCH_DLX = 19,
		ONION_PATCH_DLX = 20,
		LEMON_TREE_DLX = 21,
		GINGER_PATCH_DLX = 22
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public bool Equals(Id id1, Id id2)
		{
			return id1 == id2;
		}

		public int GetHashCode(Id obj)
		{
			return (int)obj;
		}
	}

	private class SpawnRequest
	{
		public double? spawnAtTime;

		public bool spawnRipe;
	}

	private class SpawnMetadata
	{
		public SpawnRequest request;

		public GameObject prefab;

		public int count;
	}

	private class GatherGroup : DroneFastForwarder.GatherGroup
	{
		private SpawnResource resource;

		private Identifiable.Id rid;

		private int rcount;

		public override Identifiable.Id id => rid;

		public override int count => rcount;

		public override bool overflow => false;

		public GatherGroup(SpawnResource resource, Identifiable.Id id, int count)
		{
			this.resource = resource;
			rid = id;
			rcount = count;
		}

		public override void Decrement(int decrement)
		{
			rcount = Mathf.Max(rcount - decrement, 0);
			if (rcount <= 0 && resource.spawnQueue.Any())
			{
				resource.spawnQueue.Dequeue();
			}
		}

		public override void Dispose()
		{
			rcount = 0;
		}
	}

	public static IdComparer idComparer = new IdComparer();

	public Id id;

	public GameObject[] ObjectsToSpawn;

	public GameObject[] BonusObjectsToSpawn;

	public float MaxObjectsSpawned = 1f;

	public float MinObjectsSpawned = 1f;

	public float MinNutrientObjectsSpawned = 1f;

	public float MinSpawnIntervalGameHours = 18f;

	public float MaxSpawnIntervalGameHours = 24f;

	public Joint[] SpawnJoints;

	public float BonusChance = 0.01f;

	public int minBonusSelections;

	public GameObject SpawnFX;

	public int MaxActiveSpawns;

	public int MaxTotalSpawns;

	public float wateringDurationHours = 23f;

	public bool forceDestroyLeftoversOnSpawn;

	[Tooltip("GameObject that is shown/hidden based off the watered state. (optional)")]
	public GameObject onWateredFX;

	public UnityAction onReachedSpawnTime;

	private bool allowSpawningInFastForwarding;

	private Queue<SpawnRequest> spawnQueue = new Queue<SpawnRequest>();

	private List<GameObject> spawned = new List<GameObject>();

	private int totalSpawnsRemaining;

	private Randoms rand;

	private TimeDirector timeDir;

	private AmbianceDirector ambianceDir;

	private Region region;

	private LandPlot landPlot;

	private int spawnBlockers;

	private SpawnResourceModel model;

	private const float MAX_WATER_STORED = 3f;

	private const float WATER_USED_PER_HOUR = 0.13043478f;

	private const float WATERED_TIME_FACTOR = 0.5f;

	private const float MIN_BONUS_RIPENESS = 3f;

	private const float MAX_BONUS_RIPENESS = 9f;

	private const float SCALE_IN_RESOURCE_TIME = 4f;

	private const float FAST_FORWARD_MIN_HOURS = 0.25f;

	private const float MIN_SPAWN_TIME_STEP = 1f;

	public double GetNextSpawnTime()
	{
		return model.nextSpawnTime;
	}

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
		rand = Randoms.SHARED;
		totalSpawnsRemaining = MaxTotalSpawns;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterResourceSpawner(base.transform.position, this);
	}

	public void Start()
	{
		landPlot = GetComponentInParent<LandPlot>();
		region = GetComponentInParent<Region>();
		if (landPlot != null)
		{
			allowSpawningInFastForwarding = true;
		}
		else
		{
			allowSpawningInFastForwarding = false;
		}
	}

	public void InitModel(SpawnResourceModel model)
	{
		model.pos = base.transform.position;
		model.nextSpawnRipens = false;
	}

	public void SetModel(SpawnResourceModel model)
	{
		this.model = model;
		if (model.nextSpawnTime == 0.0)
		{
			if (GetComponent<SpawnResourceForceFirstRipeness>() != null)
			{
				model.nextSpawnRipens = true;
			}
			else if (timeDir.IsAtStart())
			{
				float inRange = rand.GetInRange(3f, 9f);
				double nextSpawnTime = timeDir.HoursFromNowOrStart(0f - inRange);
				model.nextSpawnTime = nextSpawnTime;
			}
			else
			{
				model.nextSpawnTime = timeDir.WorldTime();
			}
		}
	}

	public void InitAsReplacement(SpawnResource old)
	{
		totalSpawnsRemaining = old.totalSpawnsRemaining;
		if (old.model != null)
		{
			old.model.SetParticipant(this);
			SetModel(old.model);
		}
		Joint[] spawnJoints = old.SpawnJoints;
		foreach (Joint joint in spawnJoints)
		{
			if (joint.connectedBody != null)
			{
				Joint joint2 = NearestJoint(joint.transform.position, 0.1f);
				if (joint2 != null)
				{
					joint.connectedBody.position = joint2.transform.position;
					joint.connectedBody.GetComponent<ResourceCycle>().Reattach(joint2);
				}
			}
		}
	}

	public void Update()
	{
		if (!region.Hibernated)
		{
			UpdateToTime(timeDir.WorldTime(), timeDir.DeltaWorldTime());
			if (spawnQueue.Count > 0)
			{
				Spawn(spawnQueue.Dequeue());
			}
		}
	}

	public void UpdateToTime(double worldTime, double deltaWorldTime)
	{
		model.storedWater += (float)((double)(ambianceDir.PrecipitationRate() - 0.13043478f) * deltaWorldTime * 0.00027777778450399637);
		model.storedWater = Mathf.Clamp(model.storedWater, 0f, 3f);
		if (onWateredFX != null)
		{
			onWateredFX.SetActive(IsWatered());
		}
		if (spawnBlockers > 0)
		{
			return;
		}
		model.nextSpawnTime -= (double)AdditionalRipenessPerSecond() * deltaWorldTime;
		if (!TimeUtil.HasReached(worldTime, model.nextSpawnTime))
		{
			return;
		}
		float num = (float)((worldTime - model.nextSpawnTime) * 0.00027777778450399637);
		if (allowSpawningInFastForwarding && num >= 0.25f)
		{
			ResourceCycle resourceToSpawn = GetResourceToSpawn();
			float num2 = resourceToSpawn.unripeGameHours + resourceToSpawn.ripeGameHours;
			float num3 = resourceToSpawn.edibleGameHours + resourceToSpawn.rottenGameHours;
			if (landPlot != null && landPlot.HasUpgrade(LandPlot.Upgrade.MIRACLE_MIX))
			{
				num3 *= 2f;
			}
			while (num >= 0f)
			{
				if (num < num2 + num3)
				{
					spawnQueue.Enqueue(new SpawnRequest
					{
						spawnAtTime = model.nextSpawnTime
					});
				}
				float num4 = rand.GetInRange(MinSpawnIntervalGameHours, MaxSpawnIntervalGameHours) * (IsWatered() ? 0.5f : 1f);
				if (num4 < 1f)
				{
					num4 = 1f;
				}
				StepNextSpawnTime(0f, num4);
				num -= num4;
			}
		}
		else if (model.nextSpawnRipens)
		{
			spawnQueue.Enqueue(new SpawnRequest
			{
				spawnRipe = true
			});
			UpdateNextSpawnTime(0f);
			model.nextSpawnRipens = false;
		}
		else
		{
			double num5 = model.nextSpawnTime;
			for (float num6 = rand.GetInRange(MinSpawnIntervalGameHours, MaxSpawnIntervalGameHours) * (IsWatered() ? 0.5f : 1f) * 3600f; num5 + (double)num6 < worldTime; num5 += (double)num6)
			{
			}
			spawnQueue.Enqueue(new SpawnRequest
			{
				spawnAtTime = num5
			});
			UpdateNextSpawnTime(0f);
		}
		if (onReachedSpawnTime != null)
		{
			onReachedSpawnTime();
		}
	}

	private ResourceCycle GetResourceToSpawn()
	{
		return ObjectsToSpawn[0].GetComponent<ResourceCycle>();
	}

	public void OnDestroy()
	{
		DropFromJoints();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterResourceSpawner(base.transform.position);
		}
	}

	public void WillDestroyAfterTime()
	{
		DropFromJoints();
		SRSingleton<SceneContext>.Instance.GameModel.UnregisterResourceSpawner(base.transform.position);
	}

	private void DropFromJoints()
	{
		Joint[] spawnJoints = SpawnJoints;
		foreach (Joint joint in spawnJoints)
		{
			if (joint.connectedBody != null)
			{
				ResourceCycle component = joint.connectedBody.GetComponent<ResourceCycle>();
				joint.connectedBody.WakeUp();
				if (component != null)
				{
					component.Detach(AdditionalRipenessPerSecond);
				}
				joint.connectedBody = null;
			}
		}
	}

	public Identifiable.Id GetPrimarySpawnId()
	{
		if (BonusObjectsToSpawn != null && BonusObjectsToSpawn.Length != 0)
		{
			return BonusObjectsToSpawn[0].GetComponent<Identifiable>().id;
		}
		if (ObjectsToSpawn == null || ObjectsToSpawn.Length < 1)
		{
			return Identifiable.Id.NONE;
		}
		return ObjectsToSpawn[0].GetComponent<Identifiable>().id;
	}

	public void AddLiquid(Identifiable.Id liquidId, float amount)
	{
		if (Identifiable.IsWater(liquidId))
		{
			model.storedWater = Mathf.Min(3f, model.storedWater + amount);
		}
	}

	private bool HasNutrientSoil()
	{
		LandPlot componentInParent = GetComponentInParent<LandPlot>();
		if (componentInParent != null)
		{
			return componentInParent.HasUpgrade(LandPlot.Upgrade.SOIL);
		}
		return false;
	}

	private bool HasSprinkler()
	{
		if (landPlot != null)
		{
			return landPlot.HasUpgrade(LandPlot.Upgrade.SPRINKLER);
		}
		return false;
	}

	private bool IsWatered()
	{
		if (!HasSprinkler())
		{
			return model.storedWater > 0f;
		}
		return true;
	}

	private float AdditionalRipenessPerSecond()
	{
		if (!IsWatered())
		{
			return 0f;
		}
		return 0.5f;
	}

	private IEnumerable<SpawnMetadata> GetSpawnMetadatas(SpawnRequest request, double worldTime)
	{
		int num = 0;
		if (MaxActiveSpawns != 0)
		{
			spawned.RemoveAll((GameObject go) => go == null);
			num = spawned.Count;
		}
		ReferenceCount<GameObject> referenceCount = new ReferenceCount<GameObject>();
		if (MaxActiveSpawns == 0 || num < MaxActiveSpawns)
		{
			int num2 = (int)Random.Range(HasNutrientSoil() ? MinNutrientObjectsSpawned : MinObjectsSpawned, MaxObjectsSpawned);
			for (int i = 0; i < num2; i++)
			{
				GameObject[] array = ((BonusObjectsToSpawn != null && BonusObjectsToSpawn.Length >= 1 && (i < minBonusSelections || Random.value < BonusChance)) ? BonusObjectsToSpawn : ObjectsToSpawn);
				GameObject gameObject = array[rand.GetInRange(0, array.Length - 1)];
				ResourceCycle component = gameObject.GetComponent<ResourceCycle>();
				if (!request.spawnAtTime.HasValue || !component.WouldProgressToRotten(request.spawnAtTime.Value, worldTime))
				{
					referenceCount.Increment(gameObject);
				}
			}
		}
		return referenceCount.Select((KeyValuePair<GameObject, int> pair) => new SpawnMetadata
		{
			request = request,
			prefab = pair.Key,
			count = pair.Value
		});
	}

	private void Spawn(SpawnRequest request)
	{
		Spawn(request, timeDir.WorldTime());
	}

	private void Spawn(SpawnRequest request, double worldTime)
	{
		Spawn(GetSpawnMetadatas(request, worldTime));
	}

	private void Spawn(IEnumerable<SpawnMetadata> metadatas)
	{
		List<Joint> list = new List<Joint>();
		Joint[] spawnJoints = SpawnJoints;
		foreach (Joint joint in spawnJoints)
		{
			if (joint.connectedBody == null)
			{
				list.Add(joint);
			}
			else if (forceDestroyLeftoversOnSpawn)
			{
				Destroyer.DestroyActor(joint.connectedBody.gameObject, "SpawnResource.Spawn#1");
				list.Add(joint);
			}
		}
		foreach (SpawnMetadata metadata in metadatas)
		{
			for (int j = 0; j < metadata.count; j++)
			{
				Joint joint2 = rand.Pluck(list, null);
				if (joint2 == null)
				{
					break;
				}
				Vector3 position = joint2.transform.position;
				Quaternion rotation = joint2.transform.rotation;
				GameObject gameObject = SRBehaviour.InstantiateActor(metadata.prefab, region.setId, position, rotation);
				ResourceCycle component = gameObject.GetComponent<ResourceCycle>();
				component.Attach(joint2, AdditionalRipenessPerSecond);
				if (MaxActiveSpawns != 0)
				{
					spawned.Add(gameObject);
				}
				if (metadata.request.spawnRipe)
				{
					component.ProgressResource(timeDir.WorldTime());
					continue;
				}
				if (metadata.request.spawnAtTime.HasValue)
				{
					component.ProgressResource(metadata.request.spawnAtTime.Value + (double)(component.unripeGameHours * 3600f));
					continue;
				}
				if (SpawnFX != null)
				{
					SRBehaviour.SpawnAndPlayFX(SpawnFX, position, Quaternion.identity);
				}
				TweenUtil.ScaleIn(gameObject, 4f);
			}
			if (MaxTotalSpawns != 0)
			{
				totalSpawnsRemaining -= metadata.count;
				if (totalSpawnsRemaining <= 0)
				{
					Destroyer.Destroy(base.gameObject, "SpawnResource.Spawn#2");
				}
			}
		}
	}

	private void UpdateNextSpawnTime(float preripenedHours)
	{
		model.nextSpawnTime = timeDir.HoursFromNow(0f - preripenedHours + rand.GetInRange(MinSpawnIntervalGameHours, MaxSpawnIntervalGameHours));
	}

	private void StepNextSpawnTime(float ripeness, float stepHours)
	{
		model.nextSpawnTime = TimeDirector.HoursFromTime(0f - ripeness + stepHours, model.nextSpawnTime);
	}

	public void RefreshSpawnJointObjectPositions()
	{
		Joint[] spawnJoints = SpawnJoints;
		for (int i = 0; i < spawnJoints.Length; i++)
		{
			FixedJoint fixedJoint = (FixedJoint)spawnJoints[i];
			if (fixedJoint.connectedBody != null)
			{
				fixedJoint.connectedBody.transform.position = fixedJoint.transform.position;
				fixedJoint.connectedBody.transform.rotation = fixedJoint.transform.rotation;
				RegionMember component = fixedJoint.connectedBody.GetComponent<RegionMember>();
				if (component != null)
				{
					component.UpdateRegionMembership(forceUpdateEvenWhenHibernating: true);
				}
			}
		}
	}

	public Joint PickRipeResourceJoint()
	{
		Joint[] spawnJoints = SpawnJoints;
		foreach (Joint joint in spawnJoints)
		{
			if (joint.connectedBody != null && joint.connectedBody.GetComponent<ResourceCycle>().GetState() == ResourceCycle.State.RIPE)
			{
				return joint;
			}
		}
		return null;
	}

	public Joint NearestJoint(Vector3 pos, float maxDist)
	{
		float num = maxDist * maxDist;
		Joint result = null;
		Joint[] spawnJoints = SpawnJoints;
		foreach (Joint joint in spawnJoints)
		{
			float sqrMagnitude = (joint.transform.position - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = joint;
			}
		}
		return result;
	}

	private bool AllJointsDisconnected()
	{
		return SpawnJoints.All((Joint j) => j.connectedBody == null);
	}

	public bool ReadyToDestroy()
	{
		model.nextSpawnTime = double.PositiveInfinity;
		return AllJointsDisconnected();
	}

	public void RegisterSpawnBlocker()
	{
		spawnBlockers++;
	}

	public void DeregisterSpawnBlocker()
	{
		spawnBlockers--;
	}

	public void FastForward(double startTime, double endTime)
	{
		UpdateToTime(endTime, endTime - startTime);
	}

	public IEnumerable<DroneFastForwarder.GatherGroup> GetFastForwardGroups(double endTime)
	{
		return (from m in spawnQueue.SelectMany((SpawnRequest r) => GetSpawnMetadatas(r, endTime))
			group m by Identifiable.GetId(m.prefab) into g
			select new GatherGroup(this, g.Key, g.Aggregate(0, (int agg, SpawnMetadata m) => agg + m.count))).Cast<DroneFastForwarder.GatherGroup>();
	}
}
