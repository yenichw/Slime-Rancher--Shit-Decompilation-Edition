using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WorldStatusBar : MonoBehaviour
{
	[Tooltip("The image for the bar which we're filling up.")]
	public Image statusImage;

	[Tooltip("The text we will fill in.")]
	public Text label;

	public float minValue;

	public float maxValue = 100f;

	public float currValue = 50f;

	[Tooltip("The formatting to use to form our text.")]
	public string format;

	[Tooltip("The formatting to use to form our text when empty, or null if we should use default.")]
	public string emptyFormat;

	[Tooltip("The formatting to use to form our text when full, or null if we should use default.")]
	public string fullFormat;

	[Tooltip("The formatting to use to form our text when overflowing, or null if we should use default.")]
	public string overflowFormat;

	public bool translate;

	private float lastMinValue = float.NaN;

	private float lastMaxValue = float.NaN;

	private float lastCurrValue = float.NaN;

	private MessageBundle uiBundle;

	public Color barColor
	{
		set
		{
			if (statusImage != null)
			{
				statusImage.color = value;
			}
		}
	}

	public void Start()
	{
		OnChanged();
	}

	public void Update()
	{
		if (minValue != lastMinValue || maxValue != lastMaxValue || currValue != lastCurrValue)
		{
			OnChanged();
			lastMinValue = minValue;
			lastMaxValue = maxValue;
			lastCurrValue = currValue;
		}
	}

	private void OnChanged()
	{
		float num = (currValue - minValue) / (maxValue - minValue);
		float num2 = Mathf.Clamp01(num);
		if (label != null)
		{
			label.text = ApplyFormat((num <= 0f && !string.IsNullOrEmpty(emptyFormat)) ? emptyFormat : ((num == 1f && !string.IsNullOrEmpty(fullFormat)) ? fullFormat : ((num > 1f && !string.IsNullOrEmpty(overflowFormat)) ? overflowFormat : format)), num2);
		}
		if (statusImage != null)
		{
			statusImage.fillAmount = num2;
		}
	}

	protected virtual string ApplyFormat(string format, float pct)
	{
		format = format.Replace("{cur}", string.Concat(currValue));
		format = format.Replace("{min}", string.Concat(minValue));
		format = format.Replace("{max}", string.Concat(maxValue));
		format = format.Replace("{cur%}", $"{pct * 100f:00}");
		format = format.Replace("{cur2%}", $"{pct * 100f:00.0}");
		if (translate && Application.isPlaying)
		{
			if (uiBundle == null && SRSingleton<GameContext>.Instance != null)
			{
				uiBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
			}
			if (uiBundle != null)
			{
				format = uiBundle.Xlate(format);
			}
		}
		return format;
	}
}
