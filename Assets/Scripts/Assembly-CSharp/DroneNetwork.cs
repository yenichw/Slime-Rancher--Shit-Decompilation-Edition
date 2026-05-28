using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneNetwork : PathingNetwork
{
	public class LandPlotMetadata
	{
		public LandPlot plot;

		public GardenCatcher[] gardens;

		public StorageMetadata[] feeders;

		public StorageMetadata[] silos;

		public StorageMetadata[] plortCollectors;

		public StorageMetadata[] elderCollectors;

		public TrackContainedIdentifiables[] trackers;

		public Incinerate[] incinerators;

		public GardenDroneSubnetwork subnetwork;

		public bool Contains(Identifiable identifiable)
		{
			return trackers.Any((TrackContainedIdentifiables t) => t.Contains(identifiable));
		}
	}

	public class StorageMetadata
	{
		public SiloStorage storage;

		public SiloCatcher catcher;

		public int index;

		public Ammo ammo => storage.GetRelevantAmmo();

		public Identifiable.Id id => ammo.GetSlotName(index);

		public int count => ammo.GetSlotCount(index);

		public int maxCount => ammo.GetSlotMaxCount(index);

		public StorageMetadata()
		{
		}

		public StorageMetadata(StorageMetadata other)
		{
			storage = other.storage;
			catcher = other.catcher;
			index = other.index;
		}

		public bool CanCancel()
		{
			if (!(storage == null))
			{
				return !storage.gameObject.activeInHierarchy;
			}
			return true;
		}

		public int GetAvailableSpace(Identifiable.Id id)
		{
			if (this.id != id && this.id != 0)
			{
				return 0;
			}
			return maxCount - count;
		}

		public bool Increment(Identifiable.Id id, bool overflow, int count = 1)
		{
			return storage.MaybeAddIdentifiable(id, index, count, overflow);
		}

		public void Decrement(int count = 1)
		{
			ammo.Decrement(index, count);
		}

		public bool IsFull()
		{
			return count >= maxCount;
		}
	}

	public const float PATHING_THROTTLE_HRS = 1f / 30f;

	[HideInInspector]
	public double pathingThrottleUntil;

	private const float MAX_CONNECTION_DIST = 40f;

	private DronePather pather = new DronePather(40f);

	private List<LandPlotMetadata> plots = new List<LandPlotMetadata>();

	private List<Drone> drones = new List<Drone>();

	private List<SiloCatcher> refineryCatchers = new List<SiloCatcher>();

	private List<ScorePlort> markets = new List<ScorePlort>();

	public override Pather Pather => pather;

	public IEnumerable<LandPlotMetadata> Plots => plots;

	public IEnumerable<Drone> Drones => drones;

	public IEnumerable<SiloCatcher> RefineryCatchers => refineryCatchers;

	public IEnumerable<ScorePlort> Markets => markets;

	public void Register(LandPlot p)
	{
		plots.Add(new LandPlotMetadata
		{
			plot = p,
			trackers = p.GetComponentsInChildren<TrackContainedIdentifiables>(),
			subnetwork = p.GetComponentInChildren<GardenDroneSubnetwork>(),
			feeders = ((p.typeId == LandPlot.Id.CORRAL) ? GetStorageMetadata(from c in p.GetComponents<FeederUpgrader>()
				where c != null && c.feeder.activeInHierarchy
				select c.feeder.GetComponent<SiloStorage>()).ToArray() : new StorageMetadata[0]),
			plortCollectors = ((p.typeId == LandPlot.Id.CORRAL) ? GetStorageMetadata(from c in p.GetComponents<PlortCollectorUpgrader>()
				where c != null && c.collector.activeInHierarchy
				select c.collector.GetComponent<SiloStorage>()).ToArray() : new StorageMetadata[0]),
			elderCollectors = ((p.typeId == LandPlot.Id.COOP) ? GetStorageMetadata(from c in p.GetComponents<DeluxeCoopUpgrader>()
				where c != null && c.deluxeStuff.activeInHierarchy
				select c.deluxeStuff.GetComponentInChildren<SiloStorage>()).ToArray() : new StorageMetadata[0]),
			silos = ((p.typeId == LandPlot.Id.SILO) ? p.GetComponents<SiloStorage>().SelectMany((SiloStorage s) => from i in s.GetComponentsInChildren<SiloStorageActivator>().SelectMany((SiloStorageActivator a) => a.siloSlotUIs.Select((SiloSlotUI ui) => ui.slotIdx))
				select new StorageMetadata
				{
					index = i,
					storage = s,
					catcher = s.GetComponentsInChildren<SiloStorageActivator>().First((SiloStorageActivator a) => a.siloSlotUIs.Any((SiloSlotUI ui) => ui.slotIdx == i)).siloCatcher
				}).ToArray() : new StorageMetadata[0]),
			gardens = ((p.typeId == LandPlot.Id.GARDEN) ? p.GetComponentsInChildren<GardenCatcher>() : new GardenCatcher[0]),
			incinerators = ((p.typeId == LandPlot.Id.INCINERATOR) ? p.GetComponentsInChildren<Incinerate>() : new Incinerate[0])
		});
	}

	public bool Deregister(LandPlot deregister)
	{
		return plots.RemoveAll((LandPlotMetadata p) => p.plot == deregister) >= 1;
	}

	public void OnUpgradesChanged(LandPlot plot)
	{
		if (Deregister(plot))
		{
			Register(plot);
		}
	}

	public LandPlotMetadata GetContaining(Identifiable source)
	{
		return plots.FirstOrDefault((LandPlotMetadata m) => m.Contains(source));
	}

	private static IEnumerable<StorageMetadata> GetStorageMetadata(IEnumerable<SiloStorage> storages)
	{
		return storages.Select((SiloStorage s) => new
		{
			storage = s,
			ammo = s.GetRelevantAmmo()
		}).SelectMany(s => from i in Enumerable.Range(0, s.ammo.GetUsableSlotCount())
			select new StorageMetadata
			{
				index = i,
				storage = s.storage,
				catcher = s.storage.GetComponentsInChildren<SiloCatcher>().First((SiloCatcher c) => c.slotIdx == i)
			});
	}

	public void Register(Drone drone)
	{
		drones.Add(drone);
	}

	public bool Deregister(Drone deregister)
	{
		return drones.RemoveAll((Drone d) => d == deregister) >= 1;
	}

	public void Register(SiloCatcher catcher)
	{
		if (catcher.type == SiloCatcher.Type.REFINERY)
		{
			refineryCatchers.Add(catcher);
		}
	}

	public bool Deregister(SiloCatcher catcher)
	{
		if (catcher.type == SiloCatcher.Type.REFINERY)
		{
			return refineryCatchers.RemoveAll((SiloCatcher d) => d == catcher) >= 1;
		}
		return true;
	}

	public void Register(ScorePlort market)
	{
		markets.Add(market);
	}

	public bool Deregister(ScorePlort market)
	{
		return markets.RemoveAll((ScorePlort d) => d == market) >= 1;
	}

	public static bool IsResourceReady(GameObject go)
	{
		ResourceCycle component = go.GetComponent<ResourceCycle>();
		if (!(component == null) && component.GetState() != ResourceCycle.State.RIPE)
		{
			return component.GetState() == ResourceCycle.State.EDIBLE;
		}
		return true;
	}

	public static DroneNetwork Find(GameObject gameObject)
	{
		CellDirector componentInParent = gameObject.GetComponentInParent<CellDirector>();
		if (!(componentInParent == null))
		{
			return componentInParent.GetComponent<DroneNetwork>();
		}
		return null;
	}
}
