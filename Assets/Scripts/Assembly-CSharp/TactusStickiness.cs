using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TactusStickiness : MonoBehaviour
{
	public class JointHelper : MonoBehaviour
	{
		private TactusStickiness stickiness;

		private GameObject stuckObj;

		private Joint joint;

		private double expiration;

		private SlimeSubbehaviourPlexer plexer;

		private Collider[] stuckColliders;

		public void SetTactusStickiness(TactusStickiness stickiness, GameObject stuckObj, double expiration)
		{
			this.stickiness = stickiness;
			this.stuckObj = stuckObj;
			joint = GetComponent<Joint>();
			this.expiration = expiration;
			plexer = stuckObj.GetComponent<SlimeSubbehaviourPlexer>();
			_ = plexer != null;
			stuckColliders = stuckObj.GetComponents<Collider>();
			Collider[] array = stuckColliders;
			for (int i = 0; i < array.Length; i++)
			{
				Physics.IgnoreCollision(array[i], stickiness.GetComponent<Collider>(), ignore: true);
			}
			StartCoroutine(DelayedSetJointBreakForce());
		}

		public void OnDestroy()
		{
			_ = plexer != null;
			Collider[] array = stuckColliders;
			foreach (Collider collider in array)
			{
				if (collider != null)
				{
					Physics.IgnoreCollision(collider, stickiness.GetComponent<Collider>(), ignore: false);
				}
			}
		}

		public void OnJointBreak(float force)
		{
			stickiness.ReportBrokenJoint(stuckObj.GetInstanceID());
			Destroyer.Destroy(base.gameObject, "TactusStickiness.OnJointBreak");
		}

		public void Update()
		{
			if (stickiness.timeDir.HasReached(expiration))
			{
				stickiness.ReportBrokenJoint(stuckObj.GetInstanceID());
				Destroyer.Destroy(base.gameObject, "TactusStickiness.Update");
			}
		}

		public IEnumerator DelayedSetJointBreakForce()
		{
			yield return new WaitForSeconds(1f);
			if (joint != null)
			{
				joint.breakForce = stickiness.jointBreakForce;
				joint.breakTorque = stickiness.jointBreakTorque;
			}
		}
	}

	public float jointBreakForce = 10f;

	public float jointBreakTorque = float.PositiveInfinity;

	public float jointTTLMins = 10f;

	private Dictionary<int, double> ineligibleGameObjIds = new Dictionary<int, double>();

	private TimeDirector timeDir;

	private WaitForChargeup waiter;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		waiter = GetComponentInParent<WaitForChargeup>();
	}

	public void OnCollisionEnter(Collision col)
	{
		if ((!(waiter != null) || !waiter.IsWaiting()) && !ineligibleGameObjIds.ContainsKey(col.gameObject.GetInstanceID()))
		{
			ineligibleGameObjIds[col.gameObject.GetInstanceID()] = double.PositiveInfinity;
			CreateJointObject(col.gameObject);
		}
	}

	public void Update()
	{
		List<int> list = null;
		foreach (KeyValuePair<int, double> ineligibleGameObjId in ineligibleGameObjIds)
		{
			if (timeDir.HasReached(ineligibleGameObjId.Value))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(ineligibleGameObjId.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (int item in list)
		{
			ineligibleGameObjIds.Remove(item);
		}
	}

	private void ReportBrokenJoint(int objID)
	{
		ineligibleGameObjIds[objID] = timeDir.HoursFromNowOrStart(1f / 60f);
	}

	private void CreateJointObject(GameObject stuckObj)
	{
		GameObject obj = new GameObject("Joint");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.transform.position = stuckObj.transform.position;
		obj.transform.rotation = stuckObj.transform.rotation;
		obj.AddComponent<Rigidbody>().isKinematic = true;
		FixedJoint joint = obj.AddComponent<FixedJoint>();
		SafeJointReference.AttachSafely(stuckObj, joint);
		obj.AddComponent<JointHelper>().SetTactusStickiness(this, stuckObj, timeDir.HoursFromNowOrStart(jointTTLMins * (1f / 60f)));
	}
}
