using UnityEngine;

public class EggActivator : SRBehaviour
{
	public float eggPeriod = 3f;

	public GameObject activateObj;

	private float endEgg;

	public void AddEgg()
	{
		endEgg = Time.time + eggPeriod;
		base.enabled = true;
		activateObj.SetActive(value: true);
	}

	public void Update()
	{
		if (Time.time >= endEgg)
		{
			endEgg = 0f;
			base.enabled = false;
			activateObj.SetActive(value: false);
			Destroyer.Destroy(this, "EggActivator.Update");
		}
	}
}
