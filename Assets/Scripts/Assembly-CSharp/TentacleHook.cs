using System.Collections.Generic;
using UnityEngine;

public class TentacleHook : SRBehaviour, Attachment
{
	[Tooltip("The joint connecting us to our hooked victim.")]
	public FixedJoint hookJoint;

	[Tooltip("The joint connecting us back to the tentacle-user.")]
	public SpringJoint parentJoint;

	[Tooltip("The part of ourselves that is connected to our parent.")]
	public GameObject parentEnd;

	[Tooltip("In meters per second, how quickly we reduce the length of the tentacle.")]
	public float retractSpeed = 1f;

	public SECTR_AudioCue shootCue;

	public SECTR_AudioCue grabCue;

	private const float CONVERT_TO_FIXED_DIST = 0.3f;

	private const float CONVERT_TO_FIXED_DIST_SQR = 0.09f;

	public GameObject tentacleObject;

	private Material tentacleMaterial;

	private float? fadeInTime;

	private FleeThreats targetFleeer;

	private GameObject target;

	private bool snapping;

	private float snapProgress;

	private GameObject hookEndObj;

	private bool pauseRetract;

	private static List<GameObject> allHooked = new List<GameObject>();

	private const float FADE_IN_TIME = 0.5f;

	private const float SNAP_TIME = 0.2f;

	private SafeJointReference parentSafeJoint;

	private SafeJointReference hookSafeJoint;

	public void Init(GameObject source, GameObject target, Vector3 attachPoint, bool causeFear, float intermediateHeight)
	{
		allHooked.Add(target);
		float magnitude = (attachPoint - source.transform.position).magnitude;
		parentSafeJoint = SafeJointReference.AttachSafely(source, parentJoint);
		parentJoint.minDistance = 0f;
		parentJoint.maxDistance = magnitude;
		hookSafeJoint = SafeJointReference.AttachSafely(target, hookJoint);
		hookJoint.transform.position = attachPoint;
		hookJoint.connectedAnchor = Vector3.zero;
		hookEndObj = hookJoint.gameObject;
		if (causeFear)
		{
			SlimeEmotions component = target.GetComponent<SlimeEmotions>();
			if (component != null)
			{
				component.Adjust(SlimeEmotions.Emotion.FEAR, 1f);
			}
		}
		else
		{
			SlimeFaceAnimator component2 = target.GetComponent<SlimeFaceAnimator>();
			if (component2 != null)
			{
				component2.SetTrigger("triggerLongAwe");
			}
		}
		targetFleeer = target.GetComponent<FleeThreats>();
		if (targetFleeer != null)
		{
			targetFleeer.AddGrappler(this);
		}
		this.target = target;
		AdjustConnector();
	}

	public void Awake()
	{
		tentacleMaterial = tentacleObject.GetComponent<Renderer>().material;
		tentacleMaterial.SetFloat("_Alpha", 0f);
		fadeInTime = Time.time + 0.5f;
		SECTR_AudioSystem.Play(shootCue, parentJoint.transform.position, loop: false);
	}

	public void OnDestroy()
	{
		Destroyer.Destroy(tentacleMaterial, "TentacleHook.OnDestroy");
		if (targetFleeer != null)
		{
			targetFleeer.RemoveGrappler(this);
		}
		if (target != null)
		{
			allHooked.Remove(target);
		}
		allHooked.RemoveAll((GameObject x) => x == null);
	}

	public void FixedUpdate()
	{
		if (!snapping && (hookJoint == null || parentJoint == null || hookJoint.connectedBody == null || parentJoint.connectedBody == null))
		{
			Snap();
		}
	}

	private void OnJointBreak(float breakForce)
	{
		if (hookJoint != null && hookJoint.connectedBody == null)
		{
			Destroyer.Destroy(hookJoint, "TentacleHook.OnJointBreak#1");
			hookJoint = null;
			Destroyer.Destroy(hookSafeJoint, "TentacleHook.OnJointBreak#2");
		}
		if (parentJoint != null && parentJoint.connectedBody == null)
		{
			Destroyer.Destroy(parentJoint, "TentacleHook.OnJointBreak#3");
			parentJoint = null;
			Destroyer.Destroy(parentSafeJoint, "TentacleHook.OnJointBreak#4");
		}
	}

	public void Update()
	{
		if (snapping)
		{
			snapProgress = Mathf.Min(1f, snapProgress + Time.deltaTime / 0.2f);
			float value = 1f - snapProgress;
			tentacleMaterial.SetFloat("_Alpha", value);
			if (snapProgress >= 1f)
			{
				Destroyer.Destroy(base.gameObject, "TentacleHook.Update");
			}
			return;
		}
		float time = Time.time;
		if (fadeInTime.HasValue)
		{
			if (time <= fadeInTime)
			{
				float value2 = 1f - (fadeInTime.Value - time) / 0.5f;
				tentacleMaterial.SetFloat("_Alpha", value2);
			}
			else
			{
				tentacleMaterial.SetFloat("_Alpha", 1f);
				if (target != null)
				{
					SECTR_AudioSystem.Play(grabCue, target.transform.position, loop: false);
				}
				fadeInTime = null;
			}
		}
		if (parentJoint != null && !pauseRetract)
		{
			parentJoint.maxDistance = Mathf.Max(0f, parentJoint.maxDistance - Time.deltaTime * retractSpeed);
		}
		AdjustConnector();
	}

	public void SetPauseRetract(bool pauseRetract)
	{
		this.pauseRetract = pauseRetract;
	}

	public static bool IsAlreadyHooked(GameObject obj)
	{
		return allHooked.Contains(obj);
	}

	private void AdjustConnector()
	{
		if (hookJoint != null)
		{
			Vector3 forward = hookJoint.transform.position - parentEnd.transform.position;
			if (forward.sqrMagnitude > 0f)
			{
				parentEnd.transform.forward = forward;
				hookJoint.transform.forward = forward;
			}
		}
	}

	private void Snap()
	{
		snapping = true;
		Destroyer.Destroy(hookJoint, "TentacleHook.Snap#1");
		Destroyer.Destroy(parentJoint, "TentacleHook.Snap#2");
		if (hookEndObj != null)
		{
			Rigidbody component = hookEndObj.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity = Vector3.zero;
			}
		}
	}
}
