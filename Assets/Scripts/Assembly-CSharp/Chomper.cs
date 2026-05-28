using System.Linq;
using UnityEngine;

public class Chomper : SRBehaviour
{
	public delegate void OnChompStartDelegate();

	public delegate void OnChompCompleteDelegate(GameObject chomped, Identifiable.Id chompedId, bool whileHeld, bool wasLaunched);

	private class Metadata
	{
		public OnChompCompleteDelegate onComplete;

		public GameObject gameObject;

		public Identifiable.Id id;

		public bool isHeld;

		public bool isLaunched;

		public bool isQuickChomp;

		public FixedJoint joint;
	}

	[Tooltip("Time per attack.")]
	public float timePerAttack = 3f;

	private float nextChompTime;

	private SlimeFaceAnimator faceAnim;

	private Animator bodyAnim;

	private int animQuickBiteId;

	private int animBiteId;

	private Metadata metadata;

	public void Awake()
	{
		faceAnim = GetComponent<SlimeFaceAnimator>();
		bodyAnim = GetComponentInChildren<Animator>();
		animBiteId = Animator.StringToHash("Bite");
		animQuickBiteId = Animator.StringToHash("QuickBite");
	}

	public void BiteComplete()
	{
		if (metadata != null)
		{
			metadata?.onComplete(metadata.gameObject, metadata.id, metadata.isHeld, metadata.isLaunched);
			DestroyJoints();
			metadata = null;
		}
		ResetEatClock();
		bodyAnim.SetBool(animBiteId, value: false);
		bodyAnim.SetBool(animQuickBiteId, value: false);
	}

	public bool IsChomping()
	{
		return metadata != null;
	}

	private void OnJointBreak(float breakForce)
	{
		if (metadata != null && metadata.joint != null && metadata.joint.connectedBody == null)
		{
			ForceCancelChomp();
		}
	}

	public bool CanChomp()
	{
		if (Time.fixedTime < nextChompTime)
		{
			return false;
		}
		if (IsChomping())
		{
			return false;
		}
		return true;
	}

	public bool CancelChomp(GameObject obj)
	{
		if (metadata == null)
		{
			return false;
		}
		if (metadata.gameObject != obj)
		{
			return false;
		}
		if (metadata.isQuickChomp)
		{
			return false;
		}
		ForceCancelChomp();
		return true;
	}

	private void ForceCancelChomp()
	{
		DestroyJoints();
		metadata = null;
	}

	private void DestroyJoints()
	{
		if (metadata == null || !(metadata.gameObject != null))
		{
			return;
		}
		foreach (SafeJointReference item in GetComponents<SafeJointReference>().Concat(metadata.gameObject.GetComponents<SafeJointReference>()))
		{
			item.DestroyJoint();
		}
	}

	public void StartChomp(GameObject other, Identifiable.Id otherId, bool whileHeld, bool quick, OnChompStartDelegate onChompStart, OnChompCompleteDelegate onChompComplete)
	{
		onChompStart?.Invoke();
		metadata = new Metadata
		{
			onComplete = onChompComplete,
			gameObject = other,
			isQuickChomp = quick,
			id = otherId,
			isHeld = whileHeld,
			isLaunched = (LayerMask.NameToLayer("Launched") == other.layer)
		};
		faceAnim.SetTrigger((otherId == Identifiable.Id.PLAYER) ? "triggerAttackTelegraph" : (quick ? "triggerChompOpenQuick" : "triggerChompOpen"));
		bodyAnim.SetBool(quick ? animQuickBiteId : animBiteId, value: true);
		if (!quick)
		{
			return;
		}
		foreach (SafeJointReference item in GetComponents<SafeJointReference>().Concat(other.GetComponents<SafeJointReference>()))
		{
			item.DestroyJoint();
		}
		metadata.joint = SlimeUtil.AttachToMouth(base.gameObject, other);
	}

	public void ResetEatClock()
	{
		nextChompTime = Time.fixedTime + timePerAttack;
	}

	public void OnDisable()
	{
		ForceCancelChomp();
	}
}
