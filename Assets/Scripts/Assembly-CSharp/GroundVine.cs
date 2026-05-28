using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GroundVine : FindConsumable
{
	private enum Phase
	{
		IDLE = 0,
		GRAB_GROW = 1,
		GRAB_SHRINK = 2,
		EAT_GROW = 3,
		EAT_SHRINK = 4
	}

	[Tooltip("Time between vines, in seconds.")]
	public float cooldown = 2f;

	[Tooltip("The audio cue for the vine coming up out of the ground.")]
	public SECTR_AudioCue vineUpCue;

	[Tooltip("The audio cue for the vine going down back into the ground.")]
	public SECTR_AudioCue vineDownCue;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private GameObject vinePrefab;

	private GameObject vineEnterFX;

	private GameObject vineExitFX;

	private GameObject target;

	private GameObject activeVine;

	private float nextVineTime;

	private GameObject playerObj;

	private int groundMask;

	private Phase phase;

	private float phaseEndTime = float.PositiveInfinity;

	private static HashSet<GameObject> allGrabbed = new HashSet<GameObject>();

	private const float TARGET_SCALE_TIME = 0.25f;

	private const float FULL_VINE_SCALE_TIME = 0.75f;

	private const float FULL_VINE_HEIGHT = 4f;

	private const float MIN_EAT_HEIGHT = 2f;

	private const float MAX_EAT_HEIGHT = 2.5f;

	private const float RELEASE_SCALE_TIME = 0.5f;

	private const float MAX_HEIGHT = 0.5f;

	private const float MIN_EXTRA_HEIGHT = 3f;

	private const float MAX_EXTRA_HEIGHT = 4f;

	public override void Awake()
	{
		base.Awake();
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += UpdateVineAppearance;
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateVineAppearance(slimeAppearanceApplicator.Appearance);
		}
		nextVineTime = Time.time + cooldown;
		groundMask = 268439553;
	}

	public override void Start()
	{
		base.Start();
		playerObj = SRSingleton<SceneContext>.Instance.Player;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (Time.time < nextVineTime || IsCaptive())
		{
			return 0f;
		}
		if (!isGrounded)
		{
			return 0f;
		}
		target = FindNearestConsumable(out var drive);
		if (target == null || target == playerObj || allGrabbed.Contains(target))
		{
			return 0f;
		}
		return drive * drive * 0.95f;
	}

	public override void Action()
	{
		if (phase == Phase.IDLE && activeVine == null && target != null)
		{
			if (!MaybeGrapple(target))
			{
				return;
			}
		}
		else if (target == null || activeVine == null)
		{
			Release();
			return;
		}
		if (!(Time.time >= phaseEndTime))
		{
			return;
		}
		if (phase == Phase.GRAB_GROW)
		{
			float num = 1f;
			float num2 = 0.75f * num;
			SECTR_AudioSystem.Play(vineDownCue, activeVine.transform.position, loop: false);
			TweenUtil.ScaleOut(activeVine, num2, Ease.InQuint);
			phase = Phase.GRAB_SHRINK;
			phaseEndTime = Time.time + num2;
		}
		else if (phase == Phase.GRAB_SHRINK)
		{
			Physics.Raycast(base.transform.position + base.transform.forward * (PhysicsUtil.RadiusOfObject(base.gameObject) + 0.5f), Vector3.down, out var hitInfo, 2f, groundMask);
			if (hitInfo.collider == null)
			{
				Release();
				return;
			}
			SRBehaviour.SpawnAndPlayFX(vineExitFX, activeVine.transform.position, Quaternion.identity);
			activeVine.transform.position = hitInfo.point;
			target.transform.position = hitInfo.point;
			SRBehaviour.SpawnAndPlayFX(vineEnterFX, activeVine.transform.position, Quaternion.identity);
			SECTR_AudioSystem.Play(vineUpCue, activeVine.transform.position, loop: false);
			float num3 = Randoms.SHARED.GetInRange(2f, 2.5f) / 4f;
			float num4 = 0.75f * num3;
			activeVine.transform.localScale = new Vector3(1f, 1f, num3);
			TweenUtil.ScaleIn(activeVine, num4, Ease.InOutCubic);
			TweenUtil.ScaleIn(target, 0.25f, Ease.Linear);
			phase = Phase.EAT_GROW;
			phaseEndTime = Time.time + num4;
			EnableTargetCollider(toEnable: true);
		}
		else if (phase == Phase.EAT_GROW)
		{
			float num5 = 0.25f;
			TweenUtil.ScaleOut(activeVine, num5, Ease.InQuint);
			phase = Phase.EAT_SHRINK;
			phaseEndTime = Time.time + num5;
			SECTR_AudioSystem.Play(vineDownCue, activeVine.transform.position, loop: false);
			Destroyer.Destroy(activeVine.GetComponentInChildren<Joint>(), "GroundVine.Action#1");
		}
		else if (phase == Phase.EAT_SHRINK)
		{
			SRBehaviour.SpawnAndPlayFX(vineExitFX, activeVine.transform.position, Quaternion.identity);
			Release();
		}
	}

	public override void Selected()
	{
		if (target != null)
		{
			MaybeGrapple(target);
		}
	}

	public override void Deselected()
	{
		base.Deselected();
		nextVineTime = Time.time + cooldown;
		Release();
	}

	public override bool CanRethink()
	{
		return phase == Phase.IDLE;
	}

	private void UpdateVineAppearance(SlimeAppearance appearance)
	{
		vinePrefab = appearance.VineAppearance.vinePrefab;
		vineEnterFX = appearance.VineAppearance.vineEnterFx;
		vineExitFX = appearance.VineAppearance.vineExitFx;
	}

	private bool MaybeGrapple(GameObject target)
	{
		if (!Physics.Raycast(target.transform.position, Vector3.down, out var hitInfo, 0.5f, groundMask))
		{
			return false;
		}
		if (!allGrabbed.Add(target))
		{
			return false;
		}
		float num = (target.transform.position.y - hitInfo.point.y + Randoms.SHARED.GetInRange(3f, 4f)) / 4f;
		float num2 = 0.75f * num;
		activeVine = SRBehaviour.InstantiateDynamic(vinePrefab, hitInfo.point, Quaternion.Euler(new Vector3(-90f, 0f, 0f)));
		activeVine.transform.localScale = new Vector3(1f, 1f, num);
		TweenUtil.ScaleIn(activeVine, num2, Ease.InOutCubic);
		SRBehaviour.SpawnAndPlayFX(vineEnterFX, activeVine.transform.position, Quaternion.identity);
		SECTR_AudioSystem.Play(vineUpCue, activeVine.transform.position, loop: false);
		phase = Phase.GRAB_GROW;
		phaseEndTime = Time.time + num2;
		Joint componentInChildren = activeVine.GetComponentInChildren<Joint>();
		target.transform.position = componentInChildren.transform.position;
		SafeJointReference.AttachSafely(target, componentInChildren);
		componentInChildren.connectedAnchor = Vector3.zero;
		EnableTargetCollider(toEnable: false);
		return true;
	}

	public void Release()
	{
		Destroyer.Destroy(activeVine, "GroundVine.Release");
		if (phase >= Phase.GRAB_GROW)
		{
			allGrabbed.Remove(target);
		}
		EnableTargetCollider(toEnable: true);
		target = null;
		activeVine = null;
		phase = Phase.IDLE;
		phaseEndTime = float.PositiveInfinity;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Release();
	}

	public void EnableTargetCollider(bool toEnable)
	{
		if (!(target != null))
		{
			return;
		}
		Collider[] components = target.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger)
			{
				collider.enabled = toEnable;
			}
		}
	}
}
