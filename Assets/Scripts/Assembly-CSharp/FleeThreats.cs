using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class FleeThreats : SlimeSubbehaviour, RegistryFixedUpdateable
{
	private class Threat
	{
		public Identifiable.Id id;

		public GameObject gameObject;
	}

	public FearProfile fearProfile;

	public SlimeEmotions.Emotion driver = SlimeEmotions.Emotion.FEAR;

	public float maxJump = 2f;

	public float facingStability = 0.2f;

	public float facingSpeed = 1f;

	private Threat threat;

	private RegionMember member;

	private TotemLinker totemLinker;

	private HashSet<TentacleHook> grapplers = new HashSet<TentacleHook>();

	private float nextLeap;

	private const float LEAP_COOLDOWN = 0.5f;

	private List<GameObject> nearbyGameObjects = new List<GameObject>();

	public override void Awake()
	{
		base.Awake();
		member = GetComponent<RegionMember>();
		totemLinker = GetComponentInChildren<TotemLinker>();
	}

	public override void Start()
	{
		base.Start();
	}

	public override float Relevancy(bool isGrounded)
	{
		threat = FindNearestThreat();
		if (threat != null)
		{
			return emotions.GetCurr(driver);
		}
		return 0f;
	}

	public void RegistryFixedUpdate()
	{
		if (threat != null && threat.gameObject != null && threat.gameObject.activeSelf)
		{
			float magnitude = (SlimeSubbehaviour.GetGotoPos(threat.gameObject) - base.transform.position).magnitude;
			emotions.Adjust(driver, fearProfile.DistToFearAdjust(threat.id, magnitude));
		}
		else if (threat != null && threat.gameObject == null)
		{
			threat = null;
		}
	}

	public override void Selected()
	{
		SlimeFaceAnimator component = GetComponent<SlimeFaceAnimator>();
		if (component != null)
		{
			component.SetTrigger("triggerAlarm");
		}
		if (totemLinker != null)
		{
			totemLinker.DisableToteming();
		}
	}

	public override void Action()
	{
		if (threat == null || !(threat.gameObject != null) || !IsGrounded() || !(Time.fixedTime >= nextLeap))
		{
			return;
		}
		Vector3 vector = -(SlimeSubbehaviour.GetGotoPos(threat.gameObject) - base.transform.position).normalized;
		RotateTowards(vector);
		if (grapplers.Count <= 0)
		{
			float curr = emotions.GetCurr(driver);
			if (IsBlocked(null))
			{
				slimeBody.AddForce((vector + Vector3.up * 5f).normalized * (3f * curr * maxJump * slimeBody.mass), ForceMode.Impulse);
			}
			else
			{
				slimeBody.AddForce((vector + Vector3.up).normalized * (curr * maxJump * slimeBody.mass), ForceMode.Impulse);
			}
			nextLeap = Time.fixedTime + 0.5f;
		}
	}

	private void RotateTowards(Vector3 dirToTarget)
	{
		Vector3 angularVelocity = slimeBody.angularVelocity;
		Vector3 vector = Vector3.Cross(Quaternion.AngleAxis(angularVelocity.magnitude * 57.29578f * facingStability / facingSpeed, angularVelocity) * base.transform.forward, dirToTarget);
		slimeBody.AddTorque(vector * (facingSpeed * facingSpeed));
	}

	private Threat FindNearestThreat()
	{
		Threat threat = null;
		Vector3 position = base.transform.position;
		float curr = emotions.GetCurr(driver);
		foreach (Identifiable.Id threateningIdentifiable in fearProfile.GetThreateningIdentifiables())
		{
			float searchRadius = fearProfile.GetSearchRadius(threateningIdentifiable, curr);
			float num = searchRadius * searchRadius;
			nearbyGameObjects.Clear();
			CellDirector.Get(threateningIdentifiable, member, nearbyGameObjects);
			for (int i = 0; i < nearbyGameObjects.Count; i++)
			{
				GameObject gameObject = nearbyGameObjects[i];
				if (!gameObject.activeInHierarchy || (threateningIdentifiable == Identifiable.Id.FIRE_COLUMN && !FireColumnIsActiveThreat(gameObject)))
				{
					continue;
				}
				float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(gameObject) - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					if (threat == null)
					{
						threat = new Threat();
					}
					threat.id = threateningIdentifiable;
					threat.gameObject = gameObject;
					num = sqrMagnitude;
				}
			}
		}
		nearbyGameObjects.Clear();
		return threat;
	}

	private bool FireColumnIsActiveThreat(GameObject potentialThreatObject)
	{
		FireColumn componentInParent = potentialThreatObject.GetComponentInParent<FireColumn>();
		if (componentInParent != null)
		{
			return componentInParent.IsFireActive();
		}
		return false;
	}

	public void AddGrappler(TentacleHook hook)
	{
		grapplers.Add(hook);
	}

	public void RemoveGrappler(TentacleHook hook)
	{
		grapplers.Remove(hook);
	}
}
