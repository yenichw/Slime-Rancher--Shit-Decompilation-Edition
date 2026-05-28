using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Toggle))]
public class MeshCheckboxStyler : SRBehaviour
{
	protected class CheckboxData
	{
		private Dictionary<string, MeshTextStyler.TextData> textDict;

		private string styleName;

		public CheckboxData(Toggle checkbox, CheckboxStyler styler)
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

		public void ApplyTo(Toggle checkbox, MeshCheckboxStyler styler)
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

	[StyleName(typeof(UIStyleDirector.MeshCheckboxStyle))]
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
		UIStyleDirector.MeshCheckboxStyle meshCheckboxStyle = styleDir.GetMeshCheckboxStyle(styleName);
		if (meshCheckboxStyle == null)
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
			MeshTextStyler.ApplyTextStyle(item, meshCheckboxStyle);
		}
		if (meshCheckboxStyle.bgSprite.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.enabled = meshCheckboxStyle.bgSprite != null;
		}
		if (meshCheckboxStyle.bgColor.apply && toggle.targetGraphic != null)
		{
			toggle.targetGraphic.color = meshCheckboxStyle.bgColor.value;
		}
		if (meshCheckboxStyle.bgSprite.apply && toggle.targetGraphic is Image)
		{
			((Image)toggle.targetGraphic).sprite = meshCheckboxStyle.bgSprite.value;
		}
		if (meshCheckboxStyle.markColor.apply && toggle.graphic != null)
		{
			toggle.graphic.color = meshCheckboxStyle.markColor.value;
		}
		if (meshCheckboxStyle.markSprite.apply && toggle.graphic is Image)
		{
			((Image)toggle.graphic).sprite = meshCheckboxStyle.markSprite.value;
		}
		ColorBlock colors = toggle.colors;
		if (meshCheckboxStyle.normalTint.apply)
		{
			colors.normalColor = meshCheckboxStyle.normalTint.value;
		}
		if (meshCheckboxStyle.highlightedTint.apply)
		{
			colors.highlightedColor = meshCheckboxStyle.highlightedTint.value;
		}
		if (meshCheckboxStyle.pressedTint.apply)
		{
			colors.pressedColor = meshCheckboxStyle.pressedTint.value;
		}
		if (meshCheckboxStyle.disabledTint.apply)
		{
			colors.disabledColor = meshCheckboxStyle.disabledTint.value;
		}
		toggle.colors = colors;
	}

	public static void Convert(GameObject obj)
	{
		CheckboxStyler[] componentsInChildren = obj.GetComponentsInChildren<CheckboxStyler>(includeInactive: true);
		foreach (CheckboxStyler checkboxStyler in componentsInChildren)
		{
			Toggle component = checkboxStyler.GetComponent<Toggle>();
			CheckboxData checkboxData = new CheckboxData(component, checkboxStyler);
			GameObject obj2 = checkboxStyler.gameObject;
			Object.DestroyImmediate(checkboxStyler);
			MeshCheckboxStyler styler = obj2.AddComponent<MeshCheckboxStyler>();
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
			checkboxData.ApplyTo(component, styler);
		}
	}
}
