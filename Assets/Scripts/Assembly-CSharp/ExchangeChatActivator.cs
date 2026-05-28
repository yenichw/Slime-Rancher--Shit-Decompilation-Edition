using UnityEngine;

public class ExchangeChatActivator : MonoBehaviour, TechActivator
{
	public ExchangeDirector.OfferType[] offerTypes;

	public GameObject offlineGuiPrefab;

	private ExchangeDirector exchangeDir;

	public void Awake()
	{
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
	}

	public void Activate()
	{
		ExchangeDirector.OfferType[] array = offerTypes;
		foreach (ExchangeDirector.OfferType offerType in array)
		{
			if ((offerType == ExchangeDirector.OfferType.GENERAL && exchangeDir.TryToAcceptNewOffer()) || exchangeDir.MaybeStartNext(offerType) || exchangeDir.CreateRancherChatUI(offerType, intro: false))
			{
				break;
			}
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		bool flag = true;
		ExchangeDirector.OfferType[] array = offerTypes;
		foreach (ExchangeDirector.OfferType offerType in array)
		{
			if (!exchangeDir.IsOffline(offerType))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return offlineGuiPrefab;
		}
		return null;
	}
}
