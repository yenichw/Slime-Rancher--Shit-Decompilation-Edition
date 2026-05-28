using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackContainedIdentifiables : SRBehaviour
{
	public delegate void IdentifiableEntered(TrackContainedIdentifiables container, Identifiable ident);

	public delegate void NewIdentifiableTypeEntered(TrackContainedIdentifiables container, Identifiable ident);

	[Tooltip("List of AirNet components to be checked during the slime integrity tracker.")]
	public List<AirNet> airNets;

	private Dictionary<Identifiable.Id, HashSet<Identifiable>> trackedObjects = new Dictionary<Identifiable.Id, HashSet<Identifiable>>(Identifiable.idComparer);

	private HashSet<Identifiable.Id> uniqueIdentifiableTypes = new HashSet<Identifiable.Id>(Identifiable.idComparer);

	public IdentifiableEntered OnIdentifiableEntered;

	public NewIdentifiableTypeEntered OnNewIdentifiableTypeEntered;

	private Dictionary<int, Destroyer.Metadata> metadataDict = new Dictionary<int, Destroyer.Metadata>();

	private List<int> wasRemoved = new List<int>();

	private int nextTrackFrameCount;

	private List<KeyValuePair<int, Destroyer.Metadata>> local_ToRemoveFromDict = new List<KeyValuePair<int, Destroyer.Metadata>>(64);

	public void Awake()
	{
	}

	public void GetTrackedItemsOfType(Identifiable.Id identId, List<Identifiable> trackedItems)
	{
		if (trackedObjects.ContainsKey(identId))
		{
			trackedItems.AddRange(trackedObjects[identId]);
		}
	}

	public void GetTrackedItemsOfClass(HashSet<Identifiable.Id> idClass, List<Identifiable> trackedItems)
	{
		foreach (Identifiable.Id item in idClass)
		{
			GetTrackedItemsOfType(item, trackedItems);
		}
	}

	public void GetTrackedItemsOfType<T>(Identifiable.Id identId, List<T> trackedItems)
	{
		if (!trackedObjects.ContainsKey(identId))
		{
			return;
		}
		foreach (Identifiable item in trackedObjects[identId])
		{
			T component = item.GetComponent<T>();
			trackedItems.Add(component);
		}
	}

	public void GetTrackedItemsOfClass<T>(HashSet<Identifiable.Id> idClass, List<T> trackedItems)
	{
		foreach (Identifiable.Id item in idClass)
		{
			GetTrackedItemsOfType(item, trackedItems);
		}
	}

	public void GetTrackedItemsOfClass(HashSet<Identifiable.Id> idClass, List<GameObject> trackedItems)
	{
		foreach (Identifiable.Id item in idClass)
		{
			if (!trackedObjects.ContainsKey(item))
			{
				continue;
			}
			foreach (Identifiable item2 in trackedObjects[item])
			{
				trackedItems.Add(item2.gameObject);
			}
		}
	}

	public bool HasIdentifiableType(Identifiable.Id identId)
	{
		if (trackedObjects.ContainsKey(identId))
		{
			return trackedObjects[identId].Count > 0;
		}
		return false;
	}

	public void GetTrackedIdentifiableTypes(List<Identifiable.Id> identIds)
	{
		identIds.AddRange(uniqueIdentifiableTypes);
	}

	public void GetTrackedIdentifiableTypes(HashSet<Identifiable.Id> typesToFind)
	{
		typesToFind.IntersectWith(uniqueIdentifiableTypes);
	}

	public IEnumerable<KeyValuePair<Identifiable.Id, HashSet<Identifiable>>> GetAllTracked()
	{
		return trackedObjects;
	}

	public IEnumerable<Identifiable.Id> GetTrackedIdentifiableTypes()
	{
		return uniqueIdentifiableTypes;
	}

	public int Count(Identifiable.Id id)
	{
		if (trackedObjects.ContainsKey(id))
		{
			return trackedObjects[id].Count;
		}
		return 0;
	}

	public bool Contains(Identifiable ident)
	{
		if (trackedObjects.ContainsKey(ident.id))
		{
			return trackedObjects[ident.id].Contains(ident);
		}
		return false;
	}

	public Identifiable RemoveTrackedObject(Identifiable.Id id)
	{
		Identifiable identifiable = trackedObjects[id].First();
		RemoveTrackedObject(identifiable);
		return identifiable;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			Identifiable identifiable = GetIdentifiable(other);
			if (identifiable != null)
			{
				AddTrackedObject(identifiable);
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			Identifiable identifiable = GetIdentifiable(other);
			if (identifiable != null)
			{
				RemoveTrackedObject(identifiable);
			}
		}
	}

	private Identifiable GetIdentifiable(Collider col)
	{
		Identifiable result = null;
		if (col.gameObject != null)
		{
			result = col.gameObject.GetComponent<Identifiable>();
		}
		return result;
	}

	public void OnTrackedDestroyed(Identifiable trackedObject)
	{
		RemoveTrackedObject(trackedObject);
	}

	private void AddTrackedObject(Identifiable ident)
	{
		if (!trackedObjects.ContainsKey(ident.id))
		{
			trackedObjects.Add(ident.id, new HashSet<Identifiable>());
		}
		if (!trackedObjects[ident.id].Add(ident))
		{
			return;
		}
		if (OnIdentifiableEntered != null)
		{
			OnIdentifiableEntered(this, ident);
		}
		if (!uniqueIdentifiableTypes.Contains(ident.id))
		{
			uniqueIdentifiableTypes.Add(ident.id);
			if (OnNewIdentifiableTypeEntered != null)
			{
				OnNewIdentifiableTypeEntered(this, ident);
			}
		}
		ident.NotifyOnDestroy = (Identifiable.OnDestroyListener)Delegate.Combine(ident.NotifyOnDestroy, new Identifiable.OnDestroyListener(OnTrackedDestroyed));
		if (IsTrackingIntegrity(ident))
		{
			int instanceID = ident.gameObject.GetInstanceID();
			Destroyer.Monitor(ident.gameObject, delegate(Destroyer.Metadata metadata)
			{
				metadataDict[instanceID] = metadata;
			});
		}
	}

	private void RemoveTrackedObject(Identifiable ident)
	{
		if (!trackedObjects.ContainsKey(ident.id))
		{
			Log.Debug("Request to remove object where the Identifiable.Id is not being tracked.", "Identifiable.Id", ident.id);
		}
		else if (trackedObjects[ident.id].Remove(ident))
		{
			if (trackedObjects[ident.id].Count == 0)
			{
				uniqueIdentifiableTypes.Remove(ident.id);
			}
			ident.NotifyOnDestroy = (Identifiable.OnDestroyListener)Delegate.Remove(ident.NotifyOnDestroy, new Identifiable.OnDestroyListener(OnTrackedDestroyed));
			if (IsTrackingIntegrity(ident))
			{
				wasRemoved.Add(ident.gameObject.GetInstanceID());
			}
		}
	}

	public void OnDestroy()
	{
		airNets.Clear();
	}

	private bool IsTrackingIntegrity(Identifiable ident)
	{
		if (!Identifiable.IsSlime(ident.id) || Identifiable.IsTarr(ident.id))
		{
			return false;
		}
		if (ident.gameObject.GetComponent<QuantumSlimeSuperposition>() != null)
		{
			return false;
		}
		if (!airNets.Any((AirNet net) => net.IsNetActive()))
		{
			return false;
		}
		int instanceID = ident.gameObject.GetInstanceID();
		metadataDict.TryGetValue(instanceID, out var value);
		if (value != null && (value.source.Contains("DestroyOnTouching.DestroyAndWater") || value.source.Contains("DestroyOutsideHoursOfDay") || value.source.Contains("Vacuumable")))
		{
			return false;
		}
		return true;
	}

	public void LateUpdate()
	{
		if (Time.frameCount < nextTrackFrameCount)
		{
			return;
		}
		if (wasRemoved.Count >= 5)
		{
			Log.Error("Found potential missing slime issue... " + new
			{
				corralID = base.gameObject.GetComponentInParent<IdHandler>().id,
				currentFrame = Time.frameCount,
				missingSlimes = string.Join(", ", wasRemoved.Select((int id) => new
				{
					instanceID = id,
					metadata = ((Func<object>)(() => metadataDict.TryGetValue(id, out var value) ? value.ToString() : "null"))(),
					gameObject = ((Func<object>)delegate
					{
						foreach (Transform item in SRSingleton<DynamicObjectContainer>.Instance.transform)
						{
							if (item.gameObject.GetInstanceID() == id)
							{
								return new
								{
									item.gameObject.name,
									item.gameObject.transform.position,
									item.gameObject.GetComponent<Rigidbody>().velocity,
									item.gameObject.GetComponent<Rigidbody>().angularVelocity
								}.ToString();
							}
						}
						return "null";
					})()
				}.ToString()).ToArray())
			}.ToString());
		}
		nextTrackFrameCount += 3;
		foreach (KeyValuePair<int, Destroyer.Metadata> item2 in metadataDict)
		{
			if (Time.frameCount - item2.Value.frame > 3)
			{
				local_ToRemoveFromDict.Add(item2);
			}
		}
		foreach (KeyValuePair<int, Destroyer.Metadata> item3 in local_ToRemoveFromDict)
		{
			metadataDict.Remove(item3.Key);
		}
		local_ToRemoveFromDict.Clear();
		wasRemoved.Clear();
	}
}
