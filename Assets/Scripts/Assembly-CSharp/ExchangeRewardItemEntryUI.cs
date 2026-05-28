using UnityEngine;
using UnityEngine.UI;

public class ExchangeRewardItemEntryUI : MonoBehaviour
{
	public Image icon;

	public Text amountText;

	private LookupDirector lookupDir;

	private ExchangeDirector exchangeDir;

	public void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
	}

	public void SetEntry(ExchangeDirector.ItemEntry entry)
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
			amountText.text = GetCountDisplayForReward(entry.specReward);
		}
		else
		{
			icon.sprite = lookupDir.GetIcon(entry.id);
			amountText.text = entry.count.ToString();
		}
	}

	private string GetCountDisplayForReward(ExchangeDirector.NonIdentReward specReward)
	{
		switch (specReward)
		{
		case ExchangeDirector.NonIdentReward.NEWBUCKS_SMALL:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MEDIUM:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_LARGE:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_HUGE:
		case ExchangeDirector.NonIdentReward.NEWBUCKS_MOCHI:
			return ExchangeBreakOnImpact.GetNewbucksRewardValue(specReward).ToString();
		case ExchangeDirector.NonIdentReward.TIME_EXTENSION_12H:
		{
			int value = Mathf.FloorToInt((float)ExchangeBreakOnImpact.GetTimeExtensionRewardValue(specReward) * 60f);
			return SRSingleton<SceneContext>.Instance.TimeDirector.FormatTimeMinutes(value);
		}
		default:
			return "1";
		}
	}
}
