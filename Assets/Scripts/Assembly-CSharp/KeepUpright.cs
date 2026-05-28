using UnityEngine;

public class KeepUpright : RegisteredActorBehaviour, RegistryFixedUpdateable
{
	public float stability = 0.3f;

	public float speed = 2f;

	private Rigidbody body;

	private float speedFactor;

	private float momentum;

	public override void Start()
	{
		base.Start();
		body = GetComponent<Rigidbody>();
		speedFactor = 57.29578f * stability / speed;
		momentum = speed * speed * body.mass;
	}

	public virtual void RegistryFixedUpdate()
	{
		DoUpright(Vector3.up);
	}

	protected void DoUpright(Vector3 desiredUp)
	{
		if ((object)body != null)
		{
			Vector3 angularVelocity = body.angularVelocity;
			Vector3 vector = Vector3.Cross(Quaternion.AngleAxis(angularVelocity.magnitude * speedFactor, angularVelocity) * base.transform.up, desiredUp);
			body.AddTorque(vector * momentum);
		}
	}
}
