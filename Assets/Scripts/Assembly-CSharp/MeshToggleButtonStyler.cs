using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Toggle))]
public class MeshToggleButtonStyler : SRBehaviour
{
	protected class ToggleButtonData
	{
		private Dictionary<string, MeshTextStyler.TextData> textDict;

		private string styleName;

		public ToggleButtonData(Toggle ToggleButton, ToggleButtonStyler styler)
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
		}

		private string GetPath(Transform trans)
		{
			if (trans.parent != null)
			{
				return GetPath(trans.parent) + "/" + trans.name;
			}
			return trans.name;
		}

		public void ApplyTo(Toggle ToggleButton, MeshToggleButtonStyler styler)
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
		}
	}

	[StyleName(typeof(UIStyleDirector.MeshToggleButtonStyle))]
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
		UIStyleDirector.MeshToggleButtonStyle meshToggleButtonStyle = styleDir.GetMeshToggleButtonStyle(styleName);
		if (meshToggleButtonStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown panel style: " + styleName);
			}
			return;
		}
		List<TMP_Text> list = new List<TMP_Text>();
		TMP_Text[] componentsInChildren = GetComponentsInChildren<TMP_Text>();
		foreach (TMP_Text tMP_Text in componentsInChildren)
		{
			if (!tMP_Text.GetComponent<MeshTextStyler>())
			{
				list.Add(tMP_Text);
			}
		}
		foreach (TMP_Text item in list)
		{
			MeshTextStyler.ApplyTextStyle(item, meshToggleButtonStyle);
		}
		if (meshToggleButtonStyle.bgSprite.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.enabled = meshToggleButtonStyle.bgSprite != null;
		}
		if (meshToggleButtonStyle.bgColor.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.color = meshToggleButtonStyle.bgColor.value;
		}
		if (meshToggleButtonStyle.bgSprite.apply && toggle.targetGraphic is Image)
		{
			((Image)toggle.targetGraphic).sprite = meshToggleButtonStyle.bgSprite.value;
		}
		ColorBlock colors = toggle.colors;
		if (meshToggleButtonStyle.normalTint.apply)
		{
			colors.normalColor = meshToggleButtonStyle.normalTint.value;
		}
		if (meshToggleButtonStyle.highlightedTint.apply)
		{
			colors.highlightedColor = meshToggleButtonStyle.highlightedTint.value;
		}
		if (meshToggleButtonStyle.pressedTint.apply)
		{
			colors.pressedColor = meshToggleButtonStyle.pressedTint.value;
		}
		if (meshToggleButtonStyle.disabledTint.apply)
		{
			colors.disabledColor = meshToggleButtonStyle.disabledTint.value;
		}
		toggle.colors = colors;
	}

	public static void Convert(GameObject obj)
	{
		ToggleButtonStyler[] componentsInChildren = obj.GetComponentsInChildren<ToggleButtonStyler>(includeInactive: true);
		foreach (ToggleButtonStyler toggleButtonStyler in componentsInChildren)
		{
			Toggle component = toggleButtonStyler.GetComponent<Toggle>();
			ToggleButtonData toggleButtonData = new ToggleButtonData(component, toggleButtonStyler);
			GameObject obj2 = toggleButtonStyler.gameObject;
			Object.DestroyImmediate(toggleButtonStyler);
			MeshToggleButtonStyler styler = obj2.AddComponent<MeshToggleButtonStyler>();
			Text[] componentsInChildren2 = obj2.GetComponentsInChildren<Text>();
			foreach (Text text in componentsInChildren2)
			{
				if (!text.GetComponent<TextStyler>() && !text.GetComponent<MeshTextStyler>())
				{
					GameObject obj3 = text.gameObject;
					Object.DestroyImmediate(text);
					obj3.AddComponent<TextMeshProUGUI>();
				}
			}
			toggleButtonData.ApplyTo(component, styler);
		}
	}
}
