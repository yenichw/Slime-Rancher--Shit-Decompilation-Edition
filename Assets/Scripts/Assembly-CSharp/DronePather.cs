using UnityEngine;

public class DronePather : Pather
{
	private float sqrMaxConnectionDist;

	public DronePather(float maxConnDist)
	{
		sqrMaxConnectionDist = maxConnDist * maxConnDist;
	}

	private static bool PathIsBlocked(Vector3 start, Vector3 end)
	{
		Vector3 vector = end - start;
		RaycastHit hitInfo;
		return Physics.SphereCast(start, 0.5f, vector.normalized, out hitInfo, vector.magnitude, -537968901);
	}

	protected override bool PathPredicate(Vector3 start, Vector3 end)
	{
		if ((start - end).sqrMagnitude <= sqrMaxConnectionDist)
		{
			return !PathIsBlocked(start, end);
		}
		return false;
	}

	protected override bool NearestAccessibleNodePredicate(Vector3 start, Vector3 end)
	{
		return PathPredicate(start, end);
	}
}
