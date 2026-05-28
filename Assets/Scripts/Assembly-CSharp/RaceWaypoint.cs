using System.Collections.Generic;
using UnityEngine;

public class RaceWaypoint : MonoBehaviour
{
	[Tooltip("Radius which a slime has to reach to trigger having reached this checkpoint")]
	public float checkpointRad = 5f;

	[Tooltip("Factor by which an approaching slime will scale their forward force")]
	public float approachForceFactor = 1f;

	[Tooltip("The waypoints a slime which reaches this waypoint should travel to next.")]
	public RaceWaypoint[] next;

	private static List<RaceWaypoint> allWaypoints = new List<RaceWaypoint>();

	public const float TRIGGER_RAD = 15f;

	public const float SQR_TRIGGER_RAD = 225f;

	public void Awake()
	{
		allWaypoints.Add(this);
	}

	public void OnDestroy()
	{
		allWaypoints.Remove(this);
	}

	public RaceWaypoint GetNext()
	{
		return Randoms.SHARED.Pick(next, null);
	}

	public static RaceWaypoint GetNearest(Vector3 position, float maxDistSqr)
	{
		RaceWaypoint result = null;
		float num = maxDistSqr;
		foreach (RaceWaypoint allWaypoint in allWaypoints)
		{
			float sqrMagnitude = (allWaypoint.transform.position - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = allWaypoint;
			}
		}
		return result;
	}

	public bool HasHitCheckpoint(Vector3 checkPos)
	{
		return (checkPos - base.transform.position).sqrMagnitude <= checkpointRad * checkpointRad;
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, checkpointRad);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, 15f);
	}
}
