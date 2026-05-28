using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Button))]
public class ButtonStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.ButtonStyle))]
	public string styleName = "Default";

	private InputDirector inputDir;

	private UIStyleDirector styleDir;

	private Button button;

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
		UIStyleDirector.ButtonStyle buttonStyle = styleDir.GetButtonStyle(styleName);
		if (buttonStyle == null)
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
			if (buttonStyle.hideIfGamepad)
			{
				InputDirector inputDirector2 = inputDir;
				inputDirector2.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(inputDirector2.onKeysChanged, new InputDirector.OnKeysChanged(OnInputDeviceChanged));
			}
			OnInputDeviceChanged();
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
			TextStyler.ApplyTextStyle(item, buttonStyle);
		}
		if (buttonStyle.bgSprite.apply)
		{
			button.image.enabled = buttonStyle.bgSprite.value != null;
		}
		if (buttonStyle.bgColor.apply)
		{
			button.image.color = buttonStyle.bgColor.value;
		}
		ColorBlock colors = button.colors;
		if (buttonStyle.normalTint.apply)
		{
			colors.normalColor = buttonStyle.normalTint.value;
		}
		if (buttonStyle.highlightedTint.apply)
		{
			colors.highlightedColor = buttonStyle.highlightedTint.value;
		}
		if (buttonStyle.pressedTint.apply)
		{
			colors.pressedColor = buttonStyle.pressedTint.value;
		}
		if (buttonStyle.disabledTint.apply)
		{
			colors.disabledColor = buttonStyle.disabledTint.value;
		}
		button.colors = colors;
		SpriteState spriteState = button.spriteState;
		if (buttonStyle.disabledSprite.apply)
		{
			spriteState.disabledSprite = buttonStyle.disabledSprite.value;
		}
		if (buttonStyle.highlightedSprite.apply)
		{
			spriteState.highlightedSprite = buttonStyle.highlightedSprite.value;
		}
		if (buttonStyle.pressedSprite.apply)
		{
			spriteState.pressedSprite = buttonStyle.pressedSprite.value;
		}
		button.spriteState = spriteState;
		if (buttonStyle.transition.apply)
		{
			button.transition = buttonStyle.transition.value;
		}
		if (buttonStyle.bgSprite.apply)
		{
			button.image.sprite = buttonStyle.bgSprite.value;
		}
		if (Application.isPlaying && buttonStyle.includeChild.apply && buttonStyle.includeChild.value != null)
		{
			Transform transform = base.transform.Find(buttonStyle.includeChild.value.name);
			GameObject gameObject = ((transform == null) ? null : transform.gameObject);
			if (gameObject != null)
			{
				Destroyer.Destroy(gameObject, "ButtonStyler.ApplyStyle");
			}
			GameObject obj = UnityEngine.Object.Instantiate(buttonStyle.includeChild.value);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			obj.name = buttonStyle.includeChild.value.name;
		}
	}

	private void OnInputDeviceChanged()
	{
		UIStyleDirector.ButtonStyle buttonStyle = styleDir.GetButtonStyle(styleName);
		base.gameObject.SetActive(!buttonStyle.hideIfGamepad || !InputDirector.UsingGamepad());
	}
}
