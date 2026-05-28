using UnityEngine;

public class FeederRegion : SRBehaviour
{
	public const float GROWTH_FACTOR = 2f;

	public void OnTriggerEnter(Collider collider)
	{
		TransformAfterTime component = collider.gameObject.GetComponent<TransformAfterTime>();
		if (component != null)
		{
			component.AddFeeder(this);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		TransformAfterTime component = collider.gameObject.GetComponent<TransformAfterTime>();
		if (component != null)
		{
			component.RemoveFeeder(this);
		}
	}
}
