using UnityEngine;

public class SENBDLOrbitingCube : MonoBehaviour
{
	private Transform transf;

	private Vector3 rotationVector;

	private float rotationSpeed;

	private Vector3 spherePosition;

	private Vector3 randomSphereRotation;

	private float sphereRotationSpeed;

	private Vector3 Vec3(float x)
	{
		return new Vector3(x, x, x);
	}

	private void Start()
	{
		transf = base.transform;
		rotationVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		rotationVector = Vector3.Normalize(rotationVector);
		spherePosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		spherePosition = Vector3.Normalize(spherePosition);
		spherePosition *= Random.Range(16.5f, 18f);
		randomSphereRotation = new Vector3(Random.Range(-1.1f, 1f), Random.Range(0f, 0.1f), Random.Range(0.5f, 1f));
		randomSphereRotation = Vector3.Normalize(randomSphereRotation);
		sphereRotationSpeed = Random.Range(10f, 20f);
		rotationSpeed = Random.Range(1f, 90f);
		transf.localScale = Vec3(Random.Range(1f, 2f));
	}

	private void Update()
	{
		Quaternion quaternion = Quaternion.Euler(randomSphereRotation * Time.time * sphereRotationSpeed);
		Vector3 position = quaternion * spherePosition;
		position += spherePosition * (Mathf.Sin(Time.time - spherePosition.magnitude / 10f) * 0.5f + 0.5f);
		position += quaternion * spherePosition * (Mathf.Sin(Time.time * 3.1415265f / 4f - spherePosition.magnitude / 10f) * 0.5f + 0.5f);
		transf.position = position;
		transf.rotation = Quaternion.Euler(rotationVector * Time.time * rotationSpeed);
	}
}
