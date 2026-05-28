using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DroneProgramSourceSiloStorage : DroneProgramSource<DroneNetwork.StorageMetadata>
{
	private int remaining;

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

	public override IEnumerable<DroneFastForwarder.GatherGroup> GetFastForwardGroups(double endTime)
	{
		return (from s in GetSources(predicate)
			select new DroneFastForwarder.GatherGroup.Storage(s)).Cast<DroneFastForwarder.GatherGroup>();
	}

	protected override bool CanCancel()
	{
		return source.CanCancel();
	}

	protected override IEnumerable<Orientation> GetTargetOrientations(DroneNetwork.StorageMetadata source)
	{
		orientation = GetTargetOrientation_Catcher(source.catcher.gameObject);
		yield return orientation.orientation;
	}

	protected override Vector3 GetTargetPosition(DroneNetwork.StorageMetadata source)
	{
		return source.catcher.transform.position;
	}

	protected override GameObject GetTargetGameObject(DroneNetwork.StorageMetadata source)
	{
		return source.catcher.gameObject;
	}

	protected override void OnFirstAction()
	{
		base.OnFirstAction();
		int num = drone.ammo.GetSlotMaxCount() - drone.ammo.GetSlotCount();
		int count = source.count;
		int availableDestinationSpace = GetAvailableDestinationSpace(source.id);
		remaining = Mathf.Min(num, count, availableDestinationSpace);
	}

	protected override bool OnAction()
	{
		if (timeDirector.HasReached(time))
		{
			if (remaining > 0 && drone.ammo.MaybeAddToSlot(source.id))
			{
				time = timeDirector.HoursFromNow(0.0016666668f);
				source.ammo.Decrement(source.id);
				return --remaining <= 0;
			}
			return true;
		}
		return false;
	}

	protected override void OnPathGenerationFailed()
	{
		base.OnPathGenerationFailed();
		if (orientation != null)
		{
			orientation.Dispose();
			orientation = null;
		}
	}
}
