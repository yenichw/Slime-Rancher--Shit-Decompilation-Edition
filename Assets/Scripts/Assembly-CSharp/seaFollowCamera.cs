using UnityEngine;

public class seaFollowCamera : MonoBehaviour
{
	private Rigidbody seaRigidbody;

	private Camera mainCamera;

	public void Awake()
	{
		seaRigidbody = GetComponent<Rigidbody>();
	}

	public void Start()
	{
		mainCamera = Camera.main;
	}

	private void Update()
	{
		Vector3 position = mainCamera.transform.position;
		Vector3 position2 = new Vector3(position.x, base.transform.position.y, position.z);
		seaRigidbody.MovePosition(position2);
	}
}
