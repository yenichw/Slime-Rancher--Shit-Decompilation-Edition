using UnityEngine;

public class DontGoThroughThings : MonoBehaviour
{
	private Rigidbody myRigidbody;

	private bool allowDestroy;

	private const float MIN_VELOCITY = 1f;

	private const float MIN_VELOCITY_SQR = 1f;

	public void Awake()
	{
		myRigidbody = GetComponent<Rigidbody>();
		myRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}

	public void FixedUpdate()
	{
		if (allowDestroy && myRigidbody.velocity.sqrMagnitude < 1f)
		{
			myRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			Destroyer.Destroy(this, "DontGoThroughThings.FixedUpdate");
		}
	}

	public void AllowDestroy()
	{
		allowDestroy = true;
	}
}
