using System;
using System.Collections.Generic;
using UnityEngine;

public class SlimeRandomMove : SlimeSubbehaviour
{
	private enum Mode
	{
		IDLE = 0,
		SCOOT = 1,
		JUMP = 2
	}

	public float verticalFactor = 1f;

	public float scootSpeedFactor = 1f;

	private float maxJump = 6f;

	private SlimeAudio slimeAudio;

	private float nextJumpTime;

	private Mode mode;

	private float modeChangeTime;

	private Vector3 scootDir;

	private const float TIME_BETWEEN_JUMPS = 1f;

	private const float MODE_CHANGE_LENGTH = 10f;

	private const float SCOOT_CYCLE_TIME = 1f;

	private const float SCOOT_CYCLE_FACTOR = (float)Math.PI * 2f;

	private static Dictionary<Mode, float> MODE_WEIGHTS;

	private const float MAX_VEL_TO_BOUNCE = 5f;

	private const float SQR_MAX_VEL_TO_BOUNCE = 25f;

	static SlimeRandomMove()
	{
		MODE_WEIGHTS = new Dictionary<Mode, float>();
		MODE_WEIGHTS[Mode.IDLE] = 0.2f;
		MODE_WEIGHTS[Mode.SCOOT] = 0.3f;
		MODE_WEIGHTS[Mode.JUMP] = 0.5f;
	}

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
	}

	public override void Start()
	{
		base.Start();
	}

	public override float Relevancy(bool isGrounded)
	{
		return 0.2f;
	}

	public override void Selected()
	{
	}

	public override void Action()
	{
		if (!IsGrounded())
		{
			return;
		}
		if (Time.fixedTime > modeChangeTime)
		{
			mode = Randoms.SHARED.Pick(MODE_WEIGHTS, Mode.IDLE);
			modeChangeTime = Time.time + 10f;
			float f = Mathf.Atan2(base.transform.forward.z, base.transform.forward.x) + Randoms.SHARED.GetInRange(-0.5f, 0.5f);
			scootDir = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		}
		switch (mode)
		{
		case Mode.JUMP:
			if (Time.time > nextJumpTime && slimeBody.velocity.sqrMagnitude <= 25f && IsGrounded())
			{
				float num2 = 0.5f * maxJump * slimeBody.mass;
				slimeBody.AddForce(Randoms.SHARED.GetInRange(0f - num2, num2), verticalFactor * Randoms.SHARED.GetInRange(num2, maxJump * slimeBody.mass), Randoms.SHARED.GetInRange(0f - num2, num2), ForceMode.Impulse);
				slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
				slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
				nextJumpTime = Time.fixedTime + 1f;
			}
			break;
		case Mode.SCOOT:
		{
			RotateTowards(scootDir, 1f, 1f);
			float num = ScootCycleSpeed();
			slimeBody.AddForce(base.transform.forward * (150f * slimeBody.mass * scootSpeedFactor * Time.fixedDeltaTime * num));
			Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
			slimeBody.AddForceAtPosition(base.transform.forward * (270f * slimeBody.mass * Time.fixedDeltaTime * num), position);
			break;
		}
		case Mode.IDLE:
			break;
		}
	}

	protected float ScootCycleSpeed()
	{
		return Mathf.Sin(Time.fixedTime * ((float)Math.PI * 2f)) + 1f;
	}
}
