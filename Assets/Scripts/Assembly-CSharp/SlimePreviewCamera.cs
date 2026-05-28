using UnityEngine;

public class SlimePreviewCamera : MonoBehaviour
{
	public Camera cam;

	public Transform target;

	public Vector3 lookOffset;

	public float angleSpeed = 120f;

	public float moveSpeed = 20f;

	public float minDistance = 1.25f;

	public float maxDistance = 10f;

	public Vector3 cameraOffset;

	public bool zoomControlsEnabled = true;

	private void Update()
	{
		if (target != null && zoomControlsEnabled)
		{
			float axis = Input.GetAxis("Horizontal");
			float axis2 = Input.GetAxis("Vertical");
			cam.transform.RotateAround(target.position, Vector3.up, -1f * axis * angleSpeed * Time.deltaTime);
			Vector3 vector = axis2 * (target.position - cam.transform.position).normalized * moveSpeed * Time.deltaTime;
			float num = Vector3.Distance(target.position, cam.transform.position + vector);
			if (num >= minDistance && num <= maxDistance)
			{
				cam.transform.position += vector;
			}
			cam.transform.LookAt(target.position + lookOffset);
		}
	}

	public void ResetCamToTarget(Transform target)
	{
		this.target = target;
		cam.transform.position = target.position + cameraOffset;
		cam.transform.LookAt(target);
	}
}
