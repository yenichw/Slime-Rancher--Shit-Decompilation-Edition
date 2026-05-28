using UnityEngine;

public class VortexAdder : MonoBehaviour
{
	public ActorVortexer vortexer;

	public void OnTriggerEnter(Collider col)
	{
		if (CanAdd(col.gameObject))
		{
			vortexer.Connect(col.gameObject);
		}
	}

	protected virtual bool CanAdd(GameObject gameObj)
	{
		Vacuumable component = gameObj.GetComponent<Vacuumable>();
		if (component != null && !component.isCaptive())
		{
			return component.canCapture();
		}
		return false;
	}
}
