using UnityEngine;

public interface Collidable
{
	void ProcessCollisionEnter(Collision col);

	void ProcessCollisionExit(Collision col);
}
