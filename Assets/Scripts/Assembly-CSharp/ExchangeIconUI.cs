using System;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeIconUI : MonoBehaviour
{
	public Image img;

	public Sprite defaultIcon;

	public Sprite pendingIcon;

	public ExchangeDirector.OfferType offerType;

	private ExchangeDirector exchangeDir;

	public void Awake()
	{
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		ExchangeDirector exchangeDirector = exchangeDir;
		exchangeDirector.onOfferChanged = (ExchangeDirector.OnOfferChanged)Delegate.Combine(exchangeDirector.onOfferChanged, new ExchangeDirector.OnOfferChanged(OnOfferChanged));
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
		string offerRancherId = exchangeDir.GetOfferRancherId(offerType);
		if (offerRancherId == null)
		{
			if (exchangeDir.HasPendingOffers(offerType) || offerType != 0)
			{
				img.sprite = pendingIcon;
			}
			else
			{
				img.sprite = defaultIcon;
			}
		}
		else
		{
			img.sprite = GetRancherImage(offerRancherId);
		}
	}

	private Sprite GetRancherImage(string rancherId)
	{
		return exchangeDir.GetRancherIcon(rancherId);
	}
}
