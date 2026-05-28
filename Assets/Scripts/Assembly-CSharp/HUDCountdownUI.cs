using TMPro;
using UnityEngine;

public class HUDCountdownUI : MonoBehaviour
{
	[Tooltip("Countdown text.")]
	public TMP_Text text;

	private TimeDirector timeDirector;

	private double time;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		base.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		int num = Mathf.CeilToInt((float)(time - timeDirector.WorldTime()) % 3600f / 60f);
		text.text = $"{num}";
		base.gameObject.SetActive(num >= 0);
	}

	public void SetCountdownTime(double minutes)
	{
		time = timeDirector.HoursFromNow((float)minutes * (1f / 60f));
		base.gameObject.SetActive(value: true);
	}
}
