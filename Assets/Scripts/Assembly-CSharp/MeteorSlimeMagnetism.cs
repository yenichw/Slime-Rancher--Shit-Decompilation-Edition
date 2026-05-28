using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSlimeMagnetism : SlimeSubbehaviour
{
	private enum State
	{
		IDLE = 0,
		PREPARING = 1,
		ATTRACTING = 2,
		RECOVERING = 3
	}

	private class LowGravity : SRBehaviour
	{
		private Rigidbody body;

		private Vector3 antiGrav;

		private float deathTime;

		private GameObject lowGravFX;

		public void Awake()
		{
			body = GetComponent<Rigidbody>();
			deathTime = Time.time + 10f;
		}

		public void SetLowGrav(float factor, GameObject fxPrefab)
		{
			antiGrav = Physics.gravity * (-1f + factor);
			lowGravFX = Object.Instantiate(fxPrefab);
			lowGravFX.transform.SetParent(base.transform, worldPositionStays: false);
		}

		public void OnDestroy()
		{
			Destroyer.Destroy(lowGravFX, "MeteorSlimeMagnetism.OnDestroy");
		}

		public void FixedUpdate()
		{
			if (Time.fixedTime >= deathTime)
			{
				Destroyer.Destroy(this, "MeteorSlimeMagnetism.FixedUpdate");
			}
			else
			{
				body.AddForce(antiGrav, ForceMode.Acceleration);
			}
		}
	}

	public float attractPower = 600f;

	public float attractRadius = 12f;

	public GameObject attractFX;

	public GameObject lowGravFX;

	private float nextPossibleAttract;

	private float nextExplodeDelayTime = 45f;

	private float nextRecoverTime;

	private SlimeFaceAnimator sfAnimator;

	private List<Rigidbody> attractees = new List<Rigidbody>();

	private const float ATTRACT_MIN_DELAY = 10f;

	private const float ATTRACT_MAX_DELAY = 45f;

	private const float ATTRACT_PREP_TIME = 1.5f;

	private const float ATTRACT_RECOVERY_TIME = 5f;

	private const float LOW_GRAV_TIME = 10f;

	private const float LOW_GRAV_FACTOR = 0.2f;

	private const float LOW_GRAV_RAD = 2f;

	private const float MAGNETIC_MASS_FACTOR = 100f;

	private int attracteeMask;

	private State state;

	public override void Awake()
	{
		base.Awake();
		attracteeMask = LayerMask.GetMask("Actor", "ActorIgnorePlayer");
		sfAnimator = GetComponent<SlimeFaceAnimator>();
	}

	public override void Start()
	{
		base.Start();
		nextExplodeDelayTime = AttractDelay();
		nextPossibleAttract = Time.time + nextExplodeDelayTime * Randoms.SHARED.GetInRange(0.25f, 1f);
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: false);
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!(Time.time > nextPossibleAttract))
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
		StartCoroutine(DelayedAttract());
	}

	public override void Deselected()
	{
		base.Deselected();
	}

	private float AttractDelay()
	{
		return Mathf.Lerp(10f, 45f, Mathf.Clamp(Randoms.SHARED.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f));
	}

	private IEnumerator DelayedAttract()
	{
		state = State.PREPARING;
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: true);
		sfAnimator.SetTrigger("triggerGrimace");
		float originalMass = slimeBody.mass;
		slimeBody.mass *= 100f;
		yield return new WaitForSeconds(1.5f);
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: false);
		state = State.ATTRACTING;
		FindAttractees();
		SRBehaviour.SpawnAndPlayFX(attractFX, base.transform.position, base.transform.rotation);
		nextExplodeDelayTime = AttractDelay();
		nextPossibleAttract = Time.time + nextExplodeDelayTime;
		state = State.RECOVERING;
		sfAnimator.SetTrigger("triggerFried");
		nextRecoverTime = Time.time + 5f;
		yield return new WaitForSeconds(5f);
		attractees.Clear();
		slimeBody.mass = originalMass;
		state = State.IDLE;
	}

	public void FixedUpdate()
	{
		if (state == State.RECOVERING || state == State.ATTRACTING)
		{
			Attract();
		}
	}

	private void FindAttractees()
	{
		Vector3 position = base.transform.position;
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Collider[] array = Physics.OverlapSphere(position, attractRadius, attracteeMask, QueryTriggerInteraction.Ignore);
		foreach (Collider collider in array)
		{
			if (!(collider != null))
			{
				continue;
			}
			Rigidbody component = collider.GetComponent<Rigidbody>();
			GameObject gameObject = collider.gameObject;
			if (component != null && gameObject != base.gameObject && !hashSet.Contains(gameObject))
			{
				if (gameObject != SRSingleton<SceneContext>.Instance.Player)
				{
					attractees.Add(component);
				}
				hashSet.Add(gameObject);
			}
		}
	}

	private void Attract()
	{
		Vector3 position = base.transform.position;
		float num = attractPower * Time.fixedDeltaTime;
		for (int i = 0; i < attractees.Count; i++)
		{
			Rigidbody rigidbody = attractees[i];
			if (!(rigidbody == null))
			{
				rigidbody.AddExplosionForce(0f - num, position, attractRadius);
			}
		}
	}

	private void ApplyLowGravChargesNearby()
	{
		Vector3 position = base.transform.position;
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Collider[] array = Physics.OverlapSphere(position, 2f);
		foreach (Collider collider in array)
		{
			if (!(collider != null) || collider.isTrigger)
			{
				continue;
			}
			Rigidbody component = collider.GetComponent<Rigidbody>();
			GameObject gameObject = collider.gameObject;
			if (component != null && gameObject != base.gameObject && !hashSet.Contains(gameObject))
			{
				if (gameObject != SRSingleton<SceneContext>.Instance.Player)
				{
					gameObject.AddComponent<LowGravity>().SetLowGrav(0.2f, lowGravFX);
				}
				hashSet.Add(gameObject);
			}
		}
	}

	public override bool CanRethink()
	{
		return state == State.IDLE;
	}

	public float GetReadiness()
	{
		return 1f - Mathf.Clamp((nextPossibleAttract - Time.time) / nextExplodeDelayTime, 0f, 1f);
	}

	public float GetRecoveriness()
	{
		if (state != State.RECOVERING)
		{
			return 0f;
		}
		return Mathf.Clamp((nextRecoverTime - Time.time) / 5f, 0f, 1f);
	}
}
