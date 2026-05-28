using UnityEngine;
using UnityEngine.UI;

public class ExchangeProgressItemEntryUI : MonoBehaviour
{
	public Image icon;

	public Text progressText;

	private LookupDirector lookupDir;

	private ExchangeDirector exchangeDir;

	private MessageBundle uiBundle;

	public void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		uiBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
	}

	public void SetEntry(ExchangeDirector.RequestedItemEntry entry)
	{
		if (entry == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		if (entry.specReward != 0)
		{
			icon.sprite = exchangeDir.GetSpecRewardIcon(entry.specReward);
		}
		else
		{
			icon.sprite = lookupDir.GetIcon(entry.id);
		}
		progressText.text = uiBundle.Get("l.exchange_progress", entry.progress, entry.count);
	}
}
