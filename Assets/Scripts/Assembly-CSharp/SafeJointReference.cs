using UnityEngine;

public class SafeJointReference : MonoBehaviour
{
	public Joint joint;

	private Rigidbody localRigidbody;

	private bool canDestroyJoint;

	private bool initialEnableCompleted;

	public void Awake()
	{
		localRigidbody = GetComponent<Rigidbody>();
	}

	public void OnEnable()
	{
		if (initialEnableCompleted)
		{
			if (joint != null && joint.connectedBody == localRigidbody)
			{
				joint.connectedBody = null;
				joint.connectedBody = localRigidbody;
			}
			else
			{
				Destroyer.Destroy(this, "SafeJointReference.OnEnable");
			}
		}
		else
		{
			initialEnableCompleted = true;
		}
	}

	public void Update()
	{
		if (joint == null || joint.connectedBody != localRigidbody)
		{
			Destroyer.Destroy(this, "SafeJointReference.Update");
		}
	}

	public void OnDisable()
	{
		if (initialEnableCompleted && joint == null)
		{
			Destroyer.Destroy(this, "SafeJointReference.OnDisable");
		}
	}

	public void DestroyJoint()
	{
		if (canDestroyJoint)
		{
			Destroyer.Destroy(joint, "SafeJointReference.DestroyJoint#1");
			joint = null;
		}
		Destroyer.Destroy(this, "SafeJointReference.DestroyJoint#2");
	}

	public static SafeJointReference AttachSafely(GameObject toAttach, Joint joint, bool canDestroyJoint = true)
	{
		SafeJointReference safeJointReference = toAttach.AddComponent<SafeJointReference>();
		safeJointReference.canDestroyJoint = canDestroyJoint;
		safeJointReference.joint = joint;
		joint.connectedBody = toAttach.GetComponent<Rigidbody>();
		return safeJointReference;
	}

	public void OnDrawGizmosSelected()
	{
		if (joint != null && joint.connectedBody != null)
		{
			Vector3 vector = joint.transform.TransformPoint(joint.anchor);
			Vector3 vector2 = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(vector, 0.25f);
			Gizmos.DrawWireSphere(vector2, 0.25f);
			Gizmos.DrawLine(vector, vector2);
		}
	}
}
