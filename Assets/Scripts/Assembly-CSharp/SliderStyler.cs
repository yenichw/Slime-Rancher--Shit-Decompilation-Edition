using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
public class SliderStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.SliderStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Slider slider;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		slider = GetComponent<Slider>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.SliderStyle sliderStyle = styleDir.GetSliderStyle(styleName);
		if (sliderStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown slider style: " + styleName);
			}
			return;
		}
		Transform transform = slider.transform.Find("Background");
		Image image = ((transform == null) ? null : transform.GetComponent<Image>());
		if (sliderStyle.bgSprite.apply && image != null)
		{
			image.enabled = sliderStyle.bgSprite.value != null;
		}
		if (sliderStyle.bgColor.apply && image != null)
		{
			image.color = sliderStyle.bgColor.value;
		}
		if (sliderStyle.bgSprite.apply && image != null)
		{
			image.sprite = sliderStyle.bgSprite.value;
		}
		Image image2 = ((slider.handleRect == null) ? null : slider.handleRect.GetComponent<Image>());
		if (sliderStyle.handleColor.apply && image2 != null)
		{
			image2.color = sliderStyle.handleColor.value;
		}
		if (sliderStyle.handleSprite.apply && image2 != null)
		{
			image2.sprite = sliderStyle.handleSprite.value;
		}
		Image image3 = ((slider.fillRect == null) ? null : slider.fillRect.GetComponent<Image>());
		if (sliderStyle.fillColor.apply && image3 != null)
		{
			image3.color = sliderStyle.fillColor.value;
		}
		if (sliderStyle.fillSprite.apply && image3 != null)
		{
			image3.sprite = sliderStyle.fillSprite.value;
		}
		ColorBlock colors = slider.colors;
		if (sliderStyle.normalTint.apply)
		{
			colors.normalColor = sliderStyle.normalTint.value;
		}
		if (sliderStyle.highlightedTint.apply)
		{
			colors.highlightedColor = sliderStyle.highlightedTint.value;
		}
		if (sliderStyle.pressedTint.apply)
		{
			colors.pressedColor = sliderStyle.pressedTint.value;
		}
		if (sliderStyle.disabledTint.apply)
		{
			colors.disabledColor = sliderStyle.disabledTint.value;
		}
		slider.colors = colors;
	}
}
