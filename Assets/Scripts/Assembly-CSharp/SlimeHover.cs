using UnityEngine;

public class SlimeHover : SlimeSubbehaviour, Collidable
{
	private const float HOVER_MIN_DELAY = 10f;

	private const float HOVER_MAX_DELAY = 25f;

	private float nextHoverTime;

	private float endTime;

	private Vector3 floatDir;

	private CalmedByWaterSpray calmed;

	private bool cancelHover;

	private const float HOVER_TIME = 6f;

	private const float HOVER_HEIGHT = 5f;

	private const float INV_HOVER_HEIGHT = 0.2f;

	public override void Awake()
	{
		base.Awake();
		calmed = GetComponent<CalmedByWaterSpray>();
	}

	public override void Start()
	{
		base.Start();
		cancelHover = false;
		nextHoverTime = Time.fixedTime + Randoms.SHARED.GetFloat(HoverDelay());
	}

	public override float Relevancy(bool isGrounded)
	{
		if (nextHoverTime <= Time.fixedTime)
		{
			return 0.3f;
		}
		return 0f;
	}

	public void ProcessCollisionEnter(Collision coll)
	{
		if (!(coll.rigidbody == null))
		{
			return;
		}
		ContactPoint[] contacts = coll.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			if (contactPoint.point.y > base.transform.position.y + 0.25f * base.transform.lossyScale.y)
			{
				cancelHover = true;
			}
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	public override void Selected()
	{
		endTime = Time.time + 6f;
		nextHoverTime = endTime + HoverDelay();
		floatDir = new Vector3(Randoms.SHARED.GetInRange(-1f, 1f), 0f, Randoms.SHARED.GetInRange(-1f, 1f));
	}

	public override void Deselected()
	{
		base.Deselected();
	}

	public void FixedUpdate()
	{
		if (calmed.IsCalmed())
		{
			nextHoverTime += Time.fixedDeltaTime;
		}
	}

	public override void Action()
	{
		if (!cancelHover)
		{
			if (Physics.Raycast(slimeBody.position, -Vector3.up, out var hitInfo, GetHoverHeight()))
			{
				slimeBody.AddForce(Vector3.up * (GetHoverAccel() * slimeBody.mass * Time.fixedDeltaTime) * (1f - hitInfo.distance * GetInvHoverHeight()));
			}
			slimeBody.AddForce(floatDir * (100f * slimeBody.mass * Time.fixedDeltaTime));
		}
	}

	protected virtual float GetHoverAccel()
	{
		return 1200f;
	}

	protected virtual float GetHoverHeight()
	{
		return 5f;
	}

	protected virtual float GetInvHoverHeight()
	{
		return 0.2f;
	}

	public override bool CanRethink()
	{
		if (!cancelHover)
		{
			return Time.time >= endTime;
		}
		return true;
	}

	private float HoverDelay()
	{
		return Mathf.Lerp(10f, 25f, Mathf.Clamp(Randoms.SHARED.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f));
	}
}
