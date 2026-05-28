using TMPro;
using UnityEngine;

public class TimeLimitV2IntroUI : MonoBehaviour
{
	[Tooltip("Countdown text.")]
	public TMP_Text text;

	[Tooltip("Duration, in real-time seconds, to countdown.")]
	public float countdown = 3f;

	private float time;

	public void Awake()
	{
		time = Time.unscaledTime + 3f;
	}

	public void Update()
	{
		text.text = $"{Mathf.CeilToInt(time - Time.unscaledTime)}";
		if (Time.unscaledTime >= time)
		{
			Destroyer.Destroy(base.gameObject, "TimeLimitV2IntroUI.Update");
		}
	}
}
