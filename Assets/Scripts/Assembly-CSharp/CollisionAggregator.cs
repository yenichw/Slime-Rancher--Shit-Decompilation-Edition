using System.Collections.Generic;
using UnityEngine;

public class CollisionAggregator : MonoBehaviour
{
	private List<Collidable> collidableBehaviours = new List<Collidable>();

	public void Register(CollidableActorBehaviour collidableBehaviour)
	{
		if (collidableBehaviour is Collidable)
		{
			Register(collidableBehaviour as Collidable);
		}
	}

	private void Register(Collidable collidableBehaviour)
	{
		if (!collidableBehaviours.Contains(collidableBehaviour))
		{
			collidableBehaviours.Add(collidableBehaviour);
		}
	}

	public void Deregister(CollidableActorBehaviour collidableBehaviour)
	{
		if (collidableBehaviour is Collidable)
		{
			Deregister(collidableBehaviour as Collidable);
		}
	}

	private void Deregister(Collidable collidableBehaviour)
	{
		collidableBehaviours.Remove(collidableBehaviour);
	}

	public void OnCollisionEnter(Collision col)
	{
		foreach (Collidable collidableBehaviour in collidableBehaviours)
		{
			collidableBehaviour?.ProcessCollisionEnter(col);
		}
	}
}
