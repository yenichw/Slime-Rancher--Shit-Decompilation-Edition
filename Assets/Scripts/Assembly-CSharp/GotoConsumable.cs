using System.Collections.Generic;
using UnityEngine;

public class GotoConsumable : FindConsumable
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

	private GameObject target;

	private float currDrive;

	private float nextLeapAvail;

	private SlimeEat eat;

	private const float AGITATION_PER_GIVE_UP = 0.1f;

	private int fastForwardLayerMask;

	private Mode mode;

	private float modeEndTime;

	private static List<Identifiable.Id> reuseIdList = new List<Identifiable.Id>();

	private static readonly List<Identifiable.Id> alreadyCollectedList = new List<Identifiable.Id>();

	private const float MAX_VEL_TO_BOUNCE = 0.1f;

	private const float SQR_MAX_VEL_TO_BOUNCE = 0.010000001f;

	public override void Awake()
	{
		base.Awake();
		eat = GetComponent<SlimeEat>();
		fastForwardLayerMask = LayerMask.GetMask("Actor", "ActorEchoes", "ActorIgnorePlayer");
	}

	public override float Relevancy(bool isGrounded)
	{
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
		GetComponent<SlimeFaceAnimator>().SetSeekingFood(val: true);
	}

	public override void Deselected()
	{
		base.Deselected();
		GetComponent<SlimeFaceAnimator>().SetSeekingFood(val: false);
	}

	public override void Action()
	{
		if (!(target != null))
		{
			return;
		}
		if (SRSingleton<SceneContext>.Instance.TimeDirector.IsFastForwarding() && CellDirector.IsOnRanch(member))
		{
			if (!IsBlocked(target, fastForwardLayerMask, forceCheckFullDist: true))
			{
				Identifiable.Id id = Identifiable.GetId(target);
				if (id != Identifiable.Id.PLAYER)
				{
					eat.EatImmediate(target, id, eat.GetProducedIds(id, reuseIdList), alreadyCollectedList, skipDelays: false);
				}
			}
		}
		else if (!eat.IsChomping())
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
