using System.Collections.Generic;
using UnityEngine;

public class TentacleGrapple : FindConsumable
{
	[Tooltip("The prefab to form the grappling tentacle.")]
	public GameObject tentaclePrefab;

	[Tooltip("Time between tentacles, in seconds.")]
	public float cooldown = 2f;

	[Tooltip("Whether being grappled should cause fear.")]
	public bool causeFear = true;

	[Tooltip("Heights above the slime to grapple from. If empty, will grapple from slime center")]
	public float[] heightsAboveToGrapple;

	[Tooltip("Whether to add slimes to our search list, with a constant drive")]
	public bool addSlimesToSearchList;

	[Tooltip("Whether we should ignore those currently grappling something else.")]
	public bool ignoreGrapplers;

	[Tooltip("Should we only do this behavior when grounded?")]
	public bool groundedOnly;

	private GameObject target;

	private bool grappling;

	private GameObject activeTentacle;

	private float hookTimeout;

	private const float HOOK_TIMEOUT = 10f;

	private float nextHookTime;

	private GameObject playerObj;

	private const float EXTRA_SLIME_SEARCH_DRIVE = 0.2f;

	public override void Awake()
	{
		base.Awake();
		nextHookTime = Time.time + cooldown;
	}

	public override void Start()
	{
		base.Start();
		playerObj = SRSingleton<SceneContext>.Instance.Player;
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		Dictionary<Identifiable.Id, DriveCalculator> dictionary = base.GetSearchIds();
		if (addSlimesToSearchList)
		{
			foreach (Identifiable.Id item in Identifiable.SLIME_CLASS)
			{
				dictionary[item] = new DriveCalculator(SlimeEmotions.Emotion.NONE, -0.8f, 0f);
			}
			foreach (Identifiable.Id item2 in Identifiable.LARGO_CLASS)
			{
				dictionary[item2] = new DriveCalculator(SlimeEmotions.Emotion.NONE, -0.8f, 0f);
			}
		}
		return dictionary;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (Time.time < nextHookTime || IsCaptive())
		{
			return 0f;
		}
		if (groundedOnly && !isGrounded)
		{
			return 0f;
		}
		target = FindNearestConsumable(out var drive);
		if (ignoreGrapplers)
		{
			TentacleGrapple component = target.GetComponent<TentacleGrapple>();
			if (component != null && component.activeTentacle != null)
			{
				return 0f;
			}
		}
		if (target == playerObj)
		{
			return 0f;
		}
		if (!(target == null))
		{
			return drive * drive * 0.95f;
		}
		return 0f;
	}

	public override void Action()
	{
		if (activeTentacle == null)
		{
			grappling = false;
			plexer.ForceRethink();
		}
		else if (Time.time >= hookTimeout)
		{
			Destroyer.Destroy(activeTentacle, "TentacleGrapple.Action");
			grappling = false;
		}
		else if (target != null && IsGrounded())
		{
			Vector3 normalized = (SlimeSubbehaviour.GetGotoPos(target) - base.transform.position).normalized;
			RotateTowards(normalized);
		}
	}

	public override void Selected()
	{
		if (target != null && MaybeGrapple(target))
		{
			grappling = true;
		}
	}

	public override void Deselected()
	{
		base.Deselected();
		nextHookTime = Time.time + cooldown;
	}

	public override bool CanRethink()
	{
		return !grappling;
	}

	public bool IsGrappling(GameObject target)
	{
		if (grappling)
		{
			return target == this.target;
		}
		return false;
	}

	private bool MaybeGrapple(GameObject target)
	{
		RaycastHit hitInfo = default(RaycastHit);
		float intermediateHeight = 0f;
		float[] array = heightsAboveToGrapple;
		if (array == null || array.Length == 0)
		{
			array = new float[1];
		}
		float[] array2 = array;
		foreach (float num in array2)
		{
			Vector3 vector = base.transform.position + Vector3.up * num;
			Vector3 direction = SlimeSubbehaviour.GetGotoPos(target) - vector;
			Physics.Raycast(vector, direction, out hitInfo, direction.magnitude);
			if (hitInfo.collider != null && hitInfo.collider.gameObject == target)
			{
				intermediateHeight = num;
				break;
			}
		}
		if (hitInfo.collider == null || hitInfo.collider.gameObject != target)
		{
			return false;
		}
		if (TentacleHook.IsAlreadyHooked(hitInfo.collider.gameObject))
		{
			return false;
		}
		GameObject gameObject = Object.Instantiate(tentaclePrefab);
		Attachment component = gameObject.GetComponent<Attachment>();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		component.Init(base.gameObject, target, hitInfo.point, causeFear, intermediateHeight);
		activeTentacle = gameObject;
		hookTimeout = Time.time + 10f;
		return true;
	}

	public void Release()
	{
		if (activeTentacle != null)
		{
			Destroyer.Destroy(activeTentacle, "TentacleGrapple.Release");
		}
	}
}
