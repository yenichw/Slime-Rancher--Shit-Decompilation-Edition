using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Dropdown))]
public class MeshDropdownStyler : SRBehaviour
{
	protected class DropdownData
	{
		private Dictionary<string, MeshTextStyler.TextData> textDict;

		private string styleName;

		private RectTransform template;

		private GameObject captionTextObj;

		private Image captionImage;

		private GameObject itemTextObj;

		private Image itemImage;

		public DropdownData(Dropdown dropdown, DropdownStyler styler)
		{
			textDict = new Dictionary<string, MeshTextStyler.TextData>();
			Text[] componentsInChildren = styler.GetComponentsInChildren<Text>();
			foreach (Text text in componentsInChildren)
			{
				if (!text.GetComponent<TextStyler>() && !text.GetComponent<MeshTextStyler>())
				{
					textDict[GetPath(text.transform)] = new MeshTextStyler.TextData(text, null);
				}
			}
			styleName = styler.styleName;
			template = dropdown.template;
			captionTextObj = ((dropdown.captionText == null) ? null : dropdown.captionText.gameObject);
			captionImage = dropdown.captionImage;
			itemTextObj = ((dropdown.itemText == null) ? null : dropdown.itemText.gameObject);
			itemImage = dropdown.itemImage;
		}

		private string GetPath(Transform trans)
		{
			if (trans.parent != null)
			{
				return GetPath(trans.parent) + "/" + trans.name;
			}
			return trans.name;
		}

		public void ApplyTo(TMP_Dropdown dropdown, MeshDropdownStyler styler)
		{
			TextMeshProUGUI[] componentsInChildren = styler.GetComponentsInChildren<TextMeshProUGUI>();
			foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
			{
				if (!textMeshProUGUI.GetComponent<TextStyler>() && !textMeshProUGUI.GetComponent<MeshTextStyler>())
				{
					textDict[GetPath(textMeshProUGUI.transform)].ApplyTo(textMeshProUGUI, null);
				}
			}
			styler.styleName = styleName;
			dropdown.template = template;
			dropdown.captionText = ((captionTextObj == null) ? null : captionTextObj.GetComponent<TMP_Text>());
			dropdown.captionImage = captionImage;
			dropdown.itemText = ((itemTextObj == null) ? null : itemTextObj.GetComponent<TMP_Text>());
			dropdown.itemImage = itemImage;
		}
	}

