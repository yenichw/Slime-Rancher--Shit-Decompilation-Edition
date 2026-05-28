using UnityEngine;
using UnityEngine.UI;

public class ExchangeClockUI : MonoBehaviour
{
	public Text clockText;

	public ExchangeDirector.OfferType offerType;

	private TimeDirector timeDir;

	private ExchangeDirector exchangeDir;

	private MessageBundle uiBundle;

	private int lastMins;

	private string lastTimeStr;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		uiBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
	}

	public void Update()
	{
		clockText.text = FormatTime(exchangeDir.GetOfferExpirationTime(offerType));
	}

	private string FormatTime(double? time)
	{
		if (!time.HasValue)
		{
			return uiBundle.Get("l.time_hours_mins_unset");
		}
		int num = Mathf.FloorToInt((float)time.Value / 60f);
		if (num == lastMins)
		{
			return lastTimeStr;
		}
		lastMins = num;
		lastTimeStr = timeDir.FormatTime(lastMins);
		return lastTimeStr;
	}
}
