using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class StatusBar : MonoBehaviour
{
	[Tooltip("The image for the bar which we're filling up.")]
	public Image statusImage;

	[Tooltip("The text we will fill in.")]
	public TMP_Text label;

	public float minValue;

	public float maxValue = 100f;

	public float currValue = 50f;

	[Tooltip("The formatting to use to form our text.")]
	public string format;

	[Tooltip("The formatting to use to form our text when empty, or null if we should use default.")]
	public string emptyFormat;

	[Tooltip("The formatting to use to form our text when full, or null if we should use default.")]
	public string fullFormat;

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
		if (Application.isPlaying)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
		}
	}

	public void OnDestroy()
	{
		if (Application.isPlaying && SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	private void OnBundlesAvailable(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
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
		string text = format;
		if (num == 0f && emptyFormat != null && emptyFormat != "")
		{
			text = emptyFormat;
		}
		else if (num == 1f && fullFormat != null && fullFormat != "")
		{
			text = fullFormat;
		}
		if (label != null)
		{
			label.text = ApplyFormat(text, num);
		}
		if (statusImage != null)
		{
			statusImage.fillAmount = num;
		}
	}

	protected virtual string ApplyFormat(string format, float pct)
	{
		format = format.Replace("{cur}", string.Concat(currValue));
		format = format.Replace("{min}", string.Concat(minValue));
		format = format.Replace("{max}", string.Concat(maxValue));
		format = format.Replace("{cur%}", $"{pct * 100f:00}");
		format = format.Replace("{cur2%}", $"{pct * 100f:00.0}");
		if (translate && Application.isPlaying && uiBundle != null)
		{
			format = uiBundle.Xlate(format);
		}
		return format;
	}
}
