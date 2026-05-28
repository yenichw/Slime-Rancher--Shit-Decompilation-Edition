using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ZoneDirector : SRBehaviour, WorldModel.Participant
{
	public enum Zone
	{
		NONE = -1,
		RANCH = 0,
		REEF = 1,
		QUARRY = 2,
		MOSS = 3,
		DESERT = 4,
		SEA = 5,
		RUINS = 7,
		RUINS_TRANSITION = 8,
		WILDS = 9,
		OGDEN_RANCH = 10,
		VALLEY = 11,
		MOCHI_RANCH = 12,
		SLIMULATIONS = 13,
		VIKTOR_LAB = 14
	}

	[Serializable]
	public class AuxItemEntry
	{
		public Identifiable.Id item;

		public float weight;
	}

	public class ZoneComparer : IEqualityComparer<Zone>
	{
		public bool Equals(Zone x, Zone y)
		{
			return x == y;
		}

		public int GetHashCode(Zone obj)
		{
			return (int)obj;
		}
	}

	public Zone zone;

	public GameObject cratePrefab;

	public int minCrates = 3;

	public int maxCrates = 5;

	public int minAuxItems;

	public int maxAuxItems;

	public bool auxItemsFullClear;

	public AuxItemEntry[] auxItems;

	private WorldModel model;

	public static Dictionary<Zone, ZoneDirector> zones = new Dictionary<Zone, ZoneDirector>(zoneComparer);

	public static ZoneComparer zoneComparer = new ZoneComparer();

	public int MinGingerPatches;

	public int MaxGingerPatches;

	private TimeDirector timeDir;

	private LookupDirector lookupDir;

	private List<DirectedCrateSpawner> crateSpawners = new List<DirectedCrateSpawner>();

	private List<DirectedAuxItemSpawner> auxItemSpawners = new List<DirectedAuxItemSpawner>();

	private List<GingerPatchNode> gingerPatches = new List<GingerPatchNode>();

	private List<GingerPatchNode> currentGingerPatchNodes = new List<GingerPatchNode>();

	private List<KookadobaPatchNode> kookadobaPatches = new List<KookadobaPatchNode>();

	private double nextKookadobaTime;

	private static float KOOKADOBA_DELAY = 0.5f;

	private readonly Identifiable.Id[] CRATE_TYPES_TO_CLEAR = new Identifiable.Id[7]
	{
		Identifiable.Id.CRATE_REEF_01,
		Identifiable.Id.CRATE_QUARRY_01,
		Identifiable.Id.CRATE_MOSS_01,
		Identifiable.Id.CRATE_DESERT_01,
		Identifiable.Id.CRATE_RUINS_01,
		Identifiable.Id.CRATE_WILDS_01,
		Identifiable.Id.CRATE_PARTY_01
	};

	private readonly Dictionary<Identifiable.Id, float> auxItemDict = new Dictionary<Identifiable.Id, float>(Identifiable.idComparer);

	public static Dictionary<Zone, PediaDirector.Id> zonePediaIdLookup = new Dictionary<Zone, PediaDirector.Id>
	{
		{
			Zone.RANCH,
			PediaDirector.Id.THE_RANCH
		},
		{
			Zone.REEF,
			PediaDirector.Id.REEF
		},
		{
			Zone.QUARRY,
			PediaDirector.Id.QUARRY
		},
		{
			Zone.MOSS,
			PediaDirector.Id.MOSS
		},
		{
			Zone.DESERT,
			PediaDirector.Id.DESERT
		},
		{
			Zone.RUINS,
			PediaDirector.Id.RUINS
		},
		{
			Zone.RUINS_TRANSITION,
			PediaDirector.Id.RUINS
		},
		{
			Zone.SEA,
			PediaDirector.Id.SEA
		},
		{
			Zone.WILDS,
			PediaDirector.Id.WILDS
		},
		{
			Zone.OGDEN_RANCH,
			PediaDirector.Id.OGDEN_RETREAT
		},
		{
			Zone.MOCHI_RANCH,
			PediaDirector.Id.MOCHI_MANOR
		},
		{
			Zone.VALLEY,
			PediaDirector.Id.VALLEY
		},
		{
			Zone.SLIMULATIONS,
			PediaDirector.Id.SLIMULATIONS_WORLD
		},
		{
			Zone.VIKTOR_LAB,
			PediaDirector.Id.VIKTOR_LAB
		}
	};

	public RegionRegistry.RegionSetId regionSetId => GetRegionSetId(zone);

	public static RegionRegistry.RegionSetId GetRegionSetId(Zone zone)
	{
		switch (zone)
		{
		case Zone.RANCH:
		case Zone.REEF:
		case Zone.QUARRY:
		case Zone.MOSS:
		case Zone.SEA:
		case Zone.RUINS:
		case Zone.RUINS_TRANSITION:
		case Zone.WILDS:
		case Zone.OGDEN_RANCH:
			return RegionRegistry.RegionSetId.HOME;
		case Zone.DESERT:
			return RegionRegistry.RegionSetId.DESERT;
		case Zone.VALLEY:
		case Zone.MOCHI_RANCH:
			return RegionRegistry.RegionSetId.VALLEY;
		case Zone.VIKTOR_LAB:
			return RegionRegistry.RegionSetId.VIKTOR_LAB;
		case Zone.SLIMULATIONS:
			return RegionRegistry.RegionSetId.SLIMULATIONS;
		default:
			throw new ArgumentException($"Failed to get RegionSetId from Zone. [zone={zone}]");
		}
	}

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		zones.Add(zone, this);
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void OnDestroy()
	{
		zones.Remove(zone);
	}

	public void Start()
	{
		AuxItemEntry[] array = auxItems;
		foreach (AuxItemEntry auxItemEntry in array)
		{
			auxItemDict[auxItemEntry.item] = auxItemEntry.weight;
		}
		if (SRSingleton<GameContext>.Instance.AutoSaveDirector.IsNewGame())
		{
			ResetCrates();
			ResetAuxItems();
			ResetGingerPatches();
		}
	}

	public void Update()
	{
		if (timeDir.OnPassedHour(5f))
		{
			if (SRSingleton<LockOnDeath>.Instance != null && SRSingleton<LockOnDeath>.Instance.Locked())
			{
				ResetCrates();
				if (auxItemsFullClear)
				{
					ResetAuxItems();
				}
			}
			if (!auxItemsFullClear)
			{
				ResetAuxItems();
			}
			ResetGingerPatches();
		}
		if (timeDir.HasReached(nextKookadobaTime))
		{
			SpawnKookadoba();
			nextKookadobaTime = timeDir.HoursFromNow(KOOKADOBA_DELAY);
		}
	}

	private void SpawnKookadoba()
	{
		KookadobaPatchNode kookadobaPatchNode = Randoms.SHARED.Pick(kookadobaPatches, null);
		if (kookadobaPatchNode != null)
		{
			kookadobaPatchNode.Grow();
		}
	}

	public static HashSet<Zone> Zones(RegionMember member)
	{
		HashSet<Zone> hashSet = new HashSet<Zone>();
		foreach (Region region in member.regions)
		{
			ZoneDirector componentInParent = region.GetComponentInParent<ZoneDirector>();
			if (componentInParent != null)
			{
				hashSet.Add(componentInParent.zone);
			}
		}
		return hashSet;
	}

	public static HashSet<Zone> Zones(GameObject gameObject)
	{
		RegionMember component = gameObject.GetComponent<RegionMember>();
		if (!(component != null))
		{
			return new HashSet<Zone>();
		}
		return Zones(component);
	}

	public void Register(DirectedCrateSpawner spawner)
	{
		crateSpawners.Add(spawner);
	}

	public void Register(DirectedAuxItemSpawner spawner)
	{
		auxItemSpawners.Add(spawner);
	}

	public void Register(GingerPatchNode gingerPatch)
	{
		gingerPatches.Add(gingerPatch);
	}

	public void Register(KookadobaPatchNode kookadobaPatch)
	{
		kookadobaPatches.Add(kookadobaPatch);
	}

	public void ResetCrates()
	{
		CellDirector[] componentsInChildren = GetComponentsInChildren<CellDirector>();
		foreach (CellDirector cellDirector in componentsInChildren)
		{
			List<GameObject> result = new List<GameObject>();
			Identifiable.Id[] cRATE_TYPES_TO_CLEAR = CRATE_TYPES_TO_CLEAR;
			foreach (Identifiable.Id id in cRATE_TYPES_TO_CLEAR)
			{
				cellDirector.Get(id, ref result);
			}
			foreach (GameObject item in result)
			{
				Destroyer.DestroyActor(item, "ZoneDirector.ResetCrates");
			}
		}
		if (cratePrefab == null)
		{
			Log.Warning("Zone missing crate prefab: " + base.gameObject.name);
			return;
		}
		List<DirectedCrateSpawner> list = new List<DirectedCrateSpawner>(crateSpawners);
		int inRange = Randoms.SHARED.GetInRange(minCrates, maxCrates + 1);
		for (int k = 0; k < inRange; k++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			Randoms.SHARED.Pluck(list, null).Spawn(cratePrefab);
		}
	}

	public void ResetGingerPatches()
	{
		if (gingerPatches.Count == 0)
		{
			return;
		}
		if (currentGingerPatchNodes.Count > 0)
		{
			foreach (GingerPatchNode currentGingerPatchNode in currentGingerPatchNodes)
			{
				currentGingerPatchNode.HidePatchAndReset();
				model.currentGingerPatchIds.Remove(currentGingerPatchNode.id);
			}
			currentGingerPatchNodes.Clear();
		}
		List<GingerPatchNode> list = new List<GingerPatchNode>(gingerPatches);
		int inRange = Randoms.SHARED.GetInRange(Math.Max(MinGingerPatches, 0), Math.Min(MaxGingerPatches, list.Count));
		for (int i = 0; i < inRange; i++)
		{
			GingerPatchNode gingerPatchNode = Randoms.SHARED.Pluck(list, null);
			if (gingerPatchNode != null)
			{
				currentGingerPatchNodes.Add(gingerPatchNode);
				model.currentGingerPatchIds.Add(gingerPatchNode.id);
				gingerPatchNode.Grow();
			}
		}
	}

	private void SetCurrentGingerPatch(List<string> gingerPatchIds)
	{
		currentGingerPatchNodes = (from id in gingerPatchIds
			select gingerPatches.FirstOrDefault((GingerPatchNode patch) => string.Compare(patch.id, id, ignoreCase: false, CultureInfo.InvariantCulture) == 0) into patch
			where patch != null
			select patch).ToList();
	}

	public List<GingerPatchNode> GetCurrentGingerPatches()
	{
		return currentGingerPatchNodes;
	}

	private void ResetAuxItems()
	{
		if ((auxItems == null || auxItems.Length == 0) && maxAuxItems > 0)
		{
			Log.Warning("Zone missing aux item prefab: " + base.gameObject.name);
		}
		else
		{
			if (maxAuxItems <= 0)
			{
				return;
			}
			if (auxItemsFullClear)
			{
				CellDirector[] componentsInChildren = GetComponentsInChildren<CellDirector>();
				foreach (CellDirector cellDirector in componentsInChildren)
				{
					List<GameObject> result = new List<GameObject>();
					AuxItemEntry[] array = auxItems;
					foreach (AuxItemEntry auxItemEntry in array)
					{
						cellDirector.Get(auxItemEntry.item, ref result);
					}
					foreach (GameObject item in result)
					{
						Destroyer.DestroyActor(item, "ZoneDirector.ResetAuxItems");
					}
				}
			}
			else
			{
				foreach (DirectedAuxItemSpawner auxItemSpawner in auxItemSpawners)
				{
					auxItemSpawner.UnspawnIfPresent(auxItemDict.Keys);
				}
			}
			List<DirectedAuxItemSpawner> list = new List<DirectedAuxItemSpawner>(auxItemSpawners);
			int inRange = Randoms.SHARED.GetInRange(minAuxItems, maxAuxItems + 1);
			for (int k = 0; k < inRange; k++)
			{
				if (list.Count <= 0)
				{
					break;
				}
				DirectedAuxItemSpawner directedAuxItemSpawner = Randoms.SHARED.Pluck(list, null);
				Identifiable.Id id = PickAuxItem();
				GameObject prefab = lookupDir.GetPrefab(id);
				directedAuxItemSpawner.Spawn(prefab);
			}
		}
	}

	public Identifiable.Id PickAuxItem()
	{
		return Randoms.SHARED.Pick(auxItemDict, Identifiable.Id.NONE);
	}

	public ICollection<Identifiable.Id> GetAllAuxItems()
	{
		return auxItemDict.Keys;
	}

	public static bool HasAccessToZone(Zone zone)
	{
		switch (zone)
		{
		case Zone.DESERT:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_DESERT);
		case Zone.MOSS:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_MOSS);
		case Zone.QUARRY:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_QUARRY);
		case Zone.RUINS:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_RUINS);
		case Zone.RUINS_TRANSITION:
			if (!HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_MOSS))
			{
				return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_QUARRY);
			}
			return true;
		case Zone.OGDEN_RANCH:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_OGDEN_MISSIONS);
		case Zone.WILDS:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_WILDS);
		case Zone.MOCHI_RANCH:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_MOCHI_MISSIONS);
		case Zone.VALLEY:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_VALLEY);
		case Zone.SLIMULATIONS:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_SLIMULATIONS);
		case Zone.VIKTOR_LAB:
			return HasProgressForZone(ProgressDirector.ProgressType.UNLOCK_VIKTOR_MISSIONS);
		case Zone.RANCH:
		case Zone.REEF:
		case Zone.SEA:
			return true;
		default:
			return false;
		}
	}

	private static bool HasProgressForZone(ProgressDirector.ProgressType progressType)
	{
		return SRSingleton<SceneContext>.Instance.ProgressDirector.HasProgress(progressType);
	}

	public Sprite GetZoneIcon()
	{
		if (zonePediaIdLookup.ContainsKey(zone))
		{
			PediaDirector.IdEntry idEntry = SRSingleton<SceneContext>.Instance.PediaDirector.Get(zonePediaIdLookup[zone]);
			if (idEntry != null)
			{
				return idEntry.icon;
			}
		}
		return null;
	}

	public static Sprite GetZoneIcon(GameObject gameObject)
	{
		Sprite result = null;
		ZoneDirector componentInParent = gameObject.GetComponentInParent<ZoneDirector>();
		if (componentInParent != null)
		{
			result = componentInParent.GetZoneIcon();
		}
		return result;
	}

	public void InitModel(WorldModel model)
	{
	}

	public void SetModel(WorldModel model)
	{
		this.model = model;
		if (gingerPatches.Count > 0 && model.currentGingerPatchIds.Count > 0)
		{
			SetCurrentGingerPatch(model.currentGingerPatchIds);
		}
	}
}
