using System.Collections.Generic;
using UnityEngine;

public class SlimeSubbehaviourPlexer : RegisteredActorBehaviour, FloatingReactor, RegistryFixedUpdateable
{
	private const float RETHINK_PERIOD = 1f;

	private SlimeSubbehaviour[] subbehaviors;

	private SlimeSubbehaviour currBehavior;

	private float nextRethinkTime;

	private Vacuumable vacuumable;

	private bool isFloating;

	private float activationTime;

	private float distToGround;

	private Collider ownCollider;

	private bool lastBlocked;

	private GameObject lastBlockedTarget;

	private float nextBlockCheckTime;

	private float nextGroundCheckTime;

	private bool wasGrounded;

	private RaycastHit groundHit;

	private Rigidbody body;

	private TotemLinker totemLinker;

	private int behaviorBlockers;

	private const float BLOCKED_CHECK_INTERVAL = 1f;

	private const float GROUND_CHECK_INTERVAL = 0.25f;

	private const float DEFAULT_ACTIVATION_DELAY = 3f;

	private const float GROUNDING_FACTOR = 1.3f;

	private const float BLOCKED_FACTOR = 5f;

	public float? activationDelayOverride { get; set; }

	public override void Start()
	{
		base.Start();
		CollectSubbehaviours();
		body = GetComponent<Rigidbody>();
		vacuumable = GetComponent<Vacuumable>();
		Collider[] components = GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger)
			{
				ownCollider = collider;
				break;
			}
		}
		totemLinker = GetComponentInChildren<TotemLinker>();
		activationTime = Time.fixedTime + (activationDelayOverride.HasValue ? activationDelayOverride.Value : 3f);
	}

	public void RegisterBehaviorBlocker()
	{
		behaviorBlockers++;
	}

	public void UnregisterBehaviorBlocker()
	{
		behaviorBlockers--;
	}

	public void CollectSubbehaviours()
	{
		SlimeSubbehaviour[] components = GetComponents<SlimeSubbehaviour>();
		List<SlimeSubbehaviour> list = new List<SlimeSubbehaviour>(components);
		SlimeSubbehaviour[] array = components;
		foreach (SlimeSubbehaviour slimeSubbehaviour in array)
		{
			SlimeSubbehaviour[] array2 = components;
			foreach (SlimeSubbehaviour slimeSubbehaviour2 in array2)
			{
				if (slimeSubbehaviour.Forbids(slimeSubbehaviour2))
				{
					Destroyer.Destroy(slimeSubbehaviour2, "SlimeSubbehaviourPlexer.CollectSubbehaviours");
					list.Remove(slimeSubbehaviour2);
				}
			}
		}
		subbehaviors = list.ToArray();
	}

	public void RegistryFixedUpdate()
	{
		if (ownCollider != null)
		{
			distToGround = ownCollider.bounds.extents.y * 1.3f;
		}
		if (Time.fixedTime >= nextGroundCheckTime)
		{
			nextGroundCheckTime = Time.fixedTime + 0.25f;
			if (distToGround > 0f)
			{
				RaycastCommand command = new RaycastCommand(body.position, Vector3.down, distToGround);
				SRSingleton<GameContext>.Instance.RaycastBatcher.QueueRaycast(command, OnGroundedRaycastResultReceived);
			}
			else
			{
				wasGrounded = false;
			}
		}
		if (Time.fixedTime < activationTime)
		{
			return;
		}
		if (IsCaptive() || behaviorBlockers > 0)
		{
			if (currBehavior != null)
			{
				if (currBehavior.CanRethink())
				{
					currBehavior.Deselected();
					currBehavior = null;
				}
				else
				{
					currBehavior.Action();
				}
			}
		}
		else if (Time.fixedTime >= nextRethinkTime && (currBehavior == null || currBehavior.CanRethink()))
		{
			nextRethinkTime = Time.fixedTime + 1f;
			SlimeSubbehaviour bestBehaviour = GetBestBehaviour();
			if (bestBehaviour != null)
			{
				if (bestBehaviour != currBehavior)
				{
					if (currBehavior != null)
					{
						currBehavior.Deselected();
					}
					currBehavior = bestBehaviour;
					currBehavior.Selected();
				}
				bestBehaviour.Action();
			}
			else
			{
				if (currBehavior != null)
				{
					currBehavior.Deselected();
				}
				currBehavior = null;
			}
		}
		else if (currBehavior != null)
		{
			currBehavior.Action();
		}
	}

	private void OnGroundedRaycastResultReceived(RaycastHit result)
	{
		groundHit = result;
		wasGrounded = result.collider != null;
	}

	private SlimeSubbehaviour GetBestBehaviour()
	{
		bool isGrounded = IsGrounded();
		float num = 0.0001f;
		SlimeSubbehaviour result = null;
		if (subbehaviors != null)
		{
			SlimeSubbehaviour[] array = subbehaviors;
			foreach (SlimeSubbehaviour slimeSubbehaviour in array)
			{
				if (slimeSubbehaviour.enabled)
				{
					float num2 = slimeSubbehaviour.Relevancy(isGrounded);
					if (num2 < 0f || num2 > 1f)
					{
						Log.Error("Behavior relevancy outside of correct range.", "relevancy", num2, "behavior", slimeSubbehaviour.name);
					}
					if (num2 > num)
					{
						num = num2;
						result = slimeSubbehaviour;
					}
				}
			}
		}
		return result;
	}

	public bool IsCaptive()
	{
		if (vacuumable != null)
		{
			if (!vacuumable.isCaptive())
			{
				return vacuumable.isHeld();
			}
			return true;
		}
		return false;
	}

	public void ForceRethink()
	{
		nextRethinkTime = float.NegativeInfinity;
		if (currBehavior != null)
		{
			currBehavior.Deselected();
			currBehavior = null;
		}
	}

	public bool IsFloating()
	{
		return isFloating;
	}

	public void SetIsFloating(bool isFloating)
	{
		this.isFloating = isFloating;
	}

	public bool IsGrounded()
	{
		if (IsFloating())
		{
			groundHit.normal = Vector3.up;
			return true;
		}
		return wasGrounded;
	}

	public RaycastHit GroundHit()
	{
		return groundHit;
	}

	public bool IsTotemed()
	{
		if (totemLinker != null)
		{
			return totemLinker.IsLinkedFrom();
		}
		return false;
	}

	public bool IsNearGrounded(float dist)
	{
		if (body != null)
		{
			return Physics.Raycast(body.position, Vector3.down, distToGround + dist);
		}
		return false;
	}

	public bool IsBlocked(GameObject obj, int layersToIgnore, bool forceCheckFullDist)
	{
		Vector3 direction = ((obj == null) ? base.transform.forward : (SlimeSubbehaviour.GetGotoPos(obj) - base.transform.position));
		return IsBlocked(obj, direction, layersToIgnore, forceCheckFullDist);
	}

	public bool IsBlocked(GameObject obj, Vector3 direction, int layersToIgnore, bool forceCheckFullDist)
	{
		if (forceCheckFullDist || ((Time.time > nextBlockCheckTime || lastBlockedTarget != obj) && distToGround > 0f))
		{
			direction.y = 0f;
			float radius = distToGround * 0.05f;
			float num;
			if (obj != null)
			{
				num = Vector3.Distance(base.transform.position, obj.transform.position);
				if (!forceCheckFullDist)
				{
					num = Mathf.Min(distToGround * 5f, num);
				}
			}
			else
			{
				num = distToGround * 5f;
			}
			Physics.SphereCast(body.position, radius, direction, out var hitInfo, num, ~layersToIgnore);
			lastBlocked = hitInfo.collider != null && (obj == null || hitInfo.collider.gameObject != obj);
			lastBlockedTarget = obj;
			nextBlockCheckTime = Time.time + 1f;
		}
		return lastBlocked;
	}
}
