using System.Collections.Generic;
using UnityEngine;

public class StalkConsumable : FindConsumable, Collidable, RegistryUpdateable
{
	private enum Mode
	{
		NONE = 0,
		APPROACH = 1,
		WAIT = 2,
		PREP = 3,
		POUNCE = 4,
		FEINT = 5,
		PIVOT = 6
	}

	private class CalmableDriveCalculator : DriveCalculator
	{
		private CalmedByWaterSpray calmed;

		private float calmedExtraDrive;

		public CalmableDriveCalculator(CalmedByWaterSpray calmed, SlimeEmotions.Emotion emotion, float normalBonus, float calmedBonus)
			: base(emotion, normalBonus, 0f)
		{
			calmedExtraDrive = calmedBonus;
			this.calmed = calmed;
		}

		public override float Drive(SlimeEmotions emotions, Identifiable.Id id)
		{
			return Mathf.Max(0f, emotions.GetCurr(emotion) + (calmed.IsCalmed() ? calmedExtraDrive : extraDrive));
		}
	}

	[Tooltip("Whether this should engage targets with parkour.")]
	public bool doesParkour;

	[Tooltip("The power of the feint jump will be modified by this. A value of 1.0 is a normal power jump.")]
	public float feintPowerMultiplier = 1f;

	[Tooltip("The minimum angle away from the player that the feint should be toward.")]
	public float feintMinAngle = 55f;

	[Tooltip("The maximum angle away from the player that the feint should be toward.")]
	public float feintMaxAngle = 80f;

	private GameObject target;

	private float currDrive;

	private bool pouncing;

	private bool feinting;

	private double pounceResetTime;

	private bool pounceFromPivot;

	private Mode mode;

	private double endModeTime;

	private double nextStalkTime;

	private double initStealthUntil;

	private const float POUNCE_DIST = 8f;

	private const float POUNCE_DIST_SQR = 64f;

	private const float APPROACH_TIME = 3f;

	private const float WAIT_TIME = 3f;

	private const float PREP_TIME = 2f;

	private const float POUNCE_TIME = 1f;

	private const float FEINT_TIME = 1f;

	private const float PIVOT_TIME = 1.5f;

	private const float POUNCE_RESET_TIME = 15f;

	private const float PLAYER_EAT_EXTRA_DRIVE = -0.1f;

	private const float CALMED_PLAYER_EAT_EXTRA_DRIVE = -1f;

	private int animButtWiggleId;

	private SlimeStealth stealth;

	private bool pivotNow;

	private Identifiable identifiable;

	public override void Awake()
	{
		base.Awake();
		stealth = GetComponent<SlimeStealth>();
		animButtWiggleId = Animator.StringToHash("ButtWiggle");
		identifiable = GetComponent<Identifiable>();
	}

	public override void Start()
	{
		base.Start();
		searchIds[Identifiable.Id.PLAYER] = new CalmableDriveCalculator(GetComponent<CalmedByWaterSpray>(), SlimeEmotions.Emotion.NONE, -0.1f, -1f);
	}

	public override bool Forbids(SlimeSubbehaviour toMaybeForbid)
	{
		return toMaybeForbid is GotoConsumable;
	}

	public override float Relevancy(bool isGrounded)
	{
		if ((double)Time.time < nextStalkTime)
		{
			return 0f;
		}
		target = FindNearestConsumable(out currDrive);
		if (target == null)
		{
			return 0f;
		}
		if (!(target == null))
		{
			return currDrive * currDrive;
		}
		return 0f;
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		Dictionary<Identifiable.Id, DriveCalculator> dictionary = base.GetSearchIds();
		dictionary[Identifiable.Id.PLAYER] = new CalmableDriveCalculator(GetComponent<CalmedByWaterSpray>(), SlimeEmotions.Emotion.NONE, -0.1f, -1f);
		return dictionary;
	}

	public override bool CanRethink()
	{
		return mode == Mode.NONE;
	}

	public override void Selected()
	{
		SetStealth(isStealthed: true);
	}

	public override void Deselected()
	{
		base.Deselected();
		SetMode(Mode.NONE);
		SetStealth(isStealthed: false);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		target = null;
	}

