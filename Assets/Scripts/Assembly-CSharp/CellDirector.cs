using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class CellDirector : SRBehaviour
{
	public delegate void OnSlimeAdded(Identifiable.Id slimeId);

	public delegate void OnSlimeRemoved(Identifiable.Id slimeId);

	public const string RANCH_HOME = "cellRanch_Home";

	public const string RANCH_LAB = "cellRanch_Lab";

	public int cullSlimesLimit = 250;

	public int cullAnimalsLimit = 50;

	public int targetSlimeCount = 100;

	public int targetAnimalCount = 20;

	public int minPerSpawn = 3;

	public int maxPerSpawn = 5;

	public int minAnimalPerSpawn = 3;

	public int maxAnimalPerSpawn = 5;

	public float despawnFactor = 1.2f;

	public float avgSpawnTimeGameHours = 2f;

	public bool isRanch;

	public bool isHomeRanch;

	public bool isWilds;

	public AmbianceDirector.Zone ambianceZone;

	public bool ignoreCoopCorralAnimals;

	public OnSlimeAdded onSlimeAdded;

	public OnSlimeRemoved onSlimeRemoved;

	public bool notShownOnMap;

	private List<DirectedSlimeSpawner> spawners = new List<DirectedSlimeSpawner>();

	private List<DirectedAnimalSpawner> animalSpawners = new List<DirectedAnimalSpawner>();

	protected Randoms rand;

	public GameObjectActorModelIdentifiableIndex identifiableIndex = new GameObjectActorModelIdentifiableIndex();

	protected int tarrSlimeCount;

	private double nextSpawn = double.PositiveInfinity;

	private TimeDirector timeDir;

	private GameObject player;

	private ZoneDirector zoneDirector;

	private float spawnThrottleTime;

	private const float SPAWN_THROTTLE_DELAY = 1f;

	private const float PCT_OF_TARGET_TO_DESPAWN = 0.1f;

	private const float PROB_DESTROY_ON_SLEEP = 0.5f;

	private static List<CellDirector> allCellDirectors = new List<CellDirector>();

	private static HashSet<GameObject> selectedGameObjects = new HashSet<GameObject>();

	private static HashSet<GameObjectActorModelIdentifiableIndex.Entry> selectedGameObjectEntries = new HashSet<GameObjectActorModelIdentifiableIndex.Entry>();

	public Region region { get; private set; }

	public virtual void Awake()
	{
		allCellDirectors.Add(this);
		rand = new Randoms();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		nextSpawn = timeDir.HoursFromNowOrStart((0f - avgSpawnTimeGameHours) * (float)targetSlimeCount / (float)maxPerSpawn);
		region = GetComponent<Region>();
		zoneDirector = GetComponentInParent<ZoneDirector>();
	}

	public void OnDestroy()
	{
		allCellDirectors.Remove(this);
	}

	public ZoneDirector.Zone GetZoneId()
	{
		if (zoneDirector != null)
		{
			return zoneDirector.zone;
		}
		return ZoneDirector.Zone.NONE;
	}

	public void Start()
	{
		player = SRSingleton<SceneContext>.Instance.Player;
	}

	public virtual void ForceCheckSpawn()
	{
		spawnThrottleTime = 0f;
		nextSpawn = 0.0;
	}

	public void Update()
	{
		if (Time.time >= spawnThrottleTime && !region.Hibernated)
		{
			UpdateToTime(timeDir.WorldTime());
			if (identifiableIndex.GetSlimes().Count() > cullSlimesLimit)
			{
				Despawn(identifiableIndex.GetSlimes(), identifiableIndex.GetSlimes().Count() - cullSlimesLimit);
			}
			if (identifiableIndex.GetAnimals().Count() > cullAnimalsLimit)
			{
				Despawn(identifiableIndex.GetAnimals(), identifiableIndex.GetAnimals().Count() - cullAnimalsLimit);
			}
			spawnThrottleTime = Time.time + 1f;
		}
	}

	protected virtual void UpdateToTime(double worldTime)
	{
		if (!TimeUtil.HasReached(worldTime, nextSpawn))
		{
			return;
		}
		if (spawners.Count > 0 && NeedsMoreSlimes() && CanSpawnSlimes())
		{
			Dictionary<DirectedSlimeSpawner, float> dictionary = new Dictionary<DirectedSlimeSpawner, float>();
			float num = 0f;
			foreach (DirectedSlimeSpawner spawner in spawners)
			{
				if (spawner.CanSpawn())
				{
					dictionary[spawner] = spawner.directedSpawnWeight;
					num += spawner.directedSpawnWeight;
				}
			}
			if (dictionary.Count > 0 && num > 0f)
			{
				DirectedSlimeSpawner directedSlimeSpawner = rand.Pick(dictionary, null);
				float num2 = SRSingleton<SceneContext>.Instance.ModDirector.SlimeCountFactor();
				StartCoroutine(directedSlimeSpawner.Spawn(Mathf.RoundToInt((float)rand.GetInRange(minPerSpawn, maxPerSpawn + 1) * num2), rand));
			}
		}
		else if (HasTooManySlimes())
		{
			Despawn(identifiableIndex.GetSlimes(), Mathf.CeilToInt(0.1f * (float)targetSlimeCount));
		}
		if (animalSpawners.Count > 0 && NeedsMoreAnimals())
		{
			List<DirectedAnimalSpawner> list = new List<DirectedAnimalSpawner>();
			foreach (DirectedAnimalSpawner animalSpawner in animalSpawners)
			{
				if (animalSpawner.CanSpawn())
				{
					list.Add(animalSpawner);
				}
			}
			if (list.Count > 0)
			{
				DirectedAnimalSpawner directedAnimalSpawner = rand.Pick(list, null);
				StartCoroutine(directedAnimalSpawner.Spawn(rand.GetInRange(minAnimalPerSpawn, maxAnimalPerSpawn + 1), rand));
			}
		}
		else if (HasTooManyAnimals())
		{
			Despawn(identifiableIndex.GetAnimals(), Mathf.CeilToInt(0.1f * (float)targetAnimalCount));
		}
		nextSpawn += avgSpawnTimeGameHours * 3600f * rand.GetInRange(0.5f, 1.5f);
	}

	private void Despawn(IEnumerable<GameObjectActorModelIdentifiableIndex.Entry> actorList, int count)
	{
		if (isRanch && actorList.Any((GameObjectActorModelIdentifiableIndex.Entry e) => Identifiable.IsSlime(e.Id)))
		{
			Log.Error("CellDirector is despawning slimes on the ranch... " + new
			{
				gameObject = base.gameObject.name,
				cullSlimesLimit = cullSlimesLimit,
				targetSlimeCount = targetSlimeCount,
				despawnFactor = despawnFactor,
				slimesCount = identifiableIndex.GetSlimeCount(),
				hasTooManySlimes = HasTooManySlimes()
			});
			SentrySdk.CaptureMessage("CellDirector is despawning slimes on the ranch!");
		}
		Dictionary<GameObject, float> dictionary = new Dictionary<GameObject, float>();
		foreach (GameObjectActorModelIdentifiableIndex.Entry actor in actorList)
		{
			dictionary[actor.GameObject] = GetDespawnWeight(actor.GameObject);
		}
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = rand.Pick(dictionary, null);
			dictionary.Remove(gameObject);
			Destroyer.DestroyActor(gameObject, "CellDirector.Despawn");
		}
	}

	private float GetDespawnWeight(GameObject actor)
	{
		float num = (player.transform.position - actor.transform.position).sqrMagnitude;
		Vacuumable component = actor.GetComponent<Vacuumable>();
		if (component != null)
		{
			num *= GetSizeMultiplier(component.size);
		}
		return num;
	}

	private float GetSizeMultiplier(Vacuumable.Size size)
	{
		switch (size)
		{
		case Vacuumable.Size.NORMAL:
			return 1f;
		case Vacuumable.Size.LARGE:
			return 0.5f;
		case Vacuumable.Size.GIANT:
			return 0.25f;
		default:
			return 1f;
		}
	}

	public void Register(DirectedSlimeSpawner spawner)
	{
		if (spawner.allowDirectedSpawns)
		{
			spawners.Add(spawner);
		}
	}

	public void Register(DirectedAnimalSpawner spawner)
	{
		animalSpawners.Add(spawner);
	}

	public void Register(GameObject obj, ActorModel actorModel)
	{
		Identifiable.Id ident = actorModel.ident;
		identifiableIndex.Register(obj, actorModel);
		if (Identifiable.IsTarr(ident))
		{
			tarrSlimeCount++;
		}
		if (Identifiable.IsSlime(ident) && onSlimeAdded != null)
		{
			onSlimeAdded(ident);
		}
	}

	public void Unregister(GameObject obj, ActorModel actorModel)
	{
		Identifiable.Id ident = actorModel.ident;
		identifiableIndex.Deregister(obj, actorModel);
		if (Identifiable.IsTarr(ident))
		{
			tarrSlimeCount--;
		}
		if (Identifiable.IsSlime(ident) && onSlimeRemoved != null)
		{
			onSlimeRemoved(ident);
		}
	}

	public void Get(Identifiable.Id id, ref List<GameObject> result)
	{
		result.AddRange(from entry in identifiableIndex.GetObjectsByIdentifiableId(id)
			select entry.GameObject);
	}

	public void Get(Identifiable.Id id, List<GameObject> result, HashSet<GameObject> toIgnore)
	{
		AddUniquesFromList(identifiableIndex.GetObjectsByIdentifiableId(id), result, toIgnore);
	}

	public void Get(IEnumerable<Identifiable.Id> ids, List<GameObjectActorModelIdentifiableIndex.Entry> result, HashSet<GameObjectActorModelIdentifiableIndex.Entry> toIgnore)
	{
		foreach (Identifiable.Id id in ids)
		{
			AddUniquesFromList(identifiableIndex.GetObjectsByIdentifiableId(id), result, toIgnore);
		}
	}

	public void Get(IEnumerable<Identifiable.Id> ids, List<GameObject> results)
	{
		selectedGameObjects.Clear();
		foreach (Identifiable.Id id in ids)
		{
			Get(id, results, selectedGameObjects);
		}
		selectedGameObjects.Clear();
	}

	public void GetToys(IList<GameObjectActorModelIdentifiableIndex.Entry> results, HashSet<GameObjectActorModelIdentifiableIndex.Entry> toIgnore)
	{
		AddUniquesFromList(identifiableIndex.GetToys(), results, toIgnore);
	}

	public void GetLargos(IList<GameObjectActorModelIdentifiableIndex.Entry> results, HashSet<GameObjectActorModelIdentifiableIndex.Entry> toIgnore)
	{
		AddUniquesFromList(identifiableIndex.GetLargos(), results, toIgnore);
	}

	public void GetSlimes(List<GameObject> result, HashSet<GameObject> toIgnore)
	{
		AddUniquesFromList(identifiableIndex.GetSlimes(), result, toIgnore);
	}

	private void AddUniquesFromList(IList<GameObjectActorModelIdentifiableIndex.Entry> sourceList, IList<GameObjectActorModelIdentifiableIndex.Entry> targetList, HashSet<GameObjectActorModelIdentifiableIndex.Entry> targetListLookup)
	{
		for (int i = 0; i < sourceList.Count; i++)
		{
			GameObjectActorModelIdentifiableIndex.Entry item = sourceList[i];
			if (!targetListLookup.Contains(item))
			{
				targetList.Add(item);
				targetListLookup.Add(item);
			}
		}
	}

	private void AddUniquesFromList(IList<GameObjectActorModelIdentifiableIndex.Entry> sourceList, List<GameObject> targetList, HashSet<GameObject> targetListLookup)
	{
		for (int i = 0; i < sourceList.Count; i++)
		{
			GameObject item = sourceList[i].GameObject;
			if (!targetListLookup.Contains(item))
			{
				targetList.Add(item);
				targetListLookup.Add(item);
			}
		}
	}

	public static void Get(Identifiable.Id id, RegionMember nearMember, List<GameObject> results)
	{
		selectedGameObjects.Clear();
		for (int i = 0; i < nearMember.regions.Count; i++)
		{
			nearMember.regions[i].cellDir.Get(id, results, selectedGameObjects);
		}
		selectedGameObjects.Clear();
	}

	public static void Get(IEnumerable<Identifiable.Id> ids, RegionMember nearMember, List<GameObjectActorModelIdentifiableIndex.Entry> results)
	{
		selectedGameObjectEntries.Clear();
		for (int i = 0; i < nearMember.regions.Count; i++)
		{
			nearMember.regions[i].cellDir.Get(ids, results, selectedGameObjectEntries);
		}
		selectedGameObjectEntries.Clear();
	}

	public static void GetToysNearMember(RegionMember nearMember, IList<GameObjectActorModelIdentifiableIndex.Entry> results)
	{
		selectedGameObjectEntries.Clear();
		for (int i = 0; i < nearMember.regions.Count; i++)
		{
			nearMember.regions[i].cellDir.GetToys(results, selectedGameObjectEntries);
		}
		selectedGameObjectEntries.Clear();
	}

	public static void GetLargosNearMember(RegionMember nearMember, IList<GameObjectActorModelIdentifiableIndex.Entry> results)
	{
		selectedGameObjectEntries.Clear();
		for (int i = 0; i < nearMember.regions.Count; i++)
		{
			nearMember.regions[i].cellDir.GetLargos(results, selectedGameObjectEntries);
		}
		selectedGameObjectEntries.Clear();
	}

	public static void GetSlimes(RegionMember nearMember, List<GameObject> results)
	{
		selectedGameObjects.Clear();
		for (int i = 0; i < nearMember.regions.Count; i++)
		{
			nearMember.regions[i].cellDir.GetSlimes(results, selectedGameObjects);
		}
		selectedGameObjects.Clear();
	}

	public static void UnregisterFromAll(RegionMember member, GameObject gameObj, ActorModel actorModel)
	{
		foreach (Region region in member.regions)
		{
			region.cellDir.Unregister(gameObj, actorModel);
		}
	}

	public static bool IsOnRanch(RegionMember member)
	{
		if (member.regions.Count == 0)
		{
			return false;
		}
		foreach (Region region in member.regions)
		{
			if (!region.cellDir.isRanch)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsOnHomeRanch(RegionMember member)
	{
		if (member.regions.Count == 0)
		{
			return false;
		}
		foreach (Region region in member.regions)
		{
			if (!region.cellDir.isHomeRanch)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsInWilds(RegionMember member)
	{
		if (member.regions.Count == 0)
		{
			return false;
		}
		foreach (Region region in member.regions)
		{
			if (!region.cellDir.isWilds)
			{
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<GameObjectActorModelIdentifiableIndex.Entry> GetAllRegistered()
	{
		foreach (CellDirector allCellDirector in allCellDirectors)
		{
			foreach (GameObjectActorModelIdentifiableIndex.Entry item in allCellDirector.identifiableIndex.GetAllRegistered())
			{
				yield return item;
			}
		}
	}

	protected virtual bool CanSpawnSlimes()
	{
		return tarrSlimeCount <= 0;
	}

	private bool NeedsMoreSlimes()
	{
		return identifiableIndex.GetSlimeCount() < targetSlimeCount;
	}

	private bool NeedsMoreAnimals()
	{
		return GetNonignoredAnimalCount() < targetAnimalCount;
	}

	private bool HasTooManySlimes()
	{
		return (float)identifiableIndex.GetSlimeCount() > (float)targetSlimeCount * despawnFactor;
	}

	private bool HasTooManyAnimals()
	{
		return (float)GetNonignoredAnimalCount() > (float)targetAnimalCount * despawnFactor;
	}

	private int GetNonignoredAnimalCount()
	{
		if (ignoreCoopCorralAnimals)
		{
			int num = 0;
			{
				foreach (GameObject item in from entry in identifiableIndex.GetAnimals()
					select entry.GameObject)
				{
					Vector3 position = item.transform.position;
					if (!CoopRegion.IsWithin(position) && !CorralRegion.IsWithin(position))
					{
						num++;
					}
				}
				return num;
			}
		}
		return identifiableIndex.GetAnimalCount();
	}
}