	[StyleName(typeof(UIStyleDirector.MeshDropdownStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private TMP_Dropdown dropdown;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		dropdown = GetComponent<TMP_Dropdown>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.MeshDropdownStyle meshDropdownStyle = styleDir.GetMeshDropdownStyle(styleName);
		if (meshDropdownStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown dropdown style: " + styleName);
			}
			return;
		}
		List<TMP_Text> list = new List<TMP_Text>();
		TMP_Text[] componentsInChildren = GetComponentsInChildren<TMP_Text>();
		foreach (TMP_Text tMP_Text in componentsInChildren)
		{
			if (!tMP_Text.GetComponent<TextStyler>() && !tMP_Text.GetComponent<MeshTextStyler>())
			{
				list.Add(tMP_Text);
			}
		}
		foreach (TMP_Text item in list)
		{
			MeshTextStyler.ApplyTextStyle(item, meshDropdownStyle);
		}
		if (meshDropdownStyle.bgSprite.apply)
		{
			dropdown.image.enabled = meshDropdownStyle.bgSprite.value != null;
		}
		if (meshDropdownStyle.bgColor.apply)
		{
			dropdown.image.color = meshDropdownStyle.bgColor.value;
		}
		ColorBlock colors = dropdown.colors;
		if (meshDropdownStyle.normalTint.apply)
		{
			colors.normalColor = meshDropdownStyle.normalTint.value;
		}
		if (meshDropdownStyle.highlightedTint.apply)
		{
			colors.highlightedColor = meshDropdownStyle.highlightedTint.value;
		}
		if (meshDropdownStyle.pressedTint.apply)
		{
			colors.pressedColor = meshDropdownStyle.pressedTint.value;
		}
		if (meshDropdownStyle.disabledTint.apply)
		{
			colors.disabledColor = meshDropdownStyle.disabledTint.value;
		}
		dropdown.colors = colors;
		if (meshDropdownStyle.bgSprite.apply)
		{
			dropdown.image.sprite = meshDropdownStyle.bgSprite.value;
		}
		Image component = dropdown.template.GetComponent<Image>();
		if (meshDropdownStyle.menuBgSprite.apply && !(component.enabled = meshDropdownStyle.menuBgSprite.value != null))
		{
			component.sprite = meshDropdownStyle.menuBgSprite.value;
		}
		if (meshDropdownStyle.menuBgColor.apply)
		{
			component.color = meshDropdownStyle.menuBgColor.value;
		}
		Toggle obj = component.GetComponentsInChildren<Toggle>(includeInactive: true)[0];
		Image image = (Image)obj.targetGraphic;
		if (meshDropdownStyle.itemBgSprite.apply && !(image.enabled = meshDropdownStyle.itemBgSprite.value != null))
		{
			image.sprite = meshDropdownStyle.itemBgSprite.value;
		}
		if (meshDropdownStyle.itemBgColor.apply)
		{
			image.color = meshDropdownStyle.itemBgColor.value;
		}
		ColorBlock colors2 = obj.colors;
		if (meshDropdownStyle.itemNormalTint.apply)
		{
			colors2.normalColor = meshDropdownStyle.itemNormalTint.value;
		}
		if (meshDropdownStyle.itemHighlightedTint.apply)
		{
			colors2.highlightedColor = meshDropdownStyle.itemHighlightedTint.value;
		}
		if (meshDropdownStyle.itemPressedTint.apply)
		{
			colors2.pressedColor = meshDropdownStyle.itemPressedTint.value;
		}
		if (meshDropdownStyle.itemDisabledTint.apply)
		{
			colors2.disabledColor = meshDropdownStyle.itemDisabledTint.value;
		}
		obj.colors = colors2;
	}

	public static void Convert(GameObject obj)
	{
		DropdownStyler[] componentsInChildren = obj.GetComponentsInChildren<DropdownStyler>(includeInactive: true);
		foreach (DropdownStyler dropdownStyler in componentsInChildren)
		{
			Dropdown component = dropdownStyler.GetComponent<Dropdown>();
			DropdownData dropdownData = new DropdownData(component, dropdownStyler);
			if ((bool)component.itemText.GetComponent<TextStyler>())
			{
				MeshTextStyler.Convert(component.itemText.GetComponent<TextStyler>());
			}
			if ((bool)component.captionText.GetComponent<TextStyler>())
			{
				MeshTextStyler.Convert(component.captionText.GetComponent<TextStyler>());
			}
			GameObject gameObject = dropdownStyler.gameObject;
			InitSelected component2 = component.GetComponent<InitSelected>();
			bool flag = component2 != null;
			if (component2 != null)
			{
				Object.DestroyImmediate(component2);
			}
			Object.DestroyImmediate(dropdownStyler);
			Object.DestroyImmediate(component);
			TMP_Dropdown tMP_Dropdown = gameObject.AddComponent<TMP_Dropdown>();
			MeshDropdownStyler styler = gameObject.AddComponent<MeshDropdownStyler>();
			Text[] componentsInChildren2 = gameObject.GetComponentsInChildren<Text>();
			foreach (Text text in componentsInChildren2)
			{
				if (!text.GetComponent<TextStyler>() && !text.GetComponent<MeshTextStyler>())
				{
					GameObject obj2 = text.gameObject;
					Object.DestroyImmediate(text);
					obj2.AddComponent<TextMeshProUGUI>();
				}
			}
			dropdownData.ApplyTo(tMP_Dropdown, styler);
			if (flag)
			{
				gameObject.AddComponent<InitSelected>();
			}
		}
	}
}
