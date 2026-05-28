using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ToyShopUIActivator : UIActivator
{
	public GameObject ejector;

	public override GameObject Activate()
	{
		GameObject obj = base.Activate();
		ToyShopUI component = obj.GetComponent<ToyShopUI>();
		component.ejectionPoint = ejector;
		component.regionSetId = GetComponentInParent<Region>().setId;
		return obj;
	}
}
