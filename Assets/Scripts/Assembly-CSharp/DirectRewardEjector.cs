using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DirectRewardEjector : SRBehaviour, ExchangeDirector.Awarder
{
	public GameObject rewardPrefab;

	public int rewardCount = 1;

	public ExchangeDirector.OfferType offerType;

	[Tooltip("SFX played when the reward is ejected. (optional)")]
	public SECTR_AudioCue onEjectCue;

	private bool rewardIsActor;

	private const float EJECT_FORCE = 60f;

	public void Start()
	{
		rewardIsActor = rewardPrefab.GetComponent<Identifiable>() != null;
	}

	public void AwardIfType(ExchangeDirector.OfferType offerType)
	{
		if (this.offerType == offerType)
		{
			Eject();
		}
	}

	public void Eject()
	{
		RegionRegistry.RegionSetId setId = GetComponentInParent<Region>().setId;
		for (int i = 0; i < rewardCount; i++)
		{
			Rigidbody component = (rewardIsActor ? SRBehaviour.InstantiateActor(rewardPrefab, setId, base.transform.position, base.transform.rotation) : SRBehaviour.InstantiateDynamic(rewardPrefab, base.transform.position, base.transform.rotation)).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = false;
				component.AddForce(base.transform.forward * 60f);
			}
		}
		SRSingleton<SceneContext>.Instance.ExchangeDirector.RewardsDidSpawn(offerType);
		SECTR_AudioSystem.Play(onEjectCue, base.transform.position, loop: false);
	}
}