	public override void Action()
	{
		if (target == null)
		{
			SetMode(Mode.NONE);
			endModeTime = 0.0;
			return;
		}
		if (mode == Mode.NONE)
		{
			bool flag = (SlimeSubbehaviour.GetGotoPos(target) - base.transform.position).sqrMagnitude < 64f;
			SetMode((!flag) ? Mode.APPROACH : Mode.PREP);
			endModeTime = Time.time + (flag ? 2f : 3f);
		}
		if (mode == Mode.APPROACH)
		{
			if (IsGrounded())
			{
				Vector3 vector = SlimeSubbehaviour.GetGotoPos(target) - base.transform.position;
				Vector3 normalized = vector.normalized;
				RotateTowards(normalized);
				Rigidbody component = GetComponent<Rigidbody>();
				float num = ((component.velocity.sqrMagnitude < 10f) ? 650 : 400);
				Vector3 normalized2 = (normalized * 400f + Vector3.up * num).normalized;
				component.AddForce(normalized2 * (250f * component.mass * pursuitSpeedFactor * Time.fixedDeltaTime));
				Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
				component.AddForceAtPosition(normalized2 * (450f * component.mass * Time.fixedDeltaTime), position);
				if (vector.sqrMagnitude < 64f)
				{
					SetMode(Mode.PREP);
					endModeTime = Time.fixedTime + 2f;
				}
			}
			if (mode == Mode.APPROACH && (double)Time.fixedTime > endModeTime)
			{
				SetMode(Mode.WAIT);
				endModeTime = Time.fixedTime + 3f;
			}
		}
		else if (mode == Mode.WAIT)
		{
			if ((double)Time.fixedTime > endModeTime)
			{
				SetMode(Mode.NONE);
				endModeTime = 0.0;
			}
		}
		else if (mode == Mode.PREP)
		{
			if ((double)Time.fixedTime > endModeTime)
			{
				if (doesParkour)
				{
					feinting = false;
					SetMode(Mode.FEINT);
					endModeTime = Time.time + 1f;
				}
				else
				{
					feinting = false;
					pounceFromPivot = false;
					SetMode(Mode.POUNCE);
					endModeTime = Time.time + 1f;
				}
			}
		}
		else if (mode == Mode.POUNCE)
		{
			if (IsGrounded() || pounceFromPivot)
			{
				if (pounceFromPivot)
				{
					GetComponent<Rigidbody>().velocity = Vector3.zero;
				}
				Vector3 vector2 = SlimeSubbehaviour.GetGotoPos(target) - base.transform.position;
				float sqrMagnitude = vector2.sqrMagnitude;
				Vector3 normalized3 = vector2.normalized;
				LeapToward(sqrMagnitude, normalized3, normalized3);
				SetMode(Mode.NONE);
				endModeTime = 0.0;
				target = null;
				pouncing = true;
				pounceResetTime = SRSingleton<SceneContext>.Instance.TimeDirector.WorldTime() + 60.0;
				pounceFromPivot = false;
				nextStalkTime = Time.fixedTime + 15f;
			}
			if ((double)Time.fixedTime > endModeTime)
			{
				SetMode(Mode.NONE);
				endModeTime = 0.0;
				feinting = false;
			}
		}
		else if (mode == Mode.FEINT)
		{
			if (IsGrounded())
			{
				Vector3 vector3 = SlimeSubbehaviour.GetGotoPos(target) - base.transform.position;
				float sqrMagnitude2 = vector3.sqrMagnitude;
				Vector3 normalized4 = vector3.normalized;
				float angle = Randoms.SHARED.GetInRange(feintMinAngle, feintMaxAngle) * (float)((!Randoms.SHARED.GetBoolean()) ? 1 : (-1));
				LeapToward(sqrMagnitude2 * Mathf.Pow(feintPowerMultiplier, 2f), Quaternion.AngleAxis(angle, Vector3.up) * normalized4, normalized4);
				feinting = true;
				SetMode(Mode.PIVOT);
				endModeTime = Time.time + 1.5f;
			}
			if ((double)Time.fixedTime > endModeTime)
			{
				SetMode(Mode.NONE);
				endModeTime = 0.0;
				feinting = false;
			}
		}
		else if (mode == Mode.PIVOT)
		{
			if (pivotNow)
			{
				pivotNow = false;
				pounceFromPivot = true;
				SetMode(Mode.POUNCE);
				endModeTime = Time.time + 1f;
			}
			if ((double)Time.fixedTime > endModeTime)
			{
				SetMode(Mode.NONE);
				endModeTime = 0.0;
				feinting = false;
			}
		}
	}

	private void LeapToward(float distanceSquared, Vector3 directionToJump, Vector3 directionToFace)
	{
		RotateTowards(directionToFace);
		float num = 1.2f;
		float num2 = Mathf.Sqrt(Mathf.Sqrt(distanceSquared) * Physics.gravity.magnitude) * num;
		GetComponent<Rigidbody>().AddForce((directionToJump + Vector3.up).normalized * num2, ForceMode.VelocityChange);
		slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
		slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
	}

	public void RegistryUpdate()
	{
		if (hasStarted && IsGrounded() && SRSingleton<SceneContext>.Instance.TimeDirector.HasReached(pounceResetTime))
		{
			pouncing = false;
		}
	}

	public void ProcessCollisionEnter(Collision col)
	{
		if (Identifiable.BOOP_CLASS.Contains(identifiable.id) && pouncing && stealth == null && col.gameObject == SRSingleton<SceneContext>.Instance.Player)
		{
			Vector3 vector = col.gameObject.transform.InverseTransformPoint(col.contacts[0].point);
			if (vector.z > 0.2f && vector.y > 1f)
			{
				SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.TABBY_HEADBUTT, 1);
			}
		}
		else if (feinting)
		{
			pivotNow = true;
			feinting = false;
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	private void SetMode(Mode mode)
	{
		if (this.mode != mode)
		{
			this.mode = mode;
			GetComponentInChildren<Animator>().SetBool(animButtWiggleId, mode == Mode.PREP);
			if (mode == Mode.PREP)
			{
				slimeAudio.Play(slimeAudio.slimeSounds.wiggleCue);
			}
		}
	}

	private void SetStealth(bool isStealthed)
	{
		if (stealth != null)
		{
			stealth.SetStealth(isStealthed);
		}
	}
}
