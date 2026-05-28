using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class MeshTextStyler : SRBehaviour
{
	public class TextData
	{
		private string styleName;

		private string text;

		private bool bestFit;

		private int minSize;

		private int maxSize;

		private TextAnchor align;

		public TextData(Text textComp, TextStyler styler)
		{
			text = textComp.text;
			align = textComp.alignment;
			bestFit = textComp.resizeTextForBestFit;
			if (styler != null)
			{
				styleName = styler.styleName;
			}
			minSize = textComp.resizeTextMinSize;
			maxSize = textComp.resizeTextMaxSize;
		}

		public virtual void ApplyTo(TextMeshProUGUI textComp, MeshTextStyler styler)
		{
			textComp.text = text;
			if (styleName != null)
			{
				styler.styleName = styleName;
			}
			textComp.fontSizeMin = minSize;
			textComp.fontSizeMax = maxSize;
			textComp.alignment = Convert(align);
			textComp.enableAutoSizing = bestFit;
		}
	}

	[StyleName(typeof(UIStyleDirector.MeshTextStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private TMP_Text text;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		text = GetComponent<TMP_Text>();
		ApplyStyle();
	}

	public void SetStyle(string styleName)
	{
		this.styleName = styleName;
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.MeshTextStyle meshTextStyle = styleDir.GetMeshTextStyle(styleName);
		if (meshTextStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown text style: " + styleName + " in: " + base.gameObject.name);
			}
		}
		else
		{
			ApplyTextStyle(text, meshTextStyle);
		}
	}

	public static void ApplyTextStyle(TMP_Text text, UIStyleDirector.MeshTextStyle style)
	{
		if (style.textColor.apply)
		{
			text.color = style.textColor.value;
		}
		if (style.font.apply)
		{
			text.font = style.font.value;
		}
		if (style.fontSize.apply)
		{
			text.fontSize = style.fontSize.value;
		}
		if (style.italic.apply)
		{
			if (style.italic.value)
			{
				text.fontStyle |= FontStyles.Italic;
			}
			else
			{
				text.fontStyle &= (FontStyles)(-3);
			}
		}
		if (style.bold.apply)
		{
			if (style.bold.value)
			{
				text.fontStyle |= FontStyles.Bold;
			}
			else
			{
				text.fontStyle &= (FontStyles)(-2);
			}
		}
		if (style.materialPreset.apply && style.materialPreset.value != null)
		{
			text.fontMaterial = style.materialPreset.value;
		}
	}

	protected static TextAlignmentOptions Convert(TextAnchor align)
	{
		switch (align)
		{
		case TextAnchor.LowerCenter:
			return TextAlignmentOptions.Bottom;
		case TextAnchor.MiddleCenter:
			return TextAlignmentOptions.Center;
		case TextAnchor.UpperCenter:
			return TextAlignmentOptions.Top;
		case TextAnchor.LowerLeft:
			return TextAlignmentOptions.BottomLeft;
		case TextAnchor.MiddleLeft:
			return TextAlignmentOptions.Left;
		case TextAnchor.UpperLeft:
			return TextAlignmentOptions.TopLeft;
		case TextAnchor.LowerRight:
			return TextAlignmentOptions.BottomRight;
		case TextAnchor.MiddleRight:
			return TextAlignmentOptions.Right;
		case TextAnchor.UpperRight:
			return TextAlignmentOptions.TopRight;
		default:
			return TextAlignmentOptions.Center;
		}
	}

	public static void Convert(GameObject obj)
	{
		TextStyler[] componentsInChildren = obj.GetComponentsInChildren<TextStyler>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Convert(componentsInChildren[i]);
		}
	}

	public static void Convert(TextStyler styler)
	{
		Text component = styler.GetComponent<Text>();
		TextData textData = new TextData(component, styler);
		GameObject obj = styler.gameObject;
		Object.DestroyImmediate(styler);
		Object.DestroyImmediate(component);
		TextMeshProUGUI textComp = obj.AddComponent<TextMeshProUGUI>();
		MeshTextStyler styler2 = obj.AddComponent<MeshTextStyler>();
		textData.ApplyTo(textComp, styler2);
	}
}
