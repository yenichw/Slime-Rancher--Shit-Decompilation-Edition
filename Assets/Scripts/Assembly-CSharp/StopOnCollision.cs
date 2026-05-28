using UnityEngine;

public class StopOnCollision : CollidableActorBehaviour, Collidable
{
	public float distFromCol = 0.25f;

	public void ProcessCollisionEnter(Collision col)
	{
		Vector3 position = col.contacts[0].point + col.contacts[0].normal * distFromCol;
		GetComponent<Rigidbody>().position = position;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}

	public void ProcessCollisionExit(Collision col)
	{
	}
}
