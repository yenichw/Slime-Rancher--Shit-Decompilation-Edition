using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneProgramDestinationCorral : DroneProgramDestination<DroneProgramDestinationCorral.Destination>
{
	public class Destination
	{
		public class Comparer : SRComparer<Destination>
		{
			public new static Comparer<Destination> Default = from m in new Comparer()
				orderby m.anyFavorite descending
				orderby m.available
				select m;
		}

		public readonly DroneNetwork.LandPlotMetadata metadata;

		public readonly int available;

		public readonly bool anyEat;

		public readonly bool anyFavorite;

		public Destination(DroneNetwork.LandPlotMetadata metadata, Identifiable.Id id)
		{
			this.metadata = metadata;
			int num = 0;
			int num2 = 0;
			TrackContainedIdentifiables[] trackers = metadata.trackers;
			for (int i = 0; i < trackers.Length; i++)
			{
				foreach (KeyValuePair<Identifiable.Id, HashSet<Identifiable>> item in trackers[i].GetAllTracked())
				{
					if (!item.Value.Any())
					{
						continue;
					}
					if (Identifiable.IsSlime(item.Key))
					{
						if (!anyEat || !anyFavorite)
						{
							SlimeEat component = item.Value.First().GetComponent<SlimeEat>();
							anyEat |= component.DoesEat(id);
							anyFavorite |= component.GetEatMapById(id).Any((SlimeDiet.EatMapEntry e) => e.isFavorite);
						}
						num += item.Value.Count;
					}
					else if (Identifiable.IsFood(item.Key))
					{
						num2 += item.Value.Count;
					}
				}
			}
			available = Mathf.Max(0, Mathf.Max(5, Mathf.CeilToInt((float)num * 1.2f)) - num2);
		}

		public bool CanCancel()
		{
			return metadata.plot == null;
		}
	}

	private double time;

	private int dropCount;

	private const float FOODS_PER_SLIME = 1.2f;

	private const int MINIMUM_DELIVERY = 5;

	protected override DroneAnimator.Id animation => DroneAnimator.Id.IDLE;

	protected override DroneAnimatorState.Id animationStateBegin => DroneAnimatorState.Id.NONE;

	protected override DroneAnimatorState.Id animationStateEnd => DroneAnimatorState.Id.NONE;

	public override int GetAvailableSpace(Identifiable.Id id)
	{
		return GetDestinations(id, overflow: false).Aggregate(0, (int cd, Destination m) => cd + m.available);
	}

	public override bool HasAvailableSpace(Identifiable.Id id)
	{
		return GetDestinations(id, overflow: false).Any((Destination m) => m.available > 0);
	}

	protected override IEnumerable<Destination> GetDestinations(Identifiable.Id id, bool overflow)
	{
		return from m in drone.network.Plots
			where m.plot.typeId == LandPlot.Id.CORRAL
			select new Destination(m, id) into m
			where m.anyEat && m.available >= 5
			select m;
	}

	protected override IEnumerable<Destination> Prioritize(IEnumerable<Destination> destinations)
	{
		return destinations.OrderBy((Destination d) => d, Destination.Comparer.Default);
	}

	protected override bool CanCancel()
	{
		if (!base.CanCancel())
		{
			return destination.CanCancel();
		}
		return true;
	}

	protected override IEnumerable<Orientation> GetTargetOrientations()
	{
		return GetTargetOrientations(destination);
	}

	protected static IEnumerable<Orientation> GetTargetOrientations(Destination destination)
	{
		yield return new Orientation(destination.metadata.plot.transform.position + Vector3.up * (destination.metadata.plot.HasUpgrade(LandPlot.Upgrade.WALLS) ? 6 : 3), Quaternion.Euler(0f, Randoms.SHARED.GetInRange(0, 360), 0f));
	}

	protected override Vector3 GetTargetPosition()
	{
		return destination.metadata.plot.transform.position;
	}

	protected override void OnFirstAction()
	{
		base.OnFirstAction();
		time = timeDirector.HoursFromNow(0.013333335f);
		dropCount = new Destination(destination.metadata, drone.ammo.GetSlotName()).available;
	}

	protected override bool OnAction_Deposit(bool overflow)
	{
		if (dropCount > 0 || overflow)
		{
			dropCount -= (OnAction_DumpAmmo(ref time) ? 1 : 0);
		}
		if (!overflow)
		{
			return dropCount <= 0;
		}
		return false;
	}

	public override FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward)
	{
		Destination destination = Prioritize(GetDestinations(id, overflow)).First();
		maxFastForward = (overflow ? maxFastForward : Mathf.Min(maxFastForward, destination.available));
		maxFastForward = RanchCellFastForwarder.FeedSlimes(destination.metadata, endTime, new RanchCellFastForwarder.FeedingSource.Basic(id, maxFastForward));
		return new FastForward_Response
		{
			deposits = maxFastForward
		};
	}
}
