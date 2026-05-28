using UnityEngine;

public class reflectionFollowCamera : MonoBehaviour
{
	private float waterLevel;

	private Camera mainCamera;

	public void Awake()
	{
		waterLevel = base.transform.parent.transform.position.y;
	}

	public void Start()
	{
		mainCamera = Camera.main;
	}

	private void Update()
	{
		float num = mainCamera.transform.position.y - waterLevel;
		base.transform.position = new Vector3(base.transform.position.x, waterLevel - num, base.transform.position.z);
	}
}
