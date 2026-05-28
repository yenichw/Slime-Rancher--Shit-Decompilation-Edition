using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Button))]
public class MeshButtonStyler : SRBehaviour
{
	protected class ButtonData
	{
		private Dictionary<string, MeshTextStyler.TextData> textDict;

		private string styleName;

		public ButtonData(Button button, ButtonStyler styler)
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

		public void ApplyTo(Button button, MeshButtonStyler styler)
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

	[StyleName(typeof(UIStyleDirector.MeshButtonStyle))]
	public string styleName = "Default";

	private InputDirector inputDir;

	private UIStyleDirector styleDir;

	private Button button;

	private GameObject includeChildInstance;

	public void Awake()
	{
		if (Application.isPlaying)
		{
			inputDir = SRSingleton<GameContext>.Instance.InputDirector;
		}
	}

	public void OnDestroy()
	{
		if (Application.isPlaying)
		{
			InputDirector inputDirector = inputDir;
			inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnInputDeviceChanged));
		}
	}

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		button = GetComponent<Button>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.MeshButtonStyle meshButtonStyle = styleDir.GetMeshButtonStyle(styleName);
		if (meshButtonStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown button style: " + styleName);
			}
			return;
		}
		if (Application.isPlaying)
		{
			InputDirector inputDirector = inputDir;
			inputDirector.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(inputDirector.onKeysChanged, new InputDirector.OnKeysChanged(OnInputDeviceChanged));
			if (meshButtonStyle.hideIfGamepad)
			{
				InputDirector inputDirector2 = inputDir;
				inputDirector2.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector2.onKeysChanged, new InputDirector.OnKeysChanged(OnInputDeviceChanged));
			}
			OnInputDeviceChanged();
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
			MeshTextStyler.ApplyTextStyle(item, meshButtonStyle);
		}
		if (meshButtonStyle.bgSprite.apply)
		{
			button.image.enabled = meshButtonStyle.bgSprite.value != null;
		}
		if (meshButtonStyle.bgColor.apply)
		{
			button.image.color = meshButtonStyle.bgColor.value;
		}
		ColorBlock colors = button.colors;
		if (meshButtonStyle.normalTint.apply)
		{
			colors.normalColor = meshButtonStyle.normalTint.value;
		}
		if (meshButtonStyle.highlightedTint.apply)
		{
			colors.highlightedColor = meshButtonStyle.highlightedTint.value;
		}
		if (meshButtonStyle.pressedTint.apply)
		{
			colors.pressedColor = meshButtonStyle.pressedTint.value;
		}
		if (meshButtonStyle.disabledTint.apply)
		{
			colors.disabledColor = meshButtonStyle.disabledTint.value;
		}
		button.colors = colors;
		SpriteState spriteState = button.spriteState;
		if (meshButtonStyle.disabledSprite.apply)
		{
			spriteState.disabledSprite = meshButtonStyle.disabledSprite.value;
		}
		if (meshButtonStyle.highlightedSprite.apply)
		{
			spriteState.highlightedSprite = meshButtonStyle.highlightedSprite.value;
		}
		if (meshButtonStyle.pressedSprite.apply)
		{
			spriteState.pressedSprite = meshButtonStyle.pressedSprite.value;
		}
		button.spriteState = spriteState;
		if (meshButtonStyle.transition.apply)
		{
			button.transition = meshButtonStyle.transition.value;
		}
		if (meshButtonStyle.bgSprite.apply)
		{
			button.image.sprite = meshButtonStyle.bgSprite.value;
		}
		if (Application.isPlaying && meshButtonStyle.includeChild.apply && meshButtonStyle.includeChild.value != null)
		{
			if (includeChildInstance != null)
			{
				Destroyer.Destroy(includeChildInstance, "MeshButtonStyler.ApplyStyle");
				includeChildInstance = null;
			}
			includeChildInstance = UnityEngine.Object.Instantiate(meshButtonStyle.includeChild.value);
			includeChildInstance.transform.SetParent(base.transform, worldPositionStays: false);
			includeChildInstance.name = meshButtonStyle.includeChild.value.name;
		}
	}

	private void OnInputDeviceChanged()
	{
		UIStyleDirector.MeshButtonStyle meshButtonStyle = styleDir.GetMeshButtonStyle(styleName);
		base.gameObject.SetActive(!meshButtonStyle.hideIfGamepad || !InputDirector.UsingGamepad());
	}

	public static void Convert(GameObject obj)
	{
		ButtonStyler[] componentsInChildren = obj.GetComponentsInChildren<ButtonStyler>(includeInactive: true);
		foreach (ButtonStyler buttonStyler in componentsInChildren)
		{
			Button component = buttonStyler.GetComponent<Button>();
			ButtonData buttonData = new ButtonData(component, buttonStyler);
			GameObject obj2 = buttonStyler.gameObject;
			UnityEngine.Object.DestroyImmediate(buttonStyler);
			MeshButtonStyler styler = obj2.AddComponent<MeshButtonStyler>();
			Text[] componentsInChildren2 = obj2.GetComponentsInChildren<Text>();
			foreach (Text text in componentsInChildren2)
			{
				if (!text.GetComponent<TextStyler>() && !text.GetComponent<MeshTextStyler>())
				{
					GameObject obj3 = text.gameObject;
					UnityEngine.Object.DestroyImmediate(text);
					obj3.AddComponent<TextMeshProUGUI>();
				}
			}
			buttonData.ApplyTo(component, styler);
		}
	}
}
