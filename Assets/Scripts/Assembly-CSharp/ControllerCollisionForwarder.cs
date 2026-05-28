using UnityEngine;

public class ControllerCollisionForwarder : SRBehaviour
{
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.gameObject != null)
		{
			Component[] components = hit.gameObject.GetComponents(typeof(ControllerCollisionListener));
			for (int i = 0; i < components.Length; i++)
			{
				((ControllerCollisionListener)components[i]).OnControllerCollision(base.gameObject);
			}
		}
	}
}
