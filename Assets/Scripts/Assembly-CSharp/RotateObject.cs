using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public float speed = 10f;

	public Vector3 axis;

	private void Update()
	{
		base.transform.Rotate(axis, speed * Time.deltaTime);
	}
}
