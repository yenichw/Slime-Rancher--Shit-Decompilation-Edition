using UnityEngine;

public class RancherProgressAwarder : SRBehaviour, ExchangeDirector.Awarder
{
	public ExchangeDirector.OfferType offerType;

	public ProgressDirector.ProgressType progressType;

	public GameObject awardFX;

	public Transform awardAt;

	public void AwardIfType(ExchangeDirector.OfferType offerType)
	{
		if (this.offerType == offerType)
		{
			DoAward();
		}
	}

	public void DoAward()
	{
		ProgressDirector progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
		ExchangeDirector exchangeDirector = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		progressDirector.AddProgress(progressType);
		exchangeDirector.RewardsDidSpawn(offerType);
		SRBehaviour.InstantiateDynamic(awardFX, awardAt.position, awardAt.rotation);
	}
}
