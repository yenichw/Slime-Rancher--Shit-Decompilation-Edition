using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

[RequireComponent(typeof(CellDirector))]
[RequireComponent(typeof(DroneNetwork))]
[RequireComponent(typeof(Region))]
public class RanchCellFastForwarder : IdHandler, RanchCellModel.Participant
{
	public abstract class FeedingSource
	{
		public class Basic : FeedingSource
		{
			protected Identifiable.Id id;

			protected int remaining;

			public override IEnumerable<Identifiable.Id> ids
			{
				get
				{
					if (remaining > 0)
					{
						yield return id;
					}
				}
			}

			public Basic()
			{
			}

			public Basic(Identifiable.Id id, int remaining)
			{
				this.id = id;
				this.remaining = remaining;
			}

			public override bool Selected(Identifiable.Id id)
			{
				if (base.Selected(id))
				{
					remaining--;
					return true;
				}
				return false;
			}
		}

		public class AutoFeeder : Basic
		{
			private SlimeFeeder feeder;

			public AutoFeeder(DroneNetwork.LandPlotMetadata metadata, double endTime)
			{
				feeder = metadata.plot.GetComponentInChildren<SlimeFeeder>();
				if (feeder != null)
				{
					feeder.UpdateToTime(endTime);
					id = feeder.GetFoodId();
					remaining = feeder.RemainingFeedOperationsFastForward();
				}
			}

			public override bool Selected(Identifiable.Id id)
			{
				if (base.Selected(id))
				{
					feeder.ProcessFeedOperationFastForward();
					return true;
				}
				return false;
			}
		}

		public class Dynamic : FeedingSource
		{
			private TrackContainedIdentifiables container;

			public override IEnumerable<Identifiable.Id> ids => from id in container.GetTrackedIdentifiableTypes()
				where Identifiable.IsFood(id)
				select id;

			public Dynamic(TrackContainedIdentifiables container)
			{
				this.container = container;
			}

			public override GameObject GetTarget(Identifiable.Id id)
			{
				return container.RemoveTrackedObject(id).gameObject;
			}
		}

		public class LiquidSource : FeedingSource
		{
			private global::LiquidSource source;

			public override IEnumerable<Identifiable.Id> ids
			{
				get
				{
					yield return source.liquidId;
				}
			}

			public LiquidSource(global::LiquidSource source)
			{
				this.source = source;
			}

			public override bool Selected(Identifiable.Id id)
			{
				if (source.Available())
				{
					source.ConsumeLiquid();
					return true;
				}
				return false;
			}

			public override GameObject GetTarget(Identifiable.Id id)
			{
				return source.gameObject;
			}
		}

		public abstract IEnumerable<Identifiable.Id> ids { get; }

		public virtual bool Selected(Identifiable.Id id)
		{
			return true;
		}

		public virtual GameObject GetTarget(Identifiable.Id id)
		{
			return null;
		}
	}

	private RanchCellModel model;

	private static List<GameObject> HUNGRY_SLIMES = new List<GameObject>();

	private static List<Identifiable.Id> PRODUCED = new List<Identifiable.Id>();

	private static List<Identifiable.Id> COLLECTED = new List<Identifiable.Id>();

	private const double FASTFORWARD_MIN_HOURS = 2.0;

	private const double FASTFORWARD_MIN_SECS = 7200.0;

	private const double FASTFORWARD_CHUNK_HOURS = 4.0;

	private const double FASTFORWARD_CHUNK_SECS = 14400.0;

	private DroneNetwork network;

	private Region region;

