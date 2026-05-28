using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Toggle))]
public class ToggleButtonStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.ToggleButtonStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Toggle toggle;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		toggle = GetComponent<Toggle>();
		ApplyStyle();
	}

	public void ChangeStyle(string styleName)
	{
		this.styleName = styleName;
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.ToggleButtonStyle toggleButtonStyle = styleDir.GetToggleButtonStyle(styleName);
		if (toggleButtonStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown panel style: " + styleName);
			}
			return;
		}
		List<Text> list = new List<Text>();
		Text[] componentsInChildren = GetComponentsInChildren<Text>();
		foreach (Text text in componentsInChildren)
		{
			if (!text.GetComponent<TextStyler>())
			{
				list.Add(text);
			}
		}
		foreach (Text item in list)
		{
			TextStyler.ApplyTextStyle(item, toggleButtonStyle);
		}
		if (toggleButtonStyle.bgSprite.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.enabled = toggleButtonStyle.bgSprite != null;
		}
		if (toggleButtonStyle.bgColor.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.color = toggleButtonStyle.bgColor.value;
		}
		if (toggleButtonStyle.bgSprite.apply && toggle.targetGraphic is Image)
		{
			((Image)toggle.targetGraphic).sprite = toggleButtonStyle.bgSprite.value;
		}
		if (toggleButtonStyle.selectedColor.apply && toggle.graphic != null)
		{
			toggle.graphic.color = toggleButtonStyle.selectedColor.value;
		}
		if (toggleButtonStyle.selectedSprite.apply && toggle.graphic is Image)
		{
			((Image)toggle.graphic).sprite = toggleButtonStyle.selectedSprite.value;
		}
		ColorBlock colors = toggle.colors;
		if (toggleButtonStyle.normalTint.apply)
		{
			colors.normalColor = toggleButtonStyle.normalTint.value;
		}
		if (toggleButtonStyle.highlightedTint.apply)
		{
			colors.highlightedColor = toggleButtonStyle.highlightedTint.value;
		}
		if (toggleButtonStyle.pressedTint.apply)
		{
			colors.pressedColor = toggleButtonStyle.pressedTint.value;
		}
		if (toggleButtonStyle.disabledTint.apply)
		{
			colors.disabledColor = toggleButtonStyle.disabledTint.value;
		}
		toggle.colors = colors;
	}
}
