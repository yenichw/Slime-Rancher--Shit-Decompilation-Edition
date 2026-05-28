using System.Collections.Generic;
using UnityEngine;

public class SlimeHoopTrigger : MonoBehaviour
{
	public SlimeHoop hoop;

	public List<Collider> passingDownwards = new List<Collider>();

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger)
		{
			Identifiable componentInParent = col.GetComponentInParent<Identifiable>();
			if (componentInParent != null && Identifiable.IsSlime(componentInParent.id) && col.transform.position.y > base.transform.position.y && col.GetComponent<Rigidbody>().velocity.y < 10f)
			{
				passingDownwards.Add(col);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable componentInParent = col.GetComponentInParent<Identifiable>();
		if (componentInParent != null && Identifiable.IsSlime(componentInParent.id) && passingDownwards.Contains(col))
		{
			passingDownwards.Remove(col);
			if (col.transform.position.y < base.transform.position.y)
			{
				hoop.AddScore();
			}
		}
	}

	public void Update()
	{
		for (int num = passingDownwards.Count - 1; num >= 0; num--)
		{
			if (passingDownwards[num] == null)
			{
				passingDownwards.RemoveAt(num);
			}
		}
	}
}
