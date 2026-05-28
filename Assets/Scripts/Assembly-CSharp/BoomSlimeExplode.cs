using System;
using System.Collections;
using UnityEngine;

public class BoomSlimeExplode : SlimeSubbehaviour, BoomMaterialAnimator.BoomMaterialInformer
{
	private enum State
	{
		IDLE = 0,
		PREPARING = 1,
		EXPLODING = 2,
		RECOVERING = 3
	}

	public float explodePower = 600f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 15f;

	public float maxPlayerDamage = 45f;

	private GameObject explodeFX;

	private float nextPossibleExplode;

	private float nextExplodeDelayTime = BOOM_MAX_DELAY;

	private float nextRecoverTime;

	private SlimeFaceAnimator sfAnimator;

	private CalmedByWaterSpray calmed;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	public static float BOOM_MIN_DELAY = 10f;

	public static float BOOM_MAX_DELAY = 45f;

	public static float EXPLOSION_PREP_TIME = 1.5f;

	public static float EXPLOSION_RECOVERY_TIME = 5f;

	private State state;

	public override void Awake()
	{
		base.Awake();
		sfAnimator = GetComponent<SlimeFaceAnimator>();
		calmed = GetComponent<CalmedByWaterSpray>();
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		if (slimeAppearanceApplicator.Appearance != null)
		{
			explodeFX = slimeAppearanceApplicator.Appearance.ExplosionAppearance.explodeFx;
		}
		slimeAppearanceApplicator.OnAppearanceChanged += delegate(SlimeAppearance appearance)
		{
			explodeFX = appearance.ExplosionAppearance.explodeFx;
		};
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (Time.time + BOOM_MIN_DELAY > nextPossibleExplode)
		{
			nextPossibleExplode = Math.Max(nextPossibleExplode, Time.time + Randoms.SHARED.GetFloat(BOOM_MIN_DELAY));
		}
	}

	public override void Start()
	{
		base.Start();
		nextExplodeDelayTime = BoomDelay();
		nextPossibleExplode = Time.time + nextExplodeDelayTime * Randoms.SHARED.GetInRange(0.25f, 1f);
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: false);
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!(Time.fixedTime > nextPossibleExplode) || calmed.IsCalmed())
		{
			return 0f;
		}
		return 1f;
	}

	public override void Action()
	{
	}

	public override void Selected()
	{
		StartCoroutine(DelayedExplosion());
	}

	public void FixedUpdate()
	{
		if (calmed.IsCalmed())
		{
			nextPossibleExplode += Time.fixedDeltaTime;
		}
	}

	private float BoomDelay()
	{
		return Mathf.Lerp(BOOM_MIN_DELAY, BOOM_MAX_DELAY, Mathf.Clamp(Randoms.SHARED.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f));
	}

	private IEnumerator DelayedExplosion()
	{
		state = State.PREPARING;
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: true);
		sfAnimator.SetTrigger("triggerGrimace");
		yield return new WaitForSeconds(EXPLOSION_PREP_TIME);
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: false);
		state = State.EXPLODING;
		SRBehaviour.SpawnAndPlayFX(explodeFX, base.transform.position, base.transform.rotation);
		Explode();
		nextExplodeDelayTime = BoomDelay();
		nextPossibleExplode = Time.time + nextExplodeDelayTime;
		state = State.RECOVERING;
		sfAnimator.SetTrigger("triggerFried");
		nextRecoverTime = Time.time + EXPLOSION_RECOVERY_TIME;
		yield return new WaitForSeconds(EXPLOSION_RECOVERY_TIME);
		state = State.IDLE;
	}

	private void Explode()
	{
		PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage);
		if (base.gameObject.layer == LayerMask.NameToLayer("Launched"))
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.LAUNCHED_BOOM_EXPLODE, 1);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		state = State.IDLE;
	}

	public override bool CanRethink()
	{
		return state == State.IDLE;
	}

	public float GetReadiness()
	{
		return 1f - Mathf.Clamp((nextPossibleExplode - Time.time) / nextExplodeDelayTime, 0f, 1f);
	}

	public float GetRecoveriness()
	{
		if (state != State.RECOVERING)
		{
			return 0f;
		}
		return Mathf.Clamp((nextRecoverTime - Time.time) / EXPLOSION_RECOVERY_TIME, 0f, 1f);
	}
}
