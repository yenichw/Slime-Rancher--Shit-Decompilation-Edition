using UnityEngine;

public class SlimeDragUpdater : RegisteredActorBehaviour, RegistryFixedUpdateable
{
	private static float DRAG_VAC = 0f;

	private static float DRAG_NORM = 0.5f;

	private Vacuumable vacuumable;

	private Rigidbody body;

	public void Awake()
	{
		vacuumable = GetComponent<Vacuumable>();
		body = GetComponent<Rigidbody>();
	}

	public void RegistryFixedUpdate()
	{
		if (vacuumable != null && vacuumable.isCaptive() && body != null)
		{
			body.drag = DRAG_VAC;
		}
		else if (body != null)
		{
			body.drag = DRAG_NORM;
		}
	}
}
