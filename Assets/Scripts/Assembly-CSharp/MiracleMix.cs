using System.Collections.Generic;
using UnityEngine;

public class MiracleMix : MonoBehaviour
{
	public float ripenessModifier = -0.5f;

	private HashSet<ResourceCycle> preservedResources = new HashSet<ResourceCycle>();

	public void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			Identifiable component = other.GetComponent<Identifiable>();
			if (component != null && IsPreservable(component))
			{
				ResourceCycle component2 = other.GetComponent<ResourceCycle>();
				preservedResources.Add(component2);
				component2.AttachPreservative(this);
			}
		}
	}

	public bool IsPreservable(Identifiable ident)
	{
		if (!Identifiable.VEGGIE_CLASS.Contains(ident.id))
		{
			return Identifiable.FRUIT_CLASS.Contains(ident.id);
		}
		return true;
	}

	public void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			Identifiable component = other.GetComponent<Identifiable>();
			if (component != null && IsPreservable(component))
			{
				ResourceCycle component2 = other.GetComponent<ResourceCycle>();
				RemoveResourceCycle(component2);
			}
		}
	}

	public void RemoveResourceCycle(ResourceCycle cycle)
	{
		preservedResources.Remove(cycle);
		cycle.DetachPreservative(this);
	}

	public void OnDestroy()
	{
		foreach (ResourceCycle preservedResource in preservedResources)
		{
			if (preservedResource != null)
			{
				preservedResource.DetachPreservative(this);
			}
		}
		preservedResources.Clear();
	}

	public float PreservativeRipenessModifier()
	{
		return ripenessModifier;
	}
}
