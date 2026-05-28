using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Toggle))]
public class CheckboxStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.CheckboxStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Toggle toggle;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		toggle = GetComponent<Toggle>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.CheckboxStyle checkboxStyle = styleDir.GetCheckboxStyle(styleName);
		if (checkboxStyle == null)
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
			TextStyler.ApplyTextStyle(item, checkboxStyle);
		}
		if (checkboxStyle.bgSprite.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.enabled = checkboxStyle.bgSprite != null;
		}
		if (checkboxStyle.bgColor.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.color = checkboxStyle.bgColor.value;
		}
		if (checkboxStyle.bgSprite.apply && toggle.targetGraphic is Image)
		{
			((Image)toggle.targetGraphic).sprite = checkboxStyle.bgSprite.value;
		}
		if (checkboxStyle.markColor.apply && toggle.graphic != null)
		{
			toggle.graphic.color = checkboxStyle.markColor.value;
		}
		if (checkboxStyle.markSprite.apply && toggle.graphic is Image)
		{
			((Image)toggle.graphic).sprite = checkboxStyle.markSprite.value;
		}
		ColorBlock colors = toggle.colors;
		if (checkboxStyle.normalTint.apply)
		{
			colors.normalColor = checkboxStyle.normalTint.value;
		}
		if (checkboxStyle.highlightedTint.apply)
		{
			colors.highlightedColor = checkboxStyle.highlightedTint.value;
		}
		if (checkboxStyle.pressedTint.apply)
		{
			colors.pressedColor = checkboxStyle.pressedTint.value;
		}
		if (checkboxStyle.disabledTint.apply)
		{
			colors.disabledColor = checkboxStyle.disabledTint.value;
		}
		toggle.colors = colors;
	}
}
