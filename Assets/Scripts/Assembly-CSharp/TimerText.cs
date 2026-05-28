using UnityEngine;
using UnityEngine.UI;

public class TimerText : Text
{
	private int? priorMinutes;

	protected override void Start()
	{
		base.Start();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			text = SRSingleton<SceneContext>.Instance.TimeDirector.FormatTimeMinutes(null);
		}
	}

	public void UpdateTimeRemaining(double? secondsRemaining)
	{
		int? num = RoundTimeToMinutes(secondsRemaining);
		if (num != priorMinutes)
		{
			text = SRSingleton<SceneContext>.Instance.TimeDirector.FormatTimeMinutes(num);
			priorMinutes = num;
		}
	}

	private int? RoundTimeToMinutes(double? timeInSeconds)
	{
		if (!timeInSeconds.HasValue)
		{
			return null;
		}
		return Mathf.CeilToInt((float)timeInSeconds.Value * (1f / 60f));
	}
}
