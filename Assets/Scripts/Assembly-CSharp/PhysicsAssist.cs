using UnityEngine;

public class PhysicsAssist : MonoBehaviour
{
	public float assistAmount = 5f;

	public void OnCollisionEnter(Collision col)
	{
		col.rigidbody.AddForce(Vector3.down * assistAmount, ForceMode.VelocityChange);
	}
}
