using System.Collections.Generic;
using UnityEngine;

public class CoopRegion : SRBehaviour
{
	private static List<CoopRegion> allCoops = new List<CoopRegion>();

	private bool isDeluxe;

	public void Awake()
	{
		allCoops.Add(this);
	}

	public void OnDestroy()
	{
		allCoops.Remove(this);
	}

	public void SetDeluxe()
	{
		isDeluxe = true;
	}

	public static bool IsWithin(Vector3 pos)
	{
		return IsWithin(pos, mustBeDeluxe: false);
	}

	public static bool IsWithinDeluxe(Vector3 pos)
	{
		return IsWithin(pos, mustBeDeluxe: true);
	}

	private static bool IsWithin(Vector3 pos, bool mustBeDeluxe)
	{
		bool flag = false;
		foreach (CoopRegion allCoop in allCoops)
		{
			if (allCoop.GetComponent<Collider>().bounds.Contains(pos))
			{
				flag |= !mustBeDeluxe || allCoop.isDeluxe;
			}
		}
		return flag;
	}
}
