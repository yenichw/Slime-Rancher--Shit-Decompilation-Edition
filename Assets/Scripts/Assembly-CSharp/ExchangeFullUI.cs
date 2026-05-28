using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeFullUI : BaseUI
{
	[Tooltip("The individual request UI elements we are managing.")]
	public ExchangeItemEntryUI[] requestItems;

	[Tooltip("the individual reward UI elements we are managing.")]
	public ExchangeItemEntryUI[] rewardItems;

	[Tooltip("The panel we will enable when we have no offer.")]
	public GameObject noRequestPanel;

	[Tooltip("The panel we will enable when we have an offer.")]
	public GameObject mainOfferPanel;

	[Tooltip("The text which shows the Rancher's name.")]
	public TMP_Text rancherText;

	[Tooltip("The image which shows the Rancher's face.")]
	public Image rancherImg;

	[Tooltip("The flavor text which goes with the offer.")]
	public TMP_Text flavorText;

	private ExchangeDirector exchangeDir;

	private MessageBundle exchangeBundle;

	public override void Awake()
	{
		base.Awake();
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		ExchangeDirector exchangeDirector = exchangeDir;
		exchangeDirector.onOfferChanged = (ExchangeDirector.OnOfferChanged)Delegate.Combine(exchangeDirector.onOfferChanged, new ExchangeDirector.OnOfferChanged(OnOfferChanged));
		exchangeBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("exchange");
	}

	public void Start()
	{
		OnOfferChanged();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		ExchangeDirector exchangeDirector = exchangeDir;
		exchangeDirector.onOfferChanged = (ExchangeDirector.OnOfferChanged)Delegate.Remove(exchangeDirector.onOfferChanged, new ExchangeDirector.OnOfferChanged(OnOfferChanged));
	}

	public void OnOfferChanged()
	{
		List<ExchangeDirector.RequestedItemEntry> offerRequests = exchangeDir.GetOfferRequests(ExchangeDirector.OfferType.GENERAL);
		List<ExchangeDirector.ItemEntry> offerRewards = exchangeDir.GetOfferRewards(ExchangeDirector.OfferType.GENERAL);
		if (offerRequests == null || offerRewards == null)
		{
			noRequestPanel.SetActive(value: true);
			mainOfferPanel.SetActive(value: false);
			for (int i = 0; i < requestItems.Length; i++)
			{
				requestItems[i].SetEntry(null);
			}
			for (int j = 0; j < rewardItems.Length; j++)
			{
				rewardItems[j].SetEntry(null);
			}
		}
		else
		{
			noRequestPanel.SetActive(value: false);
			mainOfferPanel.SetActive(value: true);
			for (int k = 0; k < requestItems.Length; k++)
			{
				requestItems[k].SetEntry((offerRequests.Count > k) ? offerRequests[k] : null);
			}
			for (int l = 0; l < rewardItems.Length; l++)
			{
				rewardItems[l].SetEntry((offerRewards.Count > l) ? offerRewards[l] : null);
			}
		}
		string offerRancherId = exchangeDir.GetOfferRancherId(ExchangeDirector.OfferType.GENERAL);
		string offerId = exchangeDir.GetOfferId(ExchangeDirector.OfferType.GENERAL);
		if (offerId != null && offerRancherId != null)
		{
			rancherText.text = exchangeBundle.Get("m.rancher." + offerRancherId);
			flavorText.text = exchangeBundle.Get(offerId);
			Sprite rancherImage = GetRancherImage(offerRancherId);
			if (rancherImage != null)
			{
				rancherImg.sprite = rancherImage;
			}
		}
		else
		{
			rancherText.text = "";
			flavorText.text = "";
		}
	}

	private Sprite GetRancherImage(string rancherId)
	{
		return Resources.Load("Exchange/Ranchers/" + rancherId, typeof(Sprite)) as Sprite;
	}
}
