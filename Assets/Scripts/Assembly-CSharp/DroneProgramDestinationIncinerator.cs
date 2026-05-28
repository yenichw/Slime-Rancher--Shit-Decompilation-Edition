using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneProgramDestinationIncinerator : DroneProgramDestination<Incinerate>
{
	private class Comparer : SRComparer<Incinerate>
	{
	}

	private double time;

	private int dropCount;

	public override int GetAvailableSpace(Identifiable.Id id)
	{
		return GetDestinations(id, overflow: false).Aggregate(0, (int agg, Incinerate i) => agg + i.GetAshSpace());
	}

	public override bool HasAvailableSpace(Identifiable.Id id)
	{
		return GetDestinations(id, overflow: false).Any((Incinerate i) => i.GetAshSpace() > 0);
	}

	protected override IEnumerable<Incinerate> GetDestinations(Identifiable.Id id, bool overflow)
	{
		return from i in drone.network.Plots.SelectMany((DroneNetwork.LandPlotMetadata i) => i.incinerators)
			where i.GetAshSpace() > 0
			select i;
	}

	protected override IEnumerable<Incinerate> Prioritize(IEnumerable<Incinerate> destinations)
	{
		return destinations.OrderBy((Incinerate d) => d, from i in new Comparer()
			orderby i.GetAshSpace() descending
			orderby (i.transform.position - drone.transform.position).sqrMagnitude
			select i);
	}

	protected override IEnumerable<Orientation> GetTargetOrientations()
	{
		return GetTargetOrientations_Gather(destination.gameObject, new GatherConfig
		{
			fallbackOffset = Vector3.forward,
			distanceHorizontal = 2.5f
		});
	}

	protected override Vector3 GetTargetPosition()
	{
		return destination.transform.position;
	}

	protected override void OnFirstAction()
	{
		base.OnFirstAction();
		dropCount = destination.GetAshSpace();
	}

	protected override bool OnAction_Deposit(bool overflow)
	{
		if ((dropCount > 0 || overflow) && timeDirector.HasReached(time))
		{
			Identifiable.Id slotName = drone.ammo.GetSlotName();
			time = timeDirector.HoursFromNow(0.0016666668f);
			drone.ammo.Decrement(slotName);
			destination.ProcessIncinerateResults(slotName, 1, destination.transform.position + (drone.transform.position - destination.transform.position).normalized * PhysicsUtil.RadiusOfObject(destination.gameObject) * 0.25f, Quaternion.identity);
			dropCount--;
		}
		if (!overflow)
		{
			return dropCount <= 0;
		}
		return false;
	}

	public override FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward)
	{
		Incinerate incinerate = Prioritize(GetDestinations(id, overflow)).First();
		maxFastForward = (overflow ? maxFastForward : Mathf.Min(maxFastForward, incinerate.GetAshSpace()));
		incinerate.ProcessIncinerateResults(id, maxFastForward);
		return new FastForward_Response
		{
			deposits = maxFastForward
		};
	}
}
