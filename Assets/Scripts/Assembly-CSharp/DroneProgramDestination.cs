using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DroneProgramDestination : DroneProgram
{
	public class FastForward_Response
	{
		public int deposits;

		public int currency;
	}

	public Predicate<Identifiable.Id> predicate = (Identifiable.Id id) => false;

	protected override DroneAnimator.Id animation => DroneAnimator.Id.DEPOSIT;

	protected override DroneAnimatorState.Id animationStateBegin => DroneAnimatorState.Id.DEPOSIT_BEGIN;

	protected override DroneAnimatorState.Id animationStateEnd => DroneAnimatorState.Id.DEPOSIT_END;

	public abstract int GetAvailableSpace(Identifiable.Id id);

	public abstract FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward);

	public virtual bool HasAvailableSpace(Identifiable.Id id)
	{
		return GetAvailableSpace(id) > 0;
	}

	public sealed override bool Relevancy()
	{
		throw new InvalidOperationException();
	}

	public abstract bool Relevancy(bool overflow);
}
public abstract class DroneProgramDestination<T> : DroneProgramDestination where T : class
{
	protected int MAX_AVAIL_TO_REPORT = 1000000;

	protected T destination;

	private bool overflow;

	public sealed override bool Relevancy(bool overflow)
	{
		if (drone.ammo.IsEmpty())
		{
			return false;
		}
		Identifiable.Id slotName = drone.ammo.GetSlotName();
		if (!predicate(slotName))
		{
			return false;
		}
		destination = Prioritize(GetDestinations(slotName, overflow)).FirstOrDefault();
		return !IsNull(destination);
	}

	public override void Selected()
	{
		base.Selected();
		overflow = false;
	}

	protected sealed override bool OnAction()
	{
		if (!OnAction_Deposit(overflow))
		{
			return false;
		}
		if (drone.ammo.IsEmpty())
		{
			return true;
		}
		if (overflow)
		{
			Log.Error("Failed to complete overflow deposit.", "destination", destination);
			return true;
		}
		if (GetDestinations(drone.ammo.GetSlotName(), overflow: false).Any((T d) => d != destination))
		{
			return true;
		}
		overflow = true;
		return false;
	}

	protected override bool CanCancel()
	{
		if (!drone.ammo.IsEmpty())
		{
			return IsNull(destination);
		}
		return true;
	}

	private static bool IsNull(T destination)
	{
		return destination?.Equals(null) ?? true;
	}

	protected abstract bool OnAction_Deposit(bool overflow);

	protected abstract IEnumerable<T> GetDestinations(Identifiable.Id id, bool overflow);

	protected abstract IEnumerable<T> Prioritize(IEnumerable<T> destinations);
}
