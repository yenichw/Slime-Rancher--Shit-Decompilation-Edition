using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class TextStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.TextStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Text text;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		text = GetComponent<Text>();
		ApplyStyle();
	}

	public void SetStyle(string styleName)
	{
		this.styleName = styleName;
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.TextStyle textStyle = styleDir.GetTextStyle(styleName);
		if (textStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown text style: " + styleName + " in: " + base.gameObject.name);
			}
		}
		else
		{
			ApplyTextStyle(text, textStyle);
		}
	}

	public static void ApplyTextStyle(Text text, UIStyleDirector.TextStyle style)
	{
		ApplyTextStyle(text, style.textColor, style.font, style.fontSize, style.fontStyle, style.outlineColor, style.outlineWidth);
	}

	public static void ApplyTextStyle(Text text, UIStyleDirector.ColorSetting textColor, UIStyleDirector.FontSetting font, UIStyleDirector.IntSetting fontSize, UIStyleDirector.FontStyleSetting fontStyle, UIStyleDirector.ColorSetting outlineColor, UIStyleDirector.FloatSetting outlineWidth)
	{
		if (textColor.apply)
		{
			text.color = textColor.value;
		}
		if (font.apply)
		{
			text.font = font.value;
		}
		if (fontSize.apply)
		{
			text.fontSize = fontSize.value;
		}
		if (fontStyle.apply)
		{
			text.fontStyle = fontStyle.value;
		}
		if (outlineColor.apply || outlineWidth.apply)
		{
			Outline outline = text.GetComponent<Outline>();
			if (outline == null)
			{
				outline = text.gameObject.AddComponent<Outline>();
			}
			if (outlineColor.apply)
			{
				outline.effectColor = outlineColor.value;
			}
			if (outlineWidth.apply)
			{
				outline.effectDistance = new Vector2(outlineWidth.value, outlineWidth.value);
			}
		}
	}
}
