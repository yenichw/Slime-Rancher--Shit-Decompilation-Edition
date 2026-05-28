using System;
using System.Collections;
using System.Collections.Generic;
using Noise;
using UnityEngine;

public class ActorVortexer : SRBehaviour
{
	private class Actor
	{
		public GameObject gameObject;

		public GameObject jointObj;

		public Joint joint;

		public float angleRads;

		public float height;
	}

	public GameObject spawnFX;

	public float spawnRad = 0.5f;

	public float maxRad = 5f;

	public float tornadoHeight = 45f;

	public float tornadoEccentricity = 5f;

	public float heightSpeed = 5f;

	public float heightOffset;

	public float ejectSpeed = 30f;

	public float vertEjectSpeed = 20f;

	public bool treatZAsUp = true;

	[Tooltip("Maximum number of actors we can handle at a time, or 0 for infinite.")]
	public int maxJointedActors;

	private float ejectChancePerSecond = 0.03f;

	public const float MIN_ANGULAR_SPEED = -0.1f;

	public const float MAX_ANGULAR_SPEED = -0.09f;

	private List<Actor> actors = new List<Actor>();

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void OnDestroy()
	{
		for (int num = actors.Count - 1; num >= 0; num--)
		{
			Disconnect(num, eject: false);
		}
	}

	public void FixedUpdate()
	{
		for (int num = actors.Count - 1; num >= 0; num--)
		{
			Actor actor = actors[num];
			if (!IsConnected(actor))
			{
				Disconnect(num, eject: false);
			}
			else if (Randoms.SHARED.GetProbability(ejectChancePerSecond * Time.fixedDeltaTime))
			{
				Disconnect(num, eject: true);
			}
			else
			{
				float value = global::Noise.Noise.PerlinNoise(timeDir.WorldTime(), 0f, actor.jointObj.GetInstanceID() * 1000, 500f, tornadoHeight, 1f) + heightOffset;
				float num2 = heightSpeed * Time.fixedDeltaTime;
				actor.height = Mathf.Clamp(value, actor.height - num2, actor.height + num2);
				actor.angleRads += Randoms.SHARED.GetInRange(-0.1f, -0.09f);
				float num3 = 0f;
				float num4 = 0f;
				if ((double)tornadoEccentricity != 0.0)
				{
					num3 = Mathf.Sin(actor.height * (float)Math.PI * 2f / tornadoHeight) * tornadoEccentricity;
					num4 = (0f - Mathf.Sin(actor.height * (float)Math.PI * 2f / tornadoHeight)) * tornadoEccentricity;
				}
				float num5 = Mathf.Lerp(spawnRad, maxRad, actor.height / tornadoHeight);
				if (treatZAsUp)
				{
					actor.jointObj.transform.localPosition = new Vector3(num5 * Mathf.Cos(actor.angleRads) + num3, num5 * Mathf.Sin(actor.angleRads) + num4, actor.height);
				}
				else
				{
					actor.jointObj.transform.localPosition = new Vector3(num5 * Mathf.Cos(actor.angleRads) + num3, actor.height, num5 * Mathf.Sin(actor.angleRads) + num4);
				}
				actor.jointObj.transform.eulerAngles = new Vector3(0f, actor.angleRads * 180f / (float)Math.PI, 0f);
			}
		}
	}

	public void Connect(GameObject gameObject)
	{
		if (maxJointedActors > 0 && actors.Count >= maxJointedActors)
		{
			Disconnect(Randoms.SHARED.GetInt(actors.Count), eject: true);
		}
		GameObject gameObject2 = new GameObject("Joint");
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.rotation = gameObject.transform.rotation;
		gameObject2.transform.SetParent(base.transform, worldPositionStays: true);
		float num = gameObject.transform.position.y - base.transform.position.y;
		float num2 = 0f;
		float num3 = 0f;
		if (tornadoEccentricity != 0f)
		{
			num2 = Mathf.Sin(num * (float)Math.PI * 2f / tornadoHeight) * tornadoEccentricity;
			num3 = (0f - Mathf.Sin(num * (float)Math.PI * 2f / tornadoHeight)) * tornadoEccentricity;
		}
		float angleRads = Mathf.Atan2(gameObject.transform.position.z - (base.transform.position.z + num3), gameObject.transform.position.x - (base.transform.position.x + num2));
		Connect(gameObject, gameObject2, angleRads, num);
	}

	private void Connect(GameObject gameObject, GameObject jointObj, float angleRads, float height)
	{
		FixedJoint fixedJoint = jointObj.AddComponent<FixedJoint>();
		float breakForce = (fixedJoint.breakTorque = 1000f);
		fixedJoint.breakForce = breakForce;
		jointObj.GetComponent<Rigidbody>().isKinematic = true;
		SafeJointReference.AttachSafely(gameObject, fixedJoint);
		fixedJoint.connectedAnchor = Vector3.zero;
		Vacuumable component = gameObject.GetComponent<Vacuumable>();
		component.capture(fixedJoint);
		component.SetTornadoed(tornadoed: true);
		SRBehaviour.SpawnAndPlayFX(spawnFX, jointObj.transform.position, jointObj.transform.rotation);
		actors.Add(new Actor
		{
			gameObject = gameObject,
			jointObj = jointObj,
			joint = fixedJoint,
			angleRads = angleRads,
			height = height
		});
	}

	private void Disconnect(int index, bool eject)
	{
		Actor actor = actors[index];
		actors.RemoveAt(index);
		if (actor.gameObject != null)
		{
			actor.gameObject.GetComponent<Vacuumable>().release();
			Rigidbody component = actor.gameObject.GetComponent<Rigidbody>();
			component.velocity = Vector3.zero;
			if (actor.jointObj != null && eject)
			{
				Vector3 lhs = actor.jointObj.transform.position - base.transform.position;
				lhs.y = 0f;
				Vector3 normalized = Vector3.Cross(lhs, Vector3.down).normalized;
				SRSingleton<SceneContext>.Instance.StartCoroutine(DelayedAddVelocity(component, normalized * ejectSpeed + Vector3.up * vertEjectSpeed));
			}
		}
		Destroyer.Destroy(actor.jointObj, "ActorVortexer.Eject");
	}

	private static IEnumerator DelayedAddVelocity(Rigidbody body, Vector3 velChange)
	{
		yield return new WaitForEndOfFrame();
		if (body != null)
		{
			body.AddForce(velChange, ForceMode.VelocityChange);
		}
	}

	private static bool IsConnected(Actor actor)
	{
		if (actor.gameObject != null && actor.jointObj != null)
		{
			return actor.joint != null;
		}
		return false;
	}
}
