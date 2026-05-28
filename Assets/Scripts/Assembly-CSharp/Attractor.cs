using UnityEngine;

public class Attractor : SRBehaviour
{
	private float aweFactor;

	public virtual void OnTriggerEnter(Collider col)
	{
		AweTowardsAttractors component = col.GetComponent<AweTowardsAttractors>();
		if (component != null)
		{
			component.RegisterAttractor(this);
		}
	}

	public virtual void OnTriggerExit(Collider col)
	{
		AweTowardsAttractors component = col.GetComponent<AweTowardsAttractors>();
		if (component != null)
		{
			component.UnregisterAttractor(this);
		}
	}

	public void SetAweFactor(float aweFactor)
	{
		this.aweFactor = aweFactor;
	}

	public virtual float AweFactor(GameObject slime)
	{
		if (!base.isActiveAndEnabled)
		{
			return 0f;
		}
		return aweFactor;
	}

	public virtual bool CauseMoveTowards()
	{
		return false;
	}
}
