using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	private Camera mainCamera;

	public void Awake()
	{
		FaceCamera();
	}

	public void OnRenderObject()
	{
		FaceCamera();
	}

	private void FaceCamera()
	{
		if ((object)mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		base.transform.LookAt(mainCamera.transform);
	}
}
