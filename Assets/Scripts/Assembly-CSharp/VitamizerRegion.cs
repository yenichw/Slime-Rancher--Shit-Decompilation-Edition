using System.Collections.Generic;
using UnityEngine;

public class VitamizerRegion : SRBehaviour
{
	public const float TWINS_PROB = 0.5f;

	private static List<VitamizerRegion> allVitamizers = new List<VitamizerRegion>();

	public void Awake()
	{
		allVitamizers.Add(this);
	}

	public void OnDestroy()
	{
		allVitamizers.Remove(this);
	}

	public static bool IsWithin(Vector3 pos)
	{
		foreach (VitamizerRegion allVitamizer in allVitamizers)
		{
			if (allVitamizer.GetComponent<Collider>().bounds.Contains(pos))
			{
				return true;
			}
		}
		return false;
	}
}
