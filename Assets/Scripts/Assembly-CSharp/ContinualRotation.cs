using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ContinualRotation : MonoBehaviour
{
	public float secsPerRotate = 10f;

	private Vector3 angularVel;

	private Rigidbody ownRigidbody;

	public void Start()
	{
		ownRigidbody = GetComponent<Rigidbody>();
		ownRigidbody.isKinematic = true;
		ownRigidbody.useGravity = false;
		angularVel = new Vector3(0f, 360f / secsPerRotate, 0f);
	}

	public void FixedUpdate()
	{
		ownRigidbody.MoveRotation(Quaternion.Euler(ownRigidbody.rotation.eulerAngles + angularVel * Time.fixedDeltaTime));
	}
}
