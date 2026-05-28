using System.Collections.Generic;
using UnityEngine;

public class QuantumCeiling : SRBehaviour
{
	public static List<QuantumCeiling> Instances = new List<QuantumCeiling>();

	private Collider collider;

	public void Awake()
	{
		Instances.Add(this);
		collider = GetRequiredComponent<Collider>();
	}

	public void OnDestroy()
	{
		Instances.Remove(this);
	}

	public static float AdjustMinDist(Vector3 pos, float defaultDist)
	{
		float num = defaultDist;
		foreach (QuantumCeiling instance in Instances)
		{
			if (instance.isActiveAndEnabled)
			{
				Vector3 vector = instance.collider.ClosestPoint(pos);
				if (vector.x == pos.x && vector.z == pos.z)
				{
					num = Mathf.Max(num, pos.y - instance.collider.bounds.min.y);
				}
			}
		}
		return num;
	}
}
