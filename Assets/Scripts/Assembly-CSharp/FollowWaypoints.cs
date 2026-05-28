using UnityEngine;

public class FollowWaypoints : SlimeSubbehaviour
{
	public float straightlineForceFactor = 1f;

	public float facingStability = 1f;

	public float facingSpeed = 5f;

	public float slowSpeedLimit = 0.35f;

	[Tooltip("Factor multiplied instantly to the slime's velocity when slow is applied.")]
	public float slowSpeedInstantFactor = 0.6f;

	[Tooltip("Maximum slime starting velocity when the slow is applied.")]
	public float slowSpeedInstantMaxVelocity = 18f;

	[Tooltip("Delay, in game seconds, between rotation changes. Helps reduce jitter.")]
	public float rotationDelay = 10f;

	private RaceWaypoint nextWaypoint;

	private double resetAfter = double.PositiveInfinity;

	private double disableUntil;

	private double slowUntil;

	private double rotateTime;

	private TimeDirector timeDir;

	private const float TRY_TO_FOLLOW_TIME = 1f / 6f;

	private const float RESET_DISABLE_TIME = 1f / 6f;

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!timeDir.HasReached(disableUntil))
		{
			return 0f;
		}
		if (nextWaypoint == null)
		{
			nextWaypoint = RaceWaypoint.GetNearest(base.transform.position, 225f);
			if (nextWaypoint != null)
			{
				resetAfter = timeDir.HoursFromNow(1f / 6f);
			}
		}
		if (!(nextWaypoint == null))
		{
			return 0.8f;
		}
		return 0f;
	}

	public override void Selected()
	{
	}

	public override void Deselected()
	{
		base.Deselected();
		nextWaypoint = null;
		resetAfter = double.PositiveInfinity;
		disableUntil = 0.0;
		rotateTime = 0.0;
	}

	public override void Action()
	{
		if (timeDir.HasReached(resetAfter))
		{
			disableUntil = timeDir.HoursFromNow(1f / 6f);
			nextWaypoint = null;
			resetAfter = double.PositiveInfinity;
		}
		else
		{
			if (!(nextWaypoint != null))
			{
				return;
			}
			if (nextWaypoint.HasHitCheckpoint(base.transform.position))
			{
				nextWaypoint = nextWaypoint.GetNext();
				if (nextWaypoint != null)
				{
					resetAfter = timeDir.HoursFromNow(1f / 6f);
				}
			}
			if (nextWaypoint != null)
			{
				Vector3 normalized = (nextWaypoint.transform.position - base.transform.position).normalized;
				if (timeDir.HasReached(rotateTime))
				{
					RotateTowards(normalized, facingSpeed, facingStability);
					rotateTime = timeDir.HoursFromNow(rotationDelay * 0.00027777778f);
				}
				if (IsGrounded())
				{
					MoveTowards(normalized, Mathf.Min(timeDir.HasReached(slowUntil) ? 1f : slowSpeedLimit, nextWaypoint.approachForceFactor));
				}
			}
		}
	}

	public void ApplySlow(float durationGameHrs)
	{
		if (timeDir.HasReached(slowUntil))
		{
			if (slimeBody.velocity.sqrMagnitude > slowSpeedInstantMaxVelocity * slowSpeedInstantMaxVelocity)
			{
				slimeBody.velocity = slimeBody.velocity.normalized * slowSpeedInstantMaxVelocity;
			}
			slimeBody.velocity *= slowSpeedInstantFactor;
		}
		slowUntil = timeDir.HoursFromNow(durationGameHrs);
	}

	private void MoveTowards(Vector3 direction, float approachForceFactor)
	{
		slimeBody.AddForce(direction * (straightlineForceFactor * approachForceFactor * 80f * slimeBody.mass * Time.fixedDeltaTime));
		Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
		slimeBody.AddForceAtPosition(direction * (straightlineForceFactor * approachForceFactor * 240f * slimeBody.mass * Time.fixedDeltaTime), position);
	}
}
