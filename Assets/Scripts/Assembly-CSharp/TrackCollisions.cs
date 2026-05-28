using System.Collections.Generic;
using UnityEngine;

public class TrackCollisions : SRBehaviour
{
	private Dictionary<GameObject, List<Collider>> currColliders = new Dictionary<GameObject, List<Collider>>();

	private HashSet<GameObject> gameObjSet = new HashSet<GameObject>();

	private List<GameObject> local_gameObjsToRemove = new List<GameObject>(50);

	private List<Collider> local_collidersToKeep = new List<Collider>(50);

	private HashSet<GameObject> emptySet = new HashSet<GameObject>();

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (!currColliders.ContainsKey(other.gameObject))
		{
			currColliders[other.gameObject] = new List<Collider>();
		}
		currColliders[other.gameObject].Add(other);
		gameObjSet.Add(other.gameObject);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (currColliders.ContainsKey(other.gameObject))
		{
			currColliders[other.gameObject].Remove(other);
			if (currColliders[other.gameObject].Count == 0)
			{
				currColliders.Remove(other.gameObject);
				gameObjSet.Remove(other.gameObject);
			}
		}
	}

	public HashSet<GameObject> CurrColliders()
	{
		foreach (KeyValuePair<GameObject, List<Collider>> currCollider in currColliders)
		{
			foreach (Collider item in currCollider.Value)
			{
				if (!RemovePredicate(item))
				{
					local_collidersToKeep.Add(item);
				}
			}
			if (local_collidersToKeep.Count == 0)
			{
				local_gameObjsToRemove.Add(currCollider.Key);
			}
			else
			{
				currCollider.Value.Clear();
				foreach (Collider item2 in local_collidersToKeep)
				{
					currCollider.Value.Add(item2);
				}
			}
			local_collidersToKeep.Clear();
		}
		foreach (GameObject item3 in local_gameObjsToRemove)
		{
			gameObjSet.Remove(item3);
			currColliders.Remove(item3);
		}
		local_gameObjsToRemove.Clear();
		foreach (GameObject item4 in gameObjSet)
		{
			if (RemovePredicate(item4))
			{
				local_gameObjsToRemove.Add(item4);
			}
		}
		foreach (GameObject item5 in local_gameObjsToRemove)
		{
			gameObjSet.Remove(item5);
		}
		local_gameObjsToRemove.Clear();
		return gameObjSet;
	}

	private static bool RemovePredicate(Collider collider)
	{
		if (!(collider == null) && collider.enabled)
		{
			return RemovePredicate(collider.gameObject);
		}
		return true;
	}

	private static bool RemovePredicate(GameObject go)
	{
		if (!(go == null))
		{
			return !go.activeInHierarchy;
		}
		return true;
	}
}
