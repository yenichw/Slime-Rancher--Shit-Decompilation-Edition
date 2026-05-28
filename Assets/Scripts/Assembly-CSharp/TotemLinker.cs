using UnityEngine;

public class TotemLinker : RegisteredActorBehaviour, RegistryFixedUpdateable, RegistryUpdateable
{
	[Tooltip("Probability that a totem linker will be receptive to linking another.")]
	public float receptivenessProb = 0.25f;

	[Tooltip("Minimum game hours between rethinking whether we're receptive.")]
	public float rethinkReceptivenessMin = 6f;

	[Tooltip("Maximum game hours between rethinking whether we're receptive.")]
	public float rethinkReceptivenessMax = 12f;

	[Tooltip("How much to allow gravity to do its thing on slimes while totemed.")]
	public float gravFactorWhileTotemed = 0.5f;

	private TotemLinker linkTo;

	private TotemLinker linkedFrom;

	private Joint joint;

	private SlimeEmotions emotions;

	private Vacuumable vacuumable;

	private Rigidbody body;

	private TimeDirector timeDir;

	private Vector3 antiGrav;

	private float totemActiveTime;

	private bool totemActive;

	private double rethinkReceptivenessTime;

	private bool stackReceptive;

	private bool initted;

	private const float STACK_DIST = 0.8f;

	private const float HALF_STACK_DIST = 0.4f;

	private const float AGITATION_BREAK = 0.5f;

	private const float DELAY = 1f;

	private const int CIRCULAR_LINK_STEPS = 20;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		emotions = GetComponentInParent<SlimeEmotions>();
		vacuumable = GetComponentInParent<Vacuumable>();
		body = GetComponentInParent<Rigidbody>();
	}

	public override void Start()
	{
		base.Start();
		totemActiveTime = Time.time + 1f;
		antiGrav = Physics.gravity * (-1f + gravFactorWhileTotemed);
		initted = true;
	}

	public Rigidbody JointBody()
	{
		return body;
	}

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger)
		{
			TotemLinker componentInChildren = col.GetComponentInChildren<TotemLinker>();
			if (componentInChildren != null && componentInChildren != this && CanLink() && componentInChildren.CanBeLinked() && !componentInChildren.IndirectlyLinks(this, 20))
			{
				RelinkTo(componentInChildren);
			}
		}
	}

	private bool IndirectlyLinks(TotemLinker checkLink, int checkSteps)
	{
		if (linkTo == null)
		{
			return false;
		}
		if (linkTo == checkLink)
		{
			return true;
		}
		if (checkSteps == 0)
		{
			Log.Warning("Failed to complete check for circular totem link.");
			return false;
		}
		return linkTo.IndirectlyLinks(checkLink, checkSteps - 1);
	}

	public void DisableToteming()
	{
		BreakLink();
		SetStackReceptive(receptive: false);
	}

	public void EnableToteming()
	{
		if (!stackReceptive)
		{
			rethinkReceptivenessTime = timeDir.WorldTime();
		}
	}

	protected void RelinkTo(TotemLinker totem)
	{
		if (joint != null)
		{
			Destroyer.Destroy(joint, "TotemLinker.RelinkTo");
			joint = null;
		}
		linkTo = totem;
		if (totem != null)
		{
			totem.LinkFrom(this);
			totem.SetStackReceptive(receptive: true);
			SpringJoint springJoint = JointBody().gameObject.AddComponent<SpringJoint>();
			totem.JointBody().MovePosition(base.transform.position);
			springJoint.autoConfigureConnectedAnchor = false;
			SafeJointReference.AttachSafely(totem.JointBody().gameObject, springJoint);
			springJoint.connectedAnchor = new Vector3(0f, -0.4f, 0f);
			springJoint.anchor = new Vector3(0f, 0.4f, 0f);
			springJoint.spring = 200f;
			springJoint.breakForce = 100f;
			joint = springJoint;
		}
	}

	public void RegistryUpdate()
	{
		if (linkTo != null && (joint == null || !Linkable() || !stackReceptive))
		{
			BreakLink();
		}
		if (!totemActive && Time.time >= totemActiveTime)
		{
			totemActive = true;
		}
	}

	public void UpdateEvenWhenInactive()
	{
		if (initted)
		{
			if (timeDir.HasReached(rethinkReceptivenessTime))
			{
				SetStackReceptive(Randoms.SHARED.GetProbability(receptivenessProb));
			}
			bool flag = CanLink();
			if (base.gameObject.activeSelf != flag)
			{
				base.gameObject.SetActive(flag);
			}
		}
	}

	public void RegistryFixedUpdate()
	{
		if (linkedFrom != null)
		{
			body.AddForce(antiGrav, ForceMode.Acceleration);
		}
	}

	public bool IsLinkedFrom()
	{
		return linkedFrom != null;
	}

	public void SetStackReceptive(bool receptive)
	{
		if (stackReceptive != receptive)
		{
			stackReceptive = receptive;
		}
		rethinkReceptivenessTime = timeDir.HoursFromNowOrStart(Randoms.SHARED.GetInRange(rethinkReceptivenessMin, rethinkReceptivenessMax));
	}

	private void BreakLink()
	{
		if (linkedFrom != null && linkedFrom != linkTo)
		{
			linkedFrom.RelinkTo(linkTo);
		}
		else if (linkTo != null)
		{
			linkTo.LinkFrom(null);
		}
		linkTo = null;
		linkedFrom = null;
		if (joint != null)
		{
			Destroyer.Destroy(joint, "TotemLinker.BreakLink");
			joint = null;
		}
	}

	public bool CanLink()
	{
		if (stackReceptive && linkTo == null)
		{
			return Linkable();
		}
		return false;
	}

	public bool CanBeLinked()
	{
		if (linkedFrom == null)
		{
			return Linkable();
		}
		return false;
	}

	public void LinkFrom(TotemLinker from)
	{
		linkedFrom = from;
	}

	private bool Linkable()
	{
		if (!initted)
		{
			return false;
		}
		if (vacuumable == null && emotions == null)
		{
			return false;
		}
		if (vacuumable != null && vacuumable.isCaptive())
		{
			return false;
		}
		if (emotions != null && emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) > 0.5f)
		{
			return false;
		}
		return true;
	}
}
