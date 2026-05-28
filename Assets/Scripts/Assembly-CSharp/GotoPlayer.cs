using System.Collections.Generic;
using UnityEngine;

public class GotoPlayer : FindConsumable
{
	private enum Mode
	{
		AVAIL = 0,
		ATTEMPTING = 1,
		GIVE_UP = 2
	}

	public float maxJump = 12f;

	public float attemptTime = 10f;

	public float giveUpTime = 10f;

	public bool shouldGotoPlayer;

	public SlimeEmotions.Emotion driver;

	public float extraDrive;

	public float minDrive;

	private GameObject target;

	private float currDrive;

	private float nextLeapAvail;

	private Chomper chomper;

	private const float AGITATION_PER_GIVE_UP = 0.1f;

	private Mode mode;

	private float modeEndTime;

	private const float MAX_VEL_TO_BOUNCE = 0.1f;

	private const float SQR_MAX_VEL_TO_BOUNCE = 0.010000001f;

	public override void Awake()
	{
		base.Awake();
		chomper = GetComponent<Chomper>();
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		return new Dictionary<Identifiable.Id, DriveCalculator> { [Identifiable.Id.PLAYER] = new DriveCalculator(driver, extraDrive, minDrive) };
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!shouldGotoPlayer)
		{
			return 0f;
		}
		if (Time.time >= modeEndTime)
		{
			if (mode == Mode.ATTEMPTING)
			{
				mode = Mode.GIVE_UP;
				modeEndTime = Time.time + giveUpTime;
				emotions.Adjust(SlimeEmotions.Emotion.AGITATION, 0.1f);
			}
			else if (mode == Mode.GIVE_UP)
			{
				mode = Mode.AVAIL;
				modeEndTime = float.PositiveInfinity;
			}
		}
		if (mode == Mode.GIVE_UP)
		{
			return 0f;
		}
		target = FindNearestConsumable(out currDrive);
		if (!(target == null))
		{
			return currDrive * currDrive * 0.95f;
		}
		return 0f;
	}

	public override void Selected()
	{
		mode = Mode.ATTEMPTING;
		modeEndTime = Time.time + attemptTime;
	}

	public override void Action()
	{
		if (target != null && !SRSingleton<SceneContext>.Instance.TimeDirector.IsFastForwarding() && !chomper.IsChomping())
		{
			MoveTowards(SlimeSubbehaviour.GetGotoPos(target), IsBlocked(target), ref nextLeapAvail, DriveToJumpiness(currDrive) * maxJump);
		}
	}

	private float DriveToJumpiness(float drive)
	{
		float num = Mathf.Max(0f, drive - 0.666f) / 0.334f;
		return Mathf.Lerp(0.4f, 1f, num * num);
	}
}
