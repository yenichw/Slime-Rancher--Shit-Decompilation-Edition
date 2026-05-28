using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeRewardUI : MonoBehaviour
{
	public Text noRequestText;

	public Text pendingRequestText;

	public ExchangeDirector.OfferType offerType;

	public ExchangeRewardItemEntryUI[] items;

	private ExchangeDirector exchangeDir;

	public SlimeAppearanceDirector slimeAppearanceDirector;

	public void Awake()
	{
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		ExchangeDirector exchangeDirector = exchangeDir;
		exchangeDirector.onOfferChanged = (ExchangeDirector.OnOfferChanged)Delegate.Combine(exchangeDirector.onOfferChanged, new ExchangeDirector.OnOfferChanged(OnOfferChanged));
	}

	public void OnEnable()
	{
		slimeAppearanceDirector.onSlimeAppearanceChanged += OnSlimeAppearanceUpdated;
		OnOfferChanged();
	}

	public void OnDisable()
	{
		slimeAppearanceDirector.onSlimeAppearanceChanged -= OnSlimeAppearanceUpdated;
	}

	public void OnSlimeAppearanceUpdated(SlimeDefinition slime, SlimeAppearance appearance)
	{
		OnOfferChanged();
	}

	public void Start()
	{
		OnOfferChanged();
	}

	public void OnDestroy()
	{
		ExchangeDirector exchangeDirector = exchangeDir;
		exchangeDirector.onOfferChanged = (ExchangeDirector.OnOfferChanged)Delegate.Remove(exchangeDirector.onOfferChanged, new ExchangeDirector.OnOfferChanged(OnOfferChanged));
	}

	public void OnOfferChanged()
	{
		List<ExchangeDirector.ItemEntry> offerRewards = exchangeDir.GetOfferRewards(offerType);
		if (offerRewards == null)
		{
			if (exchangeDir.HasPendingOffers(offerType))
			{
				noRequestText.enabled = false;
				pendingRequestText.enabled = true;
			}
			else
			{
				noRequestText.enabled = true;
				pendingRequestText.enabled = false;
			}
			for (int i = 0; i < items.Length; i++)
			{
				items[i].SetEntry(null);
			}
		}
		else
		{
			noRequestText.enabled = false;
			pendingRequestText.enabled = false;
			for (int j = 0; j < items.Length; j++)
			{
				items[j].SetEntry((offerRewards.Count > j) ? offerRewards[j] : null);
			}
		}
	}
}
