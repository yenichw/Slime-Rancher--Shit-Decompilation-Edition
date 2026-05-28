using System;
using System.Collections.Generic;
using UnityEngine;

public class AweTowardsAttractors : SlimeSubbehaviour
{
	public float facingStability = 1f;

	public float facingSpeed = 5f;

	private Attractor target;

	private List<Attractor> attractors = new List<Attractor>();

	private TimeDirector timeDir;

	private SlimeFaceAnimator sfAnimator;

	private double nextActivationTime;

	private float startTime;

	private float endTime;

	private const float SCOOT_CYCLE_TIME = 1f;

	private const float SCOOT_CYCLE_FACTOR = (float)Math.PI * 2f;

	private const float SCOOT_SPEED_FACTOR = 0.5f;

	public override void Awake()
	{
		base.Awake();
		startTime = Time.time;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		sfAnimator = GetComponent<SlimeFaceAnimator>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (attractors.Count == 0 || !isGrounded || !timeDir.HasReached(nextActivationTime))
		{
			return 0f;
		}
		target = Randoms.SHARED.Pick(attractors, null);
		if (target == null)
		{
			attractors.Remove(target);
			target = null;
			return 0f;
		}
		if (!(target == null))
		{
			return Randoms.SHARED.GetInRange(0.1f, 1f) * target.AweFactor(base.gameObject);
		}
		return 0f;
	}

	public override void Action()
	{
		if (target != null)
		{
			RotateTowards(SlimeSubbehaviour.GetGotoPos(target.gameObject) - base.transform.position, facingSpeed, facingStability);
			if (target.CauseMoveTowards())
			{
				ScootTowards(target.transform.position);
			}
		}
	}

	private void ScootTowards(Vector3 targetPos)
	{
		Vector3 normalized = (targetPos - base.transform.position).normalized;
		float num = ScootCycleSpeed();
		slimeBody.AddForce(normalized * (150f * slimeBody.mass * 0.5f * Time.fixedDeltaTime * num));
		Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
		slimeBody.AddForceAtPosition(normalized * (270f * slimeBody.mass * Time.fixedDeltaTime * num), position);
	}

	protected float ScootCycleSpeed()
	{
		return Mathf.Sin((Time.time - startTime) * ((float)Math.PI * 2f)) + 1f;
	}

	public override void Selected()
	{
		sfAnimator.SetTrigger("triggerLongAwe");
		nextActivationTime = timeDir.HoursFromNow(1f);
		endTime = Time.time + 3f;
	}

	public override bool CanRethink()
	{
		return Time.time >= endTime;
	}

	public void RegisterAttractor(Attractor attractor)
	{
		attractors.Add(attractor);
	}

	public void UnregisterAttractor(Attractor attractor)
	{
		attractors.Remove(attractor);
	}
}
