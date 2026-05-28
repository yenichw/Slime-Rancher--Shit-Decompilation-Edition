using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DroneFastForwarder
{
	private enum GatherDepositMode
	{
		SINGLE = 0,
		MULTIPLE = 1
	}

	public abstract class GatherGroup : IDisposable
	{
		public class Dynamic : GatherGroup
		{
			private List<Identifiable> sources;

			public override Identifiable.Id id => sources.First().id;

			public override int count => sources.Count;

			public override bool overflow => false;

			public Dynamic(IEnumerable<Identifiable> sources)
			{
				this.sources = sources.Where((Identifiable i) => BLACKLIST.Add(i.gameObject)).ToList();
			}

			public override void Decrement(int decrement)
			{
				for (int i = 0; i < decrement; i++)
				{
					Destroyer.DestroyActor(Randoms.SHARED.Pluck(sources, null).gameObject, "GatherGroup.Dynamic.Decrement");
				}
			}

			public override void Dispose()
			{
				BLACKLIST.ExceptWith(sources.Select((Identifiable i) => i.gameObject));
			}
		}

		public class Storage : GatherGroup
		{
			private DroneNetwork.StorageMetadata storage;

			public override Identifiable.Id id => storage.id;

			public override int count => storage.count;

			public override bool overflow => false;

			public Storage(DroneNetwork.StorageMetadata storage)
			{
				this.storage = storage;
			}

			public override void Decrement(int decrement)
			{
				storage.Decrement(decrement);
			}

			public override void Dispose()
			{
			}
		}

		public class Ammo : GatherGroup
		{
			private DroneAmmo ammo;

			public override Identifiable.Id id => ammo.GetSlotName();

			public override int count => ammo.GetSlotCount();

			public override bool overflow => true;

			public Ammo(DroneAmmo ammo)
			{
				this.ammo = ammo;
			}

			public override void Decrement(int decrement)
			{
				ammo.Decrement(0, decrement);
			}

			public override void Dispose()
			{
			}
		}

		public static HashSet<GameObject> BLACKLIST = new HashSet<GameObject>();

		public abstract Identifiable.Id id { get; }

		public abstract int count { get; }

		public abstract bool overflow { get; }

		public abstract void Decrement(int decrement);

		public abstract void Dispose();

		public bool Any()
		{
			return count > 0;
		}

		public bool None()
		{
			return count <= 0;
		}
	}

	private const double GATHER_TIME = 3600.0;

	private const double DEPOSIT_TIME = 3600.0;

	private const double CYCLE_TIME = 7200.0;

	private static List<GatherGroup> GATHER_GROUPS = new List<GatherGroup>();

	private static HashSet<GameObject> ACTIVE = new HashSet<GameObject>();

	private static int coinsPopup;

	public static void FastForward_Pre(RanchCellFastForwarder source)
	{
		if (ACTIVE.Add(source.gameObject))
		{
			SRSingleton<PopupElementsUI>.Instance.RegisterBlocker(source.gameObject);
			if (ACTIVE.Count == 1)
			{
				GatherGroup.BLACKLIST.Clear();
				PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
				playerState.SetCurrencyDisplay(playerState.GetCurrency());
				coinsPopup = 0;
			}
		}
	}

	public static void FastForward_Post(RanchCellFastForwarder source)
	{
		if (ACTIVE.Remove(source.gameObject))
		{
			SRSingleton<PopupElementsUI>.Instance.DeregisterBlocker(source.gameObject);
			if (ACTIVE.Count == 0)
			{
				GatherGroup.BLACKLIST.Clear();
				SRSingleton<PopupElementsUI>.Instance.CreateCoinsPopup(coinsPopup, PlayerState.CoinsType.DRONE);
				SRSingleton<SceneContext>.Instance.PlayerState.SetCurrencyDisplay(null);
			}
		}
	}

	public static void FastForward(Drone drone, double startTime, double endTime)
	{
		if (drone.station.battery.Time <= startTime)
		{
			return;
		}
		if (drone.plexer.Programs.Count == 0)
		{
			drone.ForceResting(includeFX: false);
			return;
		}
		double remainingTime = Math.Min(drone.station.battery.Time, endTime) - startTime;
		if (drone.ammo.Any())
		{
			GatherGroup group = new GatherGroup.Ammo(drone.ammo);
			DroneSubbehaviourPlexer.Program program = drone.plexer.Programs[0];
			while (FastForward_Deposit(drone, program, group, endTime, ref remainingTime))
			{
			}
		}
		if (drone.plexer.Programs.Count > 1)
		{
			Queue<int> queue = new Queue<int>(Enumerable.Range(0, drone.plexer.Programs.Count));
			while (queue.Any())
			{
				int num = queue.Dequeue();
				DroneSubbehaviourPlexer.Program program2 = drone.plexer.Programs[num];
				if (FastForward_GatherDeposit_Advanced(drone, program2, endTime, ref remainingTime))
				{
					queue.Enqueue(num);
				}
			}
		}
		else
		{
			FastForward_GatherDeposit_Basic(drone, endTime, ref remainingTime);
		}
		if (drone.station.battery.Time <= endTime)
		{
			if (drone.ammo.Any())
			{
				remainingTime = double.MaxValue;
				GatherGroup group2 = new GatherGroup.Ammo(drone.ammo);
				foreach (DroneSubbehaviourPlexer.Program program3 in drone.plexer.Programs)
				{
					while (FastForward_Deposit(drone, program3, group2, endTime, ref remainingTime))
					{
					}
				}
			}
			drone.ForceResting(includeFX: false);
		}
		drone.plexer.OnFastForward();
	}

	private static void FastForward_GatherDeposit_Basic(Drone drone, double endTime, ref double remainingTime)
	{
		DroneSubbehaviourPlexer.Program program = drone.plexer.Programs[0];
		FastForward_GatherDeposit(GatherDepositMode.MULTIPLE, drone, program, endTime, ref remainingTime);
	}

	private static bool FastForward_GatherDeposit_Advanced(Drone drone, DroneSubbehaviourPlexer.Program program, double endTime, ref double remainingTime)
	{
		return FastForward_GatherDeposit(GatherDepositMode.SINGLE, drone, program, endTime, ref remainingTime) >= 1;
	}

	private static int FastForward_GatherDeposit(GatherDepositMode mode, Drone drone, DroneSubbehaviourPlexer.Program program, double endTime, ref double remainingTime)
	{
		int num = 0;
		if (remainingTime >= 7200.0)
		{
			GATHER_GROUPS.AddRange(program.Sources.SelectMany((DroneProgramSource s) => s.GetFastForwardGroups(endTime)));
			while (GATHER_GROUPS.Any() && remainingTime >= 7200.0)
			{
				int @int = Randoms.SHARED.GetInt(GATHER_GROUPS.Count);
				GatherGroup gatherGroup = GATHER_GROUPS[@int];
				bool flag = FastForward_Deposit(drone, program, gatherGroup, endTime, ref remainingTime);
				if (flag)
				{
					remainingTime -= 3600.0;
					num++;
					if (mode == GatherDepositMode.SINGLE)
					{
						break;
					}
				}
				if (!flag || gatherGroup.None())
				{
					GATHER_GROUPS.RemoveAt(@int);
					gatherGroup.Dispose();
				}
			}
			GATHER_GROUPS.ForEach(delegate(GatherGroup g)
			{
				g.Dispose();
			});
			GATHER_GROUPS.Clear();
		}
		return num;
	}

	private static bool FastForward_Deposit(Drone drone, DroneSubbehaviourPlexer.Program program, GatherGroup group, double endTime, ref double remainingTime)
	{
		if (group.None() || remainingTime < 3600.0)
		{
			return false;
		}
		DroneProgramDestination droneProgramDestination = Randoms.SHARED.Pick(program.Destinations.Where((DroneProgramDestination d) => d.HasAvailableSpace(group.id)), null);
		if (droneProgramDestination == null)
		{
			return false;
		}
		DroneProgramDestination.FastForward_Response fastForward_Response = droneProgramDestination.FastForward(group.id, group.overflow, endTime, Mathf.Min(50, group.count));
		coinsPopup += fastForward_Response.currency;
		if (fastForward_Response.deposits <= 0)
		{
			return false;
		}
		group.Decrement(fastForward_Response.deposits);
		remainingTime -= 3600.0;
		return true;
	}
}
