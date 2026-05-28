using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneProgramDestinationRefinery : DroneProgramDestination<SiloCatcher>
{
	private class Comparer : SRComparer<SiloCatcher>
	{
	}

	private GadgetDirector gadgetDirector;

	private CatcherOrientation orientation;

	private double time;

	public override void Awake()
	{
		base.Awake();
		gadgetDirector = SRSingleton<SceneContext>.Instance.GadgetDirector;
	}

	public override void Deselected()
	{
		base.Deselected();
		if (orientation != null)
		{
			orientation.Dispose();
			orientation = null;
		}
	}

	public override int GetAvailableSpace(Identifiable.Id id)
	{
		if (!GetDestinations(id, overflow: false).Any())
		{
			return 0;
		}
		return gadgetDirector.GetRefinerySpaceAvailable(id);
	}

	public override bool HasAvailableSpace(Identifiable.Id id)
	{
		if (GetDestinations(id, overflow: false).Any())
		{
			return gadgetDirector.HasRefinerySpaceAvailable(id);
		}
		return false;
	}

	protected override IEnumerable<SiloCatcher> Prioritize(IEnumerable<SiloCatcher> destinations)
	{
		return destinations.OrderBy((SiloCatcher d) => d, new Comparer().OrderBy((SiloCatcher m) => (m.transform.position - drone.transform.position).sqrMagnitude));
	}

	protected override IEnumerable<SiloCatcher> GetDestinations(Identifiable.Id id, bool overflow)
	{
		if (!overflow && !gadgetDirector.HasRefinerySpaceAvailable(id))
		{
			yield break;
		}
		foreach (SiloCatcher refineryCatcher in drone.network.RefineryCatchers)
		{
			yield return refineryCatcher;
		}
	}

	protected override IEnumerable<Orientation> GetTargetOrientations()
	{
		orientation = GetTargetOrientation_Catcher(destination.gameObject);
		yield return orientation.orientation;
	}

	protected override Vector3 GetTargetPosition()
	{
		return destination.transform.position;
	}

	protected override bool OnAction_Deposit(bool overflow)
	{
		if (timeDirector.HasReached(time))
		{
			if (gadgetDirector.AddToRefinery(drone.ammo.GetSlotName(), 1, overflow) > 0)
			{
				Identifiable.Id id = drone.ammo.Pop();
				time = timeDirector.HoursFromNow(0.0016666668f);
				if (!overflow)
				{
					return !gadgetDirector.HasRefinerySpaceAvailable(id);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward)
	{
		maxFastForward = gadgetDirector.AddToRefinery(id, maxFastForward, overflow);
		return new FastForward_Response
		{
			deposits = maxFastForward
		};
	}
}
