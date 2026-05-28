using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DroneProgramSource : DroneProgram
{
	public static HashSet<GameObject> BLACKLIST = new HashSet<GameObject>();

	public Predicate<Identifiable.Id> predicate = (Identifiable.Id id) => false;

	public IEnumerable<DroneProgramDestination> destinations = Enumerable.Empty<DroneProgramDestination>();

	protected override DroneAnimator.Id animation => DroneAnimator.Id.GATHER;

	protected override DroneAnimatorState.Id animationStateBegin => DroneAnimatorState.Id.GATHER_BEGIN;

	protected override DroneAnimatorState.Id animationStateEnd => DroneAnimatorState.Id.GATHER_END;

	public abstract IEnumerable<DroneFastForwarder.GatherGroup> GetFastForwardGroups(double endTime);

	protected int GetAvailableDestinationSpace(Identifiable.Id id)
	{
		return destinations.Aggregate(0, (int c, DroneProgramDestination d) => c + d.GetAvailableSpace(id));
	}

	protected bool HasAvailableDestinationSpace(Identifiable.Id id)
	{
		return destinations.Any((DroneProgramDestination d) => d.HasAvailableSpace(id));
	}

	protected bool HasAvailableDestinationSpace(Identifiable.Id id, int minimum)
	{
		foreach (DroneProgramDestination destination in destinations)
		{
			int availableSpace = destination.GetAvailableSpace(id);
			if (availableSpace >= minimum)
			{
				return true;
			}
			minimum -= availableSpace;
		}
		return false;
	}
}
public abstract class DroneProgramSource<T> : DroneProgramSource where T : class
{
	protected T source;

	private GameObject sourceGameObject;

	public override bool Relevancy()
	{
		if (drone.ammo.IsFull())
		{
			return false;
		}
		if (!drone.station.battery.HasAny())
		{
			return false;
		}
		if (drone.ammo.Any() && !HasAvailableDestinationSpace(drone.ammo.GetSlotName(), drone.ammo.GetSlotCount() + 1))
		{
			return false;
		}
		foreach (T source in GetSources((Identifiable.Id id) => predicate(id) && drone.ammo.CouldAddToSlot(id) && (drone.ammo.Any() || HasAvailableDestinationSpace(id))))
		{
			sourceGameObject = GetTargetGameObject(source);
			this.source = source;
			if (DroneProgramSource.BLACKLIST.Add(sourceGameObject) && GeneratePath(GetSubnetwork(), GetTargetOrientations(), GetTargetPosition()))
			{
				return true;
			}
		}
		sourceGameObject = null;
		this.source = null;
		return false;
	}

	public override void Selected()
	{
		base.Selected();
	}

	public override void Deselected()
	{
		base.Deselected();
		DroneProgramSource.BLACKLIST.Remove(sourceGameObject);
		sourceGameObject = null;
	}

	protected override void OnPathGenerationFailed()
	{
		base.OnPathGenerationFailed();
		if (sourceGameObject != null)
		{
			sourceGameObject.AddComponent<DroneProgramSource_PathGenerationFailure>();
			sourceGameObject = null;
		}
	}

	protected sealed override IEnumerable<Orientation> GetTargetOrientations()
	{
		return GetTargetOrientations(source);
	}

	protected sealed override Vector3 GetTargetPosition()
	{
		return GetTargetPosition(source);
	}

	protected abstract IEnumerable<T> GetSources(Predicate<Identifiable.Id> predicate);

	protected abstract IEnumerable<Orientation> GetTargetOrientations(T source);

	protected abstract Vector3 GetTargetPosition(T source);

	protected abstract GameObject GetTargetGameObject(T source);
}
