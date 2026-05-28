using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DroneProgramDestinationSiloStorage<T> : DroneProgramDestination<T> where T : DroneNetwork.StorageMetadata
{
	private double time;

	private CatcherOrientation orientation;

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
		return GetDestinations(id, overflow: false).Cast<DroneNetwork.StorageMetadata>().Aggregate(0, (int accum, DroneNetwork.StorageMetadata d) => accum + d.GetAvailableSpace(id));
	}

	public override bool HasAvailableSpace(Identifiable.Id id)
	{
		return GetDestinations(id, overflow: false).Any();
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
		orientation = GetTargetOrientation_Catcher(destination.catcher.gameObject);
		yield return orientation.orientation;
	}

	protected override Vector3 GetTargetPosition()
	{
		return destination.catcher.transform.position;
	}

	protected override bool OnAction_Deposit(bool overflow)
	{
		if (timeDirector.HasReached(time))
		{
			Identifiable.Id slotName = drone.ammo.GetSlotName();
			if (destination.Increment(slotName, overflow))
			{
				drone.ammo.Decrement(slotName);
				time = timeDirector.HoursFromNow(0.0016666668f);
				if (!overflow)
				{
					return destination.IsFull();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override FastForward_Response FastForward(Identifiable.Id id, bool overflow, double endTime, int maxFastForward)
	{
		DroneNetwork.StorageMetadata storageMetadata = Prioritize(GetDestinations(id, overflow)).First();
		maxFastForward = (overflow ? maxFastForward : Mathf.Min(maxFastForward, storageMetadata.GetAvailableSpace(id)));
		storageMetadata.Increment(id, overflow, maxFastForward);
		return new FastForward_Response
		{
			deposits = maxFastForward
		};
	}
}
