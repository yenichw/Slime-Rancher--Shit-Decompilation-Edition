using System.Collections.Generic;
using UnityEngine;

public class CorralRegion : SRBehaviour
{
	private static List<CorralRegion> allCorrals = new List<CorralRegion>();

	public void Awake()
	{
		allCorrals.Add(this);
	}

	public void OnDestroy()
	{
		allCorrals.Remove(this);
	}

	public static bool IsWithin(Vector3 pos)
	{
		foreach (CorralRegion allCorral in allCorrals)
		{
			if (allCorral.GetComponent<Collider>().bounds.Contains(pos))
			{
				return true;
			}
		}
		return false;
	}
}
