using UnityEngine;

public class RotTouchedResources : SRBehaviour
{
	public void OnCollisionEnter(Collision col)
	{
		ResourceCycle component = col.gameObject.GetComponent<ResourceCycle>();
		if (component != null)
		{
			component.ImmediatelyRot();
		}
	}
}
