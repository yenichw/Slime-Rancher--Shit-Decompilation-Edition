using UnityEngine;

public class MosaicAttractor : Attractor
{
	public void Start()
	{
		SetAweFactor(1f);
	}

	public override void OnTriggerEnter(Collider col)
	{
		if (col.GetComponentInChildren<MosaicAttractor>() == null)
		{
			base.OnTriggerEnter(col);
		}
	}

	public override bool CauseMoveTowards()
	{
		return true;
	}
}
