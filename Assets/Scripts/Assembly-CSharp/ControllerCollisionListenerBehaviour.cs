using UnityEngine;

public abstract class ControllerCollisionListenerBehaviour : SRBehaviour, ControllerCollisionListener
{
	private bool isControllerColliding;

	private bool wasControllerColliding;

	public void OnControllerCollision(GameObject collision)
	{
		isControllerColliding |= Predicate(collision);
	}

	public void LateUpdate()
	{
		if (wasControllerColliding != isControllerColliding)
		{
			if (isControllerColliding)
			{
				OnControllerCollisionEntered();
			}
			else
			{
				OnControllerCollisionExited();
			}
		}
		wasControllerColliding = isControllerColliding;
		isControllerColliding = false;
	}

	protected virtual void OnControllerCollisionEntered()
	{
	}

	protected virtual void OnControllerCollisionExited()
	{
	}

	protected virtual bool Predicate(GameObject collision)
	{
		return true;
	}
}
