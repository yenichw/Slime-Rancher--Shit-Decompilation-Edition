using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneProgramDestinationPlortMarket : DroneProgramDestination<ScorePlort>
{
	private class Comparer : SRComparer<ScorePlort>
	{
	}

	private double time;

	private EconomyDirector economyDirector;

	private CatcherOrientation orientation;

	public override void Awake()
	{
		base.Awake();
		economyDirector = SRSingleton<SceneContext>.Instance.EconomyDirector;
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
		return MAX_AVAIL_TO_REPORT;
	}

	protected override IEnumerable<ScorePlort> GetDestinations(Identifiable.Id id, bool overflow)
	{
		return drone.network.Markets.Where((ScorePlort s) => s.CanDeposit(id, ignoreMarketShutdown: true));
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
		if (timeDirector.HasReached(time) && !economyDirector.IsMarketShutdown())
		{
			if (destination.Deposit(drone.ammo.GetSlotName(), 1, PlayerState.CoinsType.DRONE))
			{
				time = timeDirector.HoursFromNow(0.0016666668f);
				drone.ammo.Pop();
				return false;
			}
			return true;
		}
		return false;
	}

	public override FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward)
	{
		ScorePlort.Deposit_Response deposit_Response = Prioritize(GetDestinations(id, overflow)).First().Deposit(id, maxFastForward, PlayerState.CoinsType.NONE, ignoreMarketShutdown: true);
		return new FastForward_Response
		{
			deposits = deposit_Response.deposits,
			currency = deposit_Response.currency
		};
	}

	protected override IEnumerable<ScorePlort> Prioritize(IEnumerable<ScorePlort> destinations)
	{
		return destinations.OrderBy((ScorePlort d) => d, new Comparer().OrderBy((ScorePlort m) => (m.transform.position - drone.transform.position).sqrMagnitude));
	}
}
