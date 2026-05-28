using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(InputField))]
public class FieldStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.FieldStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private InputField field;

	private const string PLACEHOLDER_NAME = "Placeholder";

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		field = GetComponent<InputField>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.FieldStyle fieldStyle = styleDir.GetFieldStyle(styleName);
		if (fieldStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown button style: " + styleName);
			}
			return;
		}
		TextStyler.ApplyTextStyle(field.textComponent, fieldStyle);
		if (field.placeholder is Text)
		{
			TextStyler.ApplyTextStyle((Text)field.placeholder, fieldStyle.placeholderTextColor, fieldStyle.placeholderFont, fieldStyle.placeholderFontSize, fieldStyle.placeholderFontStyle, fieldStyle.placeholderOutlineColor, fieldStyle.placeholderOutlineWidth);
		}
		if (fieldStyle.bgColor.apply)
		{
			field.image.color = fieldStyle.bgColor.value;
		}
		ColorBlock colors = field.colors;
		if (fieldStyle.normalTint.apply)
		{
			colors.normalColor = fieldStyle.normalTint.value;
		}
		if (fieldStyle.highlightedTint.apply)
		{
			colors.highlightedColor = fieldStyle.highlightedTint.value;
		}
		if (fieldStyle.pressedTint.apply)
		{
			colors.pressedColor = fieldStyle.pressedTint.value;
		}
		if (fieldStyle.disabledTint.apply)
		{
			colors.disabledColor = fieldStyle.disabledTint.value;
		}
		field.colors = colors;
		if (fieldStyle.bgSprite.apply)
		{
			field.image.sprite = fieldStyle.bgSprite.value;
		}
		if (fieldStyle.selectionColor.apply)
		{
			field.selectionColor = fieldStyle.selectionColor.value;
		}
	}
}
