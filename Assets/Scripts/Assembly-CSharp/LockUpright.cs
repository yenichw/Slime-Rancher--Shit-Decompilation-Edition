using UnityEngine;

public class LockUpright : MonoBehaviour
{
	public float xOffset;

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
		base.transform.rotation = Quaternion.identity;
		base.transform.Rotate(xOffset, base.transform.rotation.y, base.transform.rotation.z);
	}
}
