using System.Collections.Generic;
using UnityEngine;

public class SlimeVarietyModules : SRBehaviour
{
	public GameObject baseModule;

	public GameObject[] slimeModules;

	private List<Component> addedComponents = new List<Component>();

	public void Assemble()
	{
		if (addedComponents.Count > 0)
		{
			Log.Error("Why are we assembling an already assembled slime? Skipping: " + base.gameObject.name);
		}
		else
		{
			MergeGeneralComponents();
		}
	}

	private void MergeGeneralComponents()
	{
		GameObject[] array = slimeModules;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject != null))
			{
				continue;
			}
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component is Collider || GetComponent(component.GetType()) == null)
				{
					addedComponents.Add(base.gameObject.AddComponent(component.GetType()).GetCopyOf(component));
				}
			}
			int childCount = gameObject.transform.childCount;
			for (int k = 0; k < childCount; k++)
			{
				GameObject obj = Object.Instantiate(gameObject.transform.GetChild(k).gameObject);
				Vector3 localPosition = obj.transform.localPosition;
				Quaternion localRotation = obj.transform.localRotation;
				obj.transform.parent = base.transform;
				obj.transform.localPosition = localPosition;
				obj.transform.localRotation = localRotation;
			}
		}
		if (baseModule != null)
		{
			bool flag = GetComponent<RejectBaseNontriggerColliders>() != null;
			Component[] components = baseModule.GetComponents<Component>();
			foreach (Component component2 in components)
			{
				if ((component2 is Collider && (((Collider)component2).isTrigger || !flag)) || GetComponent(component2.GetType()) == null)
				{
					addedComponents.Add(base.gameObject.AddComponent(component2.GetType()).GetCopyOf(component2));
				}
			}
			int childCount2 = baseModule.transform.childCount;
			for (int l = 0; l < childCount2; l++)
			{
				GameObject obj2 = Object.Instantiate(baseModule.transform.GetChild(l).gameObject);
				Vector3 localPosition2 = obj2.transform.localPosition;
				Quaternion localRotation2 = obj2.transform.localRotation;
				obj2.transform.parent = base.transform;
				obj2.transform.localPosition = localPosition2;
				obj2.transform.localRotation = localRotation2;
			}
		}
		GetComponent<SlimeSubbehaviourPlexer>().CollectSubbehaviours();
	}
}