	private TimeDirector timeDirector;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		network = GetComponent<DroneNetwork>();
		region = GetComponent<Region>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterRanchCell(base.id, this);
	}

	public void Start()
	{
		region.onHibernationStateChanged += OnHibernationStateChanged;
		timeDirector.onFastForwardChanged += OnFastForwardChanged;
	}

	public void InitModel(RanchCellModel model)
	{
	}

	public void SetModel(RanchCellModel model)
	{
		this.model = model;
	}

	public void Update()
	{
		if (model.sleepingTime.HasValue)
		{
			double num = model.sleepingTime.Value + 14400.0;
			if (timeDirector.HasReached(num) || !timeDirector.IsFastForwarding())
			{
				num = Math.Min(num, timeDirector.WorldTime());
				FastForwardDrones(model.sleepingTime.Value, num);
				bool flag = timeDirector.IsFastForwarding() && AnyDronesActive(num);
				model.sleepingTime = (flag ? new double?(num) : null);
			}
		}
	}

	public void OnDestroy()
	{
		if (timeDirector != null)
		{
			timeDirector.onFastForwardChanged -= OnFastForwardChanged;
			timeDirector = null;
		}
		if (region != null)
		{
			region.onHibernationStateChanged -= OnHibernationStateChanged;
			region = null;
		}
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterRanchCell(base.id);
		}
	}

	protected override string IdPrefix()
	{
		return "ranch";
	}

	private void OnHibernationStateChanged(bool hibernating)
	{
		if (hibernating)
		{
			OnHibernation();
		}
		else
		{
			SRSingleton<GameContext>.Instance.StartCoroutine(OnFastForward());
		}
	}

	private void OnHibernation()
	{
		if (!model.hibernationTime.HasValue)
		{
			model.hibernationTime = timeDirector.WorldTime();
		}
	}

	private IEnumerator OnFastForward()
	{
		if (!model.hibernationTime.HasValue)
		{
			yield break;
		}
		double startTime = model.hibernationTime.Value;
		double endTime = timeDirector.WorldTime();
		model.hibernationTime = null;
		if (!(endTime - startTime >= 7200.0))
		{
			yield break;
		}
		try
		{
			DroneFastForwarder.FastForward_Pre(this);
			while (startTime < endTime)
			{
				double chunkEndTime = (AnyDronesActive(startTime) ? Math.Min(endTime, startTime + 14400.0) : endTime);
				FastForwardCorrals(startTime, chunkEndTime);
				FastForwardGardens(startTime, chunkEndTime);
				FastForwardPonds(startTime, chunkEndTime);
				yield return new WaitForFixedUpdate();
				FastForwardDrones(startTime, chunkEndTime);
				startTime = chunkEndTime;
			}
		}
		finally
		{
			DroneFastForwarder.FastForward_Post(this);
		}
	}

	private void FastForwardCorrals(double startTime, double endTime)
	{
		foreach (DroneNetwork.LandPlotMetadata plot in network.Plots)
		{
			if (plot.plot.typeId == LandPlot.Id.CORRAL)
			{
				FeedSlimes(plot, endTime, new FeedingSource.AutoFeeder(plot, endTime), new FeedingSource.Dynamic(plot.trackers.First()));
			}
		}
	}

	private void FastForwardPonds(double startTime, double endTime)
	{
		foreach (DroneNetwork.LandPlotMetadata plot in network.Plots)
		{
			if (plot.plot.typeId == LandPlot.Id.POND)
			{
				LiquidSource componentInChildren = plot.plot.GetComponentInChildren<LiquidSource>();
				FeedSlimes(plot, endTime, new FeedingSource.LiquidSource(componentInChildren));
			}
		}
	}

	public static int FeedSlimes(DroneNetwork.LandPlotMetadata metadata, double endTime, params FeedingSource[] sources)
	{
		int num = 0;
		HUNGRY_SLIMES.Clear();
		PRODUCED.Clear();
		COLLECTED.Clear();
		if (sources.Any((FeedingSource s) => s.ids.Any()))
		{
			metadata.trackers.First().GetTrackedItemsOfClass(Identifiable.EATERS_CLASS, HUNGRY_SLIMES);
			while (HUNGRY_SLIMES.Any() && sources.Any((FeedingSource s) => s.ids.Any()))
			{
				GameObject gameObject = Randoms.SHARED.Pluck(HUNGRY_SLIMES, null);
				gameObject.GetComponent<SlimeEmotions>().UpdateToTime(endTime);
				if (FeedSlime(metadata, gameObject, sources))
				{
					HUNGRY_SLIMES.Add(gameObject);
					num++;
				}
			}
			HUNGRY_SLIMES.Clear();
			PRODUCED.Clear();
			COLLECTED.Clear();
		}
		return num;
	}

	private static bool FeedSlime(DroneNetwork.LandPlotMetadata metadata, GameObject slime, FeedingSource[] sources)
	{
		switch (Identifiable.GetId(slime))
		{
		case Identifiable.Id.PUDDLE_SLIME:
			return FeedSlime(metadata, slime.GetComponent<SlimeEatWater>(), sources);
		case Identifiable.Id.FIRE_SLIME:
			return FeedSlime(metadata, slime.GetComponent<SlimeEatAsh>(), sources);
		default:
			return FeedSlime(metadata, slime.GetComponent<SlimeEat>(), sources);
		}
	}

	private static bool FeedSlime(DroneNetwork.LandPlotMetadata metadata, SlimeEat eat, FeedingSource[] sources)
	{
		if (eat.WantsToEat())
		{
			PlortCollector componentInChildren = metadata.plot.GetComponentInChildren<PlortCollector>();
			foreach (FeedingSource feedingSource in sources)
			{
				foreach (Identifiable.Id id in feedingSource.ids)
				{
					if (eat.WillNowEat(id) && feedingSource.Selected(id))
					{
						PRODUCED = eat.GetProducedIds(id, PRODUCED);
						COLLECTED.Clear();
						if (componentInChildren != null)
						{
							componentInChildren.FastForward(PRODUCED, COLLECTED);
						}
						eat.EatImmediate(feedingSource.GetTarget(id), id, PRODUCED, COLLECTED, skipDelays: true);
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool FeedSlime(DroneNetwork.LandPlotMetadata metadata, SlimeEatWater eat, FeedingSource[] sources)
	{
		foreach (FeedingSource feedingSource in sources)
		{
			foreach (Identifiable.Id id in feedingSource.ids)
			{
				if (eat.WillNowEat(id) && feedingSource.Selected(id))
				{
					PRODUCED = eat.GetProducedIds(id, PRODUCED);
					COLLECTED.Clear();
					eat.EatImmediate(feedingSource.GetTarget(id), id, PRODUCED, COLLECTED, skipDelays: true);
					return true;
				}
			}
		}
		return false;
	}

	private static bool FeedSlime(DroneNetwork.LandPlotMetadata metadata, SlimeEatAsh eat, FeedingSource[] sources)
	{
		return false;
	}

	private void FastForwardGardens(double startTime, double endTime)
	{
		foreach (DroneNetwork.LandPlotMetadata plot in network.Plots)
		{
			if (plot.plot.typeId == LandPlot.Id.GARDEN)
			{
				SpawnResource componentInChildren = plot.plot.GetComponentInChildren<SpawnResource>();
				if (componentInChildren != null)
				{
					componentInChildren.FastForward(startTime, endTime);
				}
			}
		}
	}

	private void FastForwardDrones(double startTime, double endTime)
	{
		foreach (Drone drone in network.Drones)
		{
			DroneFastForwarder.FastForward(drone, startTime, endTime);
		}
	}

	private bool AnyDronesActive(double time)
	{
		return network.Drones.Any((Drone d) => d.station.battery.Time > time);
	}

	private void OnFastForwardChanged(bool isFastForwarding)
	{
		if (isFastForwarding)
		{
			if (!region.Hibernated && AnyDronesActive(timeDirector.WorldTime()))
			{
				DroneFastForwarder.FastForward_Pre(this);
				model.sleepingTime = timeDirector.WorldTime();
			}
		}
		else
		{
			SRSingleton<SceneContext>.Instance.StartCoroutine(OnFastForwardChangedCoroutine());
		}
	}

	private IEnumerator OnFastForwardChangedCoroutine()
	{
		yield return new WaitForEndOfFrame();
		DroneFastForwarder.FastForward_Post(this);
	}
}
