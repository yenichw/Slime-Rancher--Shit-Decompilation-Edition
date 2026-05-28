using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public abstract class DroneProgramSourceDynamic : DroneProgramSource<Identifiable>
{
	private float? pickupDelay;

	private int? maxPickup;

	private const int PICKUP_MASK = 34816;

	protected const float PICKUP_DURATION = 0.3f;

	public override void Selected()
	{
		base.Selected();
		pickupDelay = null;
		maxPickup = null;
		if (source != null)
		{
			source.gameObject.GetComponent<RegionMember>().regionsChanged += OnRegionsChanged;
		}
	}

	public override void Deselected()
	{
		base.Deselected();
		if (source != null)
		{
			source.gameObject.GetComponent<RegionMember>().regionsChanged -= OnRegionsChanged;
		}
	}

	protected override bool CanCancel()
	{
		if (!(source == null) && source.gameObject.activeInHierarchy)
		{
			if (maxPickup.HasValue)
			{
				return maxPickup <= 0;
			}
			return false;
		}
		return true;
	}

	protected override IEnumerable<Orientation> GetTargetOrientations(Identifiable source)
	{
		return GetTargetOrientations_Gather(source.gameObject);
	}

	protected override Vector3 GetTargetPosition(Identifiable source)
	{
		return source.transform.position;
	}

	protected override GameObject GetTargetGameObject(Identifiable source)
	{
		return source.gameObject;
	}

	protected override void OnReachedDestination()
	{
		base.OnReachedDestination();
		maxPickup = GetMaxPickup(source.id);
	}

	protected override bool OnAction()
	{
		if (!pickupDelay.HasValue)
		{
			pickupDelay = Time.fixedTime + (SphereCastPickup(source, maxPickup.Value, GetPickupRadius(), SourcePredicate) ? 0.3f : 0f);
		}
		return pickupDelay.Value <= Time.fixedTime;
	}

	protected override IEnumerable<Identifiable> GetSources(Predicate<Identifiable.Id> predicate)
	{
		return from e in drone.network.gameObject.GetComponent<CellDirector>().identifiableIndex.GetAllRegistered()
			select e.GameObject.GetComponent<Identifiable>() into s
			where predicate(s.id) && SourcePredicate(s)
			orderby (s.transform.position - drone.transform.position).sqrMagnitude
			select s;
	}

	public override IEnumerable<DroneFastForwarder.GatherGroup> GetFastForwardGroups(double endTime)
	{
		return (from source in GetSources(predicate)
			group source by source.id into @group
			select new DroneFastForwarder.GatherGroup.Dynamic(@group) into @group
			where @group.Any()
			select @group).Cast<DroneFastForwarder.GatherGroup>();
	}

	protected bool SourcePredicate(Identifiable source)
	{
		return SourcePredicate(drone.network.GetContaining(source), source);
	}

	protected virtual bool SourcePredicate(DroneNetwork.LandPlotMetadata metadata, Identifiable source)
	{
		if (source != null && source.transform.parent == null && source.gameObject.activeInHierarchy)
		{
			return DroneNetwork.IsResourceReady(source.gameObject);
		}
		return false;
	}

	protected virtual float GetPickupRadius()
	{
		return 1f;
	}

	protected virtual int GetMaxPickup(Identifiable.Id id)
	{
		int slotMaxCount = drone.ammo.GetSlotMaxCount();
		int availableDestinationSpace = GetAvailableDestinationSpace(id);
		return Mathf.Min(slotMaxCount, availableDestinationSpace) - drone.ammo.GetSlotCount();
	}

	private void OnRegionsChanged(List<Region> left, List<Region> joined)
	{
		if (source != null)
		{
			source.gameObject.GetComponent<RegionMember>().regionsChanged -= OnRegionsChanged;
			source = null;
		}
	}

	protected bool SphereCastPickup(Identifiable source, int maxPickup, float radius, Predicate<Identifiable> predicate)
	{
		if (maxPickup >= 1 && predicate(source) && drone.ammo.CouldAddToSlot(source.id))
		{
			SphereCastPickupTween(source);
			int num = maxPickup - 1;
			if (num <= 0)
			{
				return true;
			}
			HashSet<GameObject> hashSet = new HashSet<GameObject> { source.gameObject };
			Vector3 direction = source.transform.position - drone.transform.position;
			RaycastHit[] array = Physics.SphereCastAll(drone.transform.position, radius, direction, direction.magnitude, 34816, QueryTriggerInteraction.Ignore);
			foreach (RaycastHit raycastHit in array)
			{
				Identifiable component = raycastHit.collider.gameObject.GetComponent<Identifiable>();
				if (component != null && component.id == source.id && hashSet.Add(component.gameObject) && predicate(component))
				{
					SphereCastPickupTween(component);
					num--;
					if (num <= 0)
					{
						break;
					}
				}
			}
			return true;
		}
		return false;
	}

	protected void SphereCastPickupTween(Identifiable source)
	{
		List<Collider> list = new List<Collider>();
		Collider[] components = source.gameObject.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (collider.enabled)
			{
				list.Add(collider);
				collider.enabled = false;
			}
		}
		float endValue = source.transform.localScale.x * 0.2f;
		float x = source.transform.localScale.x;
		DOTween.Sequence().Append(source.transform.DOScale(endValue, 0.3f).SetEase(Ease.Linear)).Append(source.transform.DOScale(x, 0.3f).SetEase(Ease.Linear));
		source.transform.DOMove(drone.transform.position, 0.375f).SetEase(Ease.Linear);
		source.transform.DORotate(Quaternion.LookRotation(drone.transform.position - source.transform.position).eulerAngles, 0.45000002f).SetEase(Ease.Linear);
		SRSingleton<SceneContext>.Instance.StartCoroutine(SphereCastPickupCoroutine(source, list));
	}

	protected IEnumerator SphereCastPickupCoroutine(Identifiable identifiable, List<Collider> disabledColliders)
	{
		yield return new WaitForSeconds(0.3f);
		if (!(identifiable != null))
		{
			yield break;
		}
		if (drone.ammo.MaybeAddToSlot(identifiable.id))
		{
			Destroyer.DestroyActor(identifiable.gameObject, "DroneSubbehaviour.SphereCastPickupCoroutine");
			yield break;
		}
		foreach (Collider disabledCollider in disabledColliders)
		{
			disabledCollider.enabled = true;
		}
	}
}
