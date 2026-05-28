using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ExchangeEjector : SRBehaviour, ExchangeDirector.Awarder
{
	public GameObject cratePrefab;

	public ExchangeDirector.OfferType offerType;

	public GameObject awardFX;

	public Transform awardAt;

	private const float EJECT_FORCE = 100f;

	public void AwardIfType(ExchangeDirector.OfferType offerType)
	{
		if (this.offerType == offerType)
		{
			Eject();
		}
	}

	private void Eject()
	{
		RegionRegistry.RegionSetId setId = GetComponentInParent<Region>().setId;
		GameObject obj = SRBehaviour.InstantiateActor(cratePrefab, setId, base.transform.position, base.transform.rotation);
		Rigidbody component = obj.GetComponent<Rigidbody>();
		component.isKinematic = false;
		component.AddForce(base.transform.forward * 100f);
		obj.GetComponent<ExchangeBreakOnImpact>().breakOpenOnStart = false;
		SRBehaviour.InstantiateDynamic(awardFX, awardAt.position, awardAt.rotation);
	}
}
