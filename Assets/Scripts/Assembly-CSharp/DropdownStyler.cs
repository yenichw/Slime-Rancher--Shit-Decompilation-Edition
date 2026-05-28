using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Dropdown))]
public class DropdownStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.DropdownStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Dropdown dropdown;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		dropdown = GetComponent<Dropdown>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.DropdownStyle dropdownStyle = styleDir.GetDropdownStyle(styleName);
		if (dropdownStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown dropdown style: " + styleName);
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
			TextStyler.ApplyTextStyle(item, dropdownStyle);
		}
		if (dropdownStyle.bgSprite.apply)
		{
			dropdown.image.enabled = dropdownStyle.bgSprite.value != null;
		}
		if (dropdownStyle.bgColor.apply)
		{
			dropdown.image.color = dropdownStyle.bgColor.value;
		}
		ColorBlock colors = dropdown.colors;
		if (dropdownStyle.normalTint.apply)
		{
			colors.normalColor = dropdownStyle.normalTint.value;
		}
		if (dropdownStyle.highlightedTint.apply)
		{
			colors.highlightedColor = dropdownStyle.highlightedTint.value;
		}
		if (dropdownStyle.pressedTint.apply)
		{
			colors.pressedColor = dropdownStyle.pressedTint.value;
		}
		if (dropdownStyle.disabledTint.apply)
		{
			colors.disabledColor = dropdownStyle.disabledTint.value;
		}
		dropdown.colors = colors;
		if (dropdownStyle.bgSprite.apply)
		{
			dropdown.image.sprite = dropdownStyle.bgSprite.value;
		}
		Image component = dropdown.template.GetComponent<Image>();
		if (dropdownStyle.menuBgSprite.apply && !(component.enabled = dropdownStyle.menuBgSprite.value != null))
		{
			component.sprite = dropdownStyle.menuBgSprite.value;
		}
		if (dropdownStyle.menuBgColor.apply)
		{
			component.color = dropdownStyle.menuBgColor.value;
		}
		Toggle obj = component.GetComponentsInChildren<Toggle>(includeInactive: true)[0];
		Image image = (Image)obj.targetGraphic;
		if (dropdownStyle.itemBgSprite.apply && !(image.enabled = dropdownStyle.itemBgSprite.value != null))
		{
			image.sprite = dropdownStyle.itemBgSprite.value;
		}
		if (dropdownStyle.itemBgColor.apply)
		{
			image.color = dropdownStyle.itemBgColor.value;
		}
		ColorBlock colors2 = obj.colors;
		if (dropdownStyle.itemNormalTint.apply)
		{
			colors2.normalColor = dropdownStyle.itemNormalTint.value;
		}
		if (dropdownStyle.itemHighlightedTint.apply)
		{
			colors2.highlightedColor = dropdownStyle.itemHighlightedTint.value;
		}
		if (dropdownStyle.itemPressedTint.apply)
		{
			colors2.pressedColor = dropdownStyle.itemPressedTint.value;
		}
		if (dropdownStyle.itemDisabledTint.apply)
		{
			colors2.disabledColor = dropdownStyle.itemDisabledTint.value;
		}
		obj.colors = colors2;
	}
}
