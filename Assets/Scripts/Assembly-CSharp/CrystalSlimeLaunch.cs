using System;
using System.Collections;
using UnityEngine;

public class CrystalSlimeLaunch : SlimeSubbehaviour
{
	private enum State
	{
		IDLE = 0,
		PREPARING = 1,
		LAUNCHED = 2
	}

	private static readonly int ANIMATION_ROCK_MODE = Animator.StringToHash("RockMode");

	private const float LAUNCH_MIN_HOURS = 0.050000004f;

	private const float LAUNCH_MAX_HOURS = 0.25f;

	private const float MIN_LAUNCH_HOURS = 0.0016666668f;

	private const float PREP_HOURS = 1f / 60f;

	private const float LAUNCH_FORCE = 200f;

	private const float LAUNCH_FORCE_FORWARD = 40f;

	private const float SPIKES_SPREAD = 1.5f;

	private const int LAYER_MASK = 2129921;

	private GameObject launchSpawnLarge;

	private GameObject launchSpawnSmall;

	private TimeDirector timeDirector;

	private Animator animatorBase;

	private SlimeFaceAnimator animatorFace;

	private CalmedByWaterSpray calmed;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private SlimeAudio audio;

	private Vector3 rollDirection;

	private Vector3 rollForward;

	private double nextLaunchTime;

	private State state;

	private double stateEndTime;

	public override void Awake()
	{
		base.Awake();
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		animatorBase = GetComponentInChildren<Animator>();
		animatorFace = GetComponent<SlimeFaceAnimator>();
		audio = GetComponent<SlimeAudio>();
		calmed = GetComponent<CalmedByWaterSpray>();
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += UpdateSpikeAppearance;
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateSpikeAppearance(slimeAppearanceApplicator.Appearance);
		}
	}

	public override void Start()
	{
		base.Start();
		ResetNextLaunchTime();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!isGrounded || !timeDirector.HasReached(nextLaunchTime) || calmed.IsCalmed())
		{
			return 0f;
		}
		rollDirection = base.transform.right;
		rollDirection.y = 0f;
		rollDirection.Normalize();
		rollForward = Vector3.Cross(rollDirection, Vector3.up);
		return 0.3f;
	}

	public override void Selected()
	{
		stateEndTime = timeDirector.HoursFromNow(1f / 60f);
		state = State.PREPARING;
		animatorFace.SetTrigger("triggerAwe");
		audio.Play(audio.slimeSounds.rollCue);
	}

	public override void Deselected()
	{
		base.Deselected();
		animatorBase.SetBool(ANIMATION_ROCK_MODE, value: false);
		state = State.IDLE;
	}

	public override bool CanRethink()
	{
		return state == State.IDLE;
	}

	public override void Action()
	{
		if (calmed.IsCalmed())
		{
			plexer.ForceRethink();
			return;
		}
		if (timeDirector.HasReached(stateEndTime) && IsGrounded())
		{
			if (state == State.PREPARING)
			{
				StartCoroutine(CreateSpikes());
				stateEndTime = timeDirector.HoursFromNow(0.0016666668f);
				state = State.LAUNCHED;
			}
			else if (state == State.LAUNCHED)
			{
				ResetNextLaunchTime();
				state = State.IDLE;
				stateEndTime = 0.0;
			}
		}
		if (state == State.LAUNCHED)
		{
			slimeBody.AddTorque(rollDirection * (1200f * slimeBody.mass * Time.fixedDeltaTime));
		}
	}

	private void UpdateSpikeAppearance(SlimeAppearance appearance)
	{
		launchSpawnLarge = appearance.CrystalAppearance.largeCrystalPrefab;
		launchSpawnSmall = appearance.CrystalAppearance.smallCrystalPrefab;
	}

	private IEnumerator CreateSpikes()
	{
		animatorBase.SetBool(ANIMATION_ROCK_MODE, value: true);
		slimeBody.AddForce(Vector3.up * 200f + rollForward * 40f * slimeBody.mass);
		CreateSpikes(launchSpawnLarge, base.transform.position);
		yield return new WaitForSeconds(0.2f);
		int inRange = Randoms.SHARED.GetInRange(Mathf.CeilToInt(4f * slimeBody.mass), Mathf.CeilToInt(7f * slimeBody.mass));
		float num = Randoms.SHARED.GetFloat((float)Math.PI * 2f);
		int num2 = 0;
		while (num2 < inRange)
		{
			CreateSpikes(launchSpawnSmall, base.transform.position + new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num)) * 1.5f);
			num2++;
			num += (float)Math.PI * 2f / (float)inRange;
		}
	}

	private bool CreateSpikes(GameObject prefab, Vector3 position)
	{
		RaycastHit? raycastHit = null;
		RaycastHit[] array = Physics.RaycastAll(new Ray(position, Vector3.down), 2f, 2129921, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit value = array[i];
			if (value.rigidbody == null && (!raycastHit.HasValue || value.distance < raycastHit.Value.distance))
			{
				raycastHit = value;
			}
		}
		if (raycastHit.HasValue)
		{
			SRBehaviour.InstantiateDynamic(prefab, raycastHit.Value.point, Quaternion.Euler(0f, Randoms.SHARED.GetFloat(360f), 0f));
			return true;
		}
		return false;
	}

	private void ResetNextLaunchTime()
	{
		nextLaunchTime = timeDirector.HoursFromNow(Mathf.Lerp(0.050000004f, 0.25f, Mathf.Clamp(Randoms.SHARED.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f)));
	}
}
