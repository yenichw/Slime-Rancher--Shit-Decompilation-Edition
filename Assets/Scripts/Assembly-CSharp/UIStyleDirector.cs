using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIStyleDirector : SRBehaviour
{
	public class Setting
	{
	}

	[Serializable]
	public class TransitionSetting : Setting
	{
		public bool apply;

		public Selectable.Transition value;
	}

	[Serializable]
	public class ColorSetting : Setting
	{
		public bool apply;

		public Color value;
	}

	[Serializable]
	public class SpriteSetting : Setting
	{
		public bool apply;

		public Sprite value;
	}

	[Serializable]
	public class GameObjSetting : Setting
	{
		public bool apply;

		public GameObject value;
	}

	[Serializable]
	public class FontSetting : Setting
	{
		public bool apply;

		public Font value;
	}

	[Serializable]
	public class FontStyleSetting : Setting
	{
		public bool apply;

		public FontStyle value;
	}

	[Serializable]
	public class MeshFontSetting : Setting
	{
		public bool apply;

		public TMP_FontAsset value;
	}

	[Serializable]
	public class IntSetting : Setting
	{
		public bool apply;

		public int value;
	}

	[Serializable]
	public class BoolSetting : Setting
	{
		public bool apply;

		public bool value;
	}

	[Serializable]
	public class FloatSetting : Setting
	{
		public bool apply;

		public float value;
	}

	[Serializable]
	public class MaterialPresetSetting : Setting
	{
		public bool apply;

		public Material value;
	}

	[Serializable]
	public class MeshTextStyle
	{
		public string name;

		public ColorSetting textColor;

		public MeshFontSetting font;

		public MaterialPresetSetting materialPreset;

		public BoolSetting bold;

		public BoolSetting italic;

		public IntSetting fontSize;

		internal void Convert(UIStyleDirector styleDir, TextStyle oldStyle)
		{
			bold = new BoolSetting();
			bold.value = oldStyle.fontStyle.value == FontStyle.Bold || oldStyle.fontStyle.value == FontStyle.BoldAndItalic;
			bold.apply = oldStyle.fontStyle.apply;
			italic = new BoolSetting();
			italic.value = oldStyle.fontStyle.value == FontStyle.Italic || oldStyle.fontStyle.value == FontStyle.BoldAndItalic;
			italic.apply = oldStyle.fontStyle.apply;
			font = new MeshFontSetting();
			font.value = styleDir.defaultMeshFont;
			font.apply = oldStyle.font.apply;
			fontSize = new IntSetting();
			fontSize.value = oldStyle.fontSize.value;
			fontSize.apply = oldStyle.fontSize.apply;
			textColor = new ColorSetting();
			textColor.value = oldStyle.textColor.value;
			textColor.apply = oldStyle.textColor.apply;
			materialPreset = null;
			name = oldStyle.name;
		}
	}

	[Serializable]
	public class TextStyle
	{
		public string name;

		public ColorSetting textColor;

		public FontSetting font;

		public FontStyleSetting fontStyle;

		public IntSetting fontSize;

		public ColorSetting outlineColor;

		public FloatSetting outlineWidth;
	}

	[Serializable]
	public class MeshButtonStyle : MeshTextStyle
	{
		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public GameObjSetting includeChild;

		public bool hideIfGamepad;

		public TransitionSetting transition;

		public SpriteSetting disabledSprite;

		public SpriteSetting highlightedSprite;

		public SpriteSetting pressedSprite;

		public void Convert(UIStyleDirector styleDir, ButtonStyle buttonStyle)
		{
			Convert(styleDir, (TextStyle)buttonStyle);
			bgSprite = buttonStyle.bgSprite;
			bgColor = buttonStyle.bgColor;
			normalTint = buttonStyle.normalTint;
			highlightedTint = buttonStyle.highlightedTint;
			pressedTint = buttonStyle.pressedTint;
			disabledTint = buttonStyle.disabledTint;
			includeChild = buttonStyle.includeChild;
			hideIfGamepad = buttonStyle.hideIfGamepad;
			transition = buttonStyle.transition;
			disabledSprite = buttonStyle.disabledSprite;
			highlightedSprite = buttonStyle.highlightedSprite;
			pressedSprite = buttonStyle.pressedSprite;
		}
	}

	[Serializable]
	public class ButtonStyle : TextStyle
	{
		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public GameObjSetting includeChild;

		public bool hideIfGamepad;

		public TransitionSetting transition;

		public SpriteSetting disabledSprite;

		public SpriteSetting highlightedSprite;

		public SpriteSetting pressedSprite;
	}

	[Serializable]
	public class MeshDropdownStyle : MeshTextStyle
	{
		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public SpriteSetting menuBgSprite;

		public ColorSetting menuBgColor;

		public SpriteSetting itemBgSprite;

		public ColorSetting itemBgColor;

		public ColorSetting itemNormalTint;

		public ColorSetting itemHighlightedTint;

		public ColorSetting itemPressedTint;

		public ColorSetting itemDisabledTint;

		public void Convert(UIStyleDirector styleDir, DropdownStyle dropdownStyle)
		{
			Convert(styleDir, (TextStyle)dropdownStyle);
			bgSprite = dropdownStyle.bgSprite;
			bgColor = dropdownStyle.bgColor;
			normalTint = dropdownStyle.normalTint;
			highlightedTint = dropdownStyle.highlightedTint;
			pressedTint = dropdownStyle.pressedTint;
			disabledTint = dropdownStyle.disabledTint;
			menuBgSprite = dropdownStyle.menuBgSprite;
			menuBgColor = dropdownStyle.menuBgColor;
			itemBgSprite = dropdownStyle.itemBgSprite;
			itemBgColor = dropdownStyle.itemBgColor;
			itemNormalTint = dropdownStyle.itemNormalTint;
			itemHighlightedTint = dropdownStyle.itemHighlightedTint;
			itemPressedTint = dropdownStyle.itemPressedTint;
			itemDisabledTint = dropdownStyle.itemDisabledTint;
		}
	}

	[Serializable]
	public class DropdownStyle : TextStyle
	{
		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public SpriteSetting menuBgSprite;

		public ColorSetting menuBgColor;

		public SpriteSetting itemBgSprite;

		public ColorSetting itemBgColor;

		public ColorSetting itemNormalTint;

		public ColorSetting itemHighlightedTint;

		public ColorSetting itemPressedTint;

		public ColorSetting itemDisabledTint;
	}

	[Serializable]
	public class PanelStyle
	{
		public string name;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;
	}

	[Serializable]
	public class IconStyle
	{
		public string name;

		public SpriteSetting sprite;

		public ColorSetting color;
	}

	[Serializable]
	public class FieldStyle : TextStyle
	{
		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public ColorSetting placeholderTextColor;

		public FontSetting placeholderFont;

		public FontStyleSetting placeholderFontStyle;

		public IntSetting placeholderFontSize;

		public ColorSetting placeholderOutlineColor;

		public FloatSetting placeholderOutlineWidth;

		public ColorSetting selectionColor;
	}

	[Serializable]
	public class ScrollbarStyle
	{
		public string name;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public SpriteSetting handleSprite;

		public ColorSetting handleColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;
	}

	[Serializable]
	public class MeshCheckboxStyle : MeshTextStyle
	{
		public SpriteSetting markSprite;

		public ColorSetting markColor;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public void Convert(UIStyleDirector styleDir, CheckboxStyle checkboxStyle)
		{
			Convert(styleDir, (TextStyle)checkboxStyle);
			markSprite = checkboxStyle.markSprite;
			markColor = checkboxStyle.markColor;
			bgSprite = checkboxStyle.bgSprite;
			bgColor = checkboxStyle.bgColor;
			normalTint = checkboxStyle.normalTint;
			highlightedTint = checkboxStyle.highlightedTint;
			pressedTint = checkboxStyle.pressedTint;
			disabledTint = checkboxStyle.disabledTint;
		}
	}

	[Serializable]
	public class CheckboxStyle : TextStyle
	{
		public SpriteSetting markSprite;

		public ColorSetting markColor;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;
	}

	[Serializable]
	public class MeshToggleButtonStyle : MeshTextStyle
	{
		public SpriteSetting selectedSprite;

		public ColorSetting selectedColor;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;

		public void Convert(UIStyleDirector styleDir, ToggleButtonStyle toggleButtonStyle)
		{
			Convert(styleDir, (TextStyle)toggleButtonStyle);
			selectedSprite = toggleButtonStyle.selectedSprite;
			selectedColor = toggleButtonStyle.selectedColor;
			bgSprite = toggleButtonStyle.bgSprite;
			bgColor = toggleButtonStyle.bgColor;
			normalTint = toggleButtonStyle.normalTint;
			highlightedTint = toggleButtonStyle.highlightedTint;
			pressedTint = toggleButtonStyle.pressedTint;
			disabledTint = toggleButtonStyle.disabledTint;
		}
	}

	[Serializable]
	public class ToggleButtonStyle : TextStyle
	{
		public SpriteSetting selectedSprite;

		public ColorSetting selectedColor;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;
	}

	[Serializable]
	public class SliderStyle
	{
		public string name;

		public SpriteSetting bgSprite;

		public ColorSetting bgColor;

		public SpriteSetting fillSprite;

		public ColorSetting fillColor;

		public SpriteSetting handleSprite;

		public ColorSetting handleColor;

		public ColorSetting normalTint;

		public ColorSetting highlightedTint;

		public ColorSetting pressedTint;

		public ColorSetting disabledTint;
	}

	public TextStyle[] textStyles = new TextStyle[0];

	public MeshTextStyle[] meshTextStyles = new MeshTextStyle[0];

	public DropdownStyle[] dropdownStyles = new DropdownStyle[0];

	public MeshDropdownStyle[] meshDropdownStyles = new MeshDropdownStyle[0];

	public ButtonStyle[] buttonStyles = new ButtonStyle[0];

	public MeshButtonStyle[] meshButtonStyles = new MeshButtonStyle[0];

	public PanelStyle[] panelStyles = new PanelStyle[0];

	public IconStyle[] iconStyles = new IconStyle[0];

	public FieldStyle[] fieldStyles = new FieldStyle[0];

	public ScrollbarStyle[] scrollbarStyles = new ScrollbarStyle[0];

	public CheckboxStyle[] checkboxStyles = new CheckboxStyle[0];

	public MeshCheckboxStyle[] meshCheckboxStyles = new MeshCheckboxStyle[0];

	public ToggleButtonStyle[] toggleButtonStyles = new ToggleButtonStyle[0];

	public MeshToggleButtonStyle[] meshToggleButtonStyles = new MeshToggleButtonStyle[0];

	public SliderStyle[] sliderStyles = new SliderStyle[0];

	private Dictionary<string, TextStyle> textDict = new Dictionary<string, TextStyle>();

	private string[] textStyleNames = new string[0];

	private Dictionary<string, MeshTextStyle> meshTextDict = new Dictionary<string, MeshTextStyle>();

	private string[] meshTextStyleNames = new string[0];

	private Dictionary<string, ButtonStyle> buttonDict = new Dictionary<string, ButtonStyle>();

	private string[] buttonStyleNames = new string[0];

	private Dictionary<string, MeshButtonStyle> meshButtonDict = new Dictionary<string, MeshButtonStyle>();

	private string[] meshButtonStyleNames = new string[0];

	private Dictionary<string, DropdownStyle> dropdownDict = new Dictionary<string, DropdownStyle>();

	private string[] dropdownStyleNames = new string[0];

	private Dictionary<string, MeshDropdownStyle> meshDropdownDict = new Dictionary<string, MeshDropdownStyle>();

	private string[] meshDropdownStyleNames = new string[0];

	private Dictionary<string, PanelStyle> panelDict = new Dictionary<string, PanelStyle>();

	private string[] panelStyleNames = new string[0];

	private Dictionary<string, IconStyle> iconDict = new Dictionary<string, IconStyle>();

	private string[] iconStyleNames = new string[0];

	private Dictionary<string, FieldStyle> fieldDict = new Dictionary<string, FieldStyle>();

	private string[] fieldStyleNames = new string[0];

	private Dictionary<string, ScrollbarStyle> scrollbarDict = new Dictionary<string, ScrollbarStyle>();

	private string[] scrollbarStyleNames = new string[0];

	private Dictionary<string, CheckboxStyle> checkboxDict = new Dictionary<string, CheckboxStyle>();

	private string[] checkboxStyleNames = new string[0];

	private Dictionary<string, MeshCheckboxStyle> meshCheckboxDict = new Dictionary<string, MeshCheckboxStyle>();

	private string[] meshCheckboxStyleNames = new string[0];

	private Dictionary<string, ToggleButtonStyle> toggleButtonDict = new Dictionary<string, ToggleButtonStyle>();

	private string[] toggleButtonStyleNames = new string[0];

	private Dictionary<string, MeshToggleButtonStyle> meshToggleButtonDict = new Dictionary<string, MeshToggleButtonStyle>();

	private string[] meshToggleButtonStyleNames = new string[0];

	private Dictionary<string, SliderStyle> sliderDict = new Dictionary<string, SliderStyle>();

	private string[] sliderStyleNames = new string[0];

	public TMP_FontAsset defaultMeshFont;

	private static UIStyleDirector instance;

	public static UIStyleDirector Instance
	{
		get
		{
			if ((object)instance == null)
			{
				instance = CreateInstance();
			}
			return instance;
		}
	}

	private MeshTextStyle ConvertToMesh(TextStyle style)
	{
		MeshTextStyle meshTextStyle = new MeshTextStyle();
		meshTextStyle.Convert(this, style);
		return meshTextStyle;
	}

	private MeshButtonStyle ConvertToMesh(ButtonStyle style)
	{
		MeshButtonStyle meshButtonStyle = new MeshButtonStyle();
		meshButtonStyle.Convert(this, style);
		return meshButtonStyle;
	}

	private MeshDropdownStyle ConvertToMesh(DropdownStyle style)
	{
		MeshDropdownStyle meshDropdownStyle = new MeshDropdownStyle();
		meshDropdownStyle.Convert(this, style);
		return meshDropdownStyle;
	}

	private MeshCheckboxStyle ConvertToMesh(CheckboxStyle style)
	{
		MeshCheckboxStyle meshCheckboxStyle = new MeshCheckboxStyle();
		meshCheckboxStyle.Convert(this, style);
		return meshCheckboxStyle;
	}

	private MeshToggleButtonStyle ConvertToMesh(ToggleButtonStyle style)
	{
		MeshToggleButtonStyle meshToggleButtonStyle = new MeshToggleButtonStyle();
		meshToggleButtonStyle.Convert(this, style);
		return meshToggleButtonStyle;
	}

	public void OnEnable()
	{
		List<string> list = new List<string>();
		TextStyle[] array = textStyles;
		foreach (TextStyle textStyle in array)
		{
			textDict[textStyle.name] = textStyle;
			list.Add(textStyle.name);
		}
		textStyleNames = list.ToArray();
		List<string> list2 = new List<string>();
		MeshTextStyle[] array2 = meshTextStyles;
		foreach (MeshTextStyle meshTextStyle in array2)
		{
			meshTextDict[meshTextStyle.name] = meshTextStyle;
			list2.Add(meshTextStyle.name);
		}
		meshTextStyleNames = list2.ToArray();
		List<string> list3 = new List<string>();
		ButtonStyle[] array3 = buttonStyles;
		foreach (ButtonStyle buttonStyle in array3)
		{
			buttonDict[buttonStyle.name] = buttonStyle;
			list3.Add(buttonStyle.name);
		}
		buttonStyleNames = list3.ToArray();
		List<string> list4 = new List<string>();
		MeshButtonStyle[] array4 = meshButtonStyles;
		foreach (MeshButtonStyle meshButtonStyle in array4)
		{
			meshButtonDict[meshButtonStyle.name] = meshButtonStyle;
			list4.Add(meshButtonStyle.name);
		}
		meshButtonStyleNames = list4.ToArray();
		List<string> list5 = new List<string>();
		DropdownStyle[] array5 = dropdownStyles;
		foreach (DropdownStyle dropdownStyle in array5)
		{
			dropdownDict[dropdownStyle.name] = dropdownStyle;
			list5.Add(dropdownStyle.name);
		}
		dropdownStyleNames = list5.ToArray();
		List<string> list6 = new List<string>();
		MeshDropdownStyle[] array6 = meshDropdownStyles;
		foreach (MeshDropdownStyle meshDropdownStyle in array6)
		{
			meshDropdownDict[meshDropdownStyle.name] = meshDropdownStyle;
			list6.Add(meshDropdownStyle.name);
		}
		meshDropdownStyleNames = list6.ToArray();
		List<string> list7 = new List<string>();
		PanelStyle[] array7 = panelStyles;
		foreach (PanelStyle panelStyle in array7)
		{
			panelDict[panelStyle.name] = panelStyle;
			list7.Add(panelStyle.name);
		}
		panelStyleNames = list7.ToArray();
		List<string> list8 = new List<string>();
		IconStyle[] array8 = iconStyles;
		foreach (IconStyle iconStyle in array8)
		{
			iconDict[iconStyle.name] = iconStyle;
			list8.Add(iconStyle.name);
		}
		iconStyleNames = list8.ToArray();
		List<string> list9 = new List<string>();
		FieldStyle[] array9 = fieldStyles;
		foreach (FieldStyle fieldStyle in array9)
		{
			fieldDict[fieldStyle.name] = fieldStyle;
			list9.Add(fieldStyle.name);
		}
		fieldStyleNames = list9.ToArray();
		List<string> list10 = new List<string>();
		ScrollbarStyle[] array10 = scrollbarStyles;
		foreach (ScrollbarStyle scrollbarStyle in array10)
		{
			scrollbarDict[scrollbarStyle.name] = scrollbarStyle;
			list10.Add(scrollbarStyle.name);
		}
		scrollbarStyleNames = list10.ToArray();
		List<string> list11 = new List<string>();
		CheckboxStyle[] array11 = checkboxStyles;
		foreach (CheckboxStyle checkboxStyle in array11)
		{
			checkboxDict[checkboxStyle.name] = checkboxStyle;
			list11.Add(checkboxStyle.name);
		}
		checkboxStyleNames = list11.ToArray();
		List<string> list12 = new List<string>();
		MeshCheckboxStyle[] array12 = meshCheckboxStyles;
		foreach (MeshCheckboxStyle meshCheckboxStyle in array12)
		{
			meshCheckboxDict[meshCheckboxStyle.name] = meshCheckboxStyle;
			list12.Add(meshCheckboxStyle.name);
		}
		meshCheckboxStyleNames = list12.ToArray();
		List<string> list13 = new List<string>();
		ToggleButtonStyle[] array13 = toggleButtonStyles;
		foreach (ToggleButtonStyle toggleButtonStyle in array13)
		{
			toggleButtonDict[toggleButtonStyle.name] = toggleButtonStyle;
			list13.Add(toggleButtonStyle.name);
		}
		toggleButtonStyleNames = list13.ToArray();
		List<string> list14 = new List<string>();
		MeshToggleButtonStyle[] array14 = meshToggleButtonStyles;
		foreach (MeshToggleButtonStyle meshToggleButtonStyle in array14)
		{
			meshToggleButtonDict[meshToggleButtonStyle.name] = meshToggleButtonStyle;
			list14.Add(meshToggleButtonStyle.name);
		}
		meshToggleButtonStyleNames = list14.ToArray();
		List<string> list15 = new List<string>();
		SliderStyle[] array15 = sliderStyles;
		foreach (SliderStyle sliderStyle in array15)
		{
			sliderDict[sliderStyle.name] = sliderStyle;
			list15.Add(sliderStyle.name);
		}
		sliderStyleNames = list15.ToArray();
	}

	public TextStyle GetTextStyle(string name)
	{
		return textDict.Get(name);
	}

	public string[] GetTextStyles()
	{
		return textStyleNames;
	}

	public MeshTextStyle GetMeshTextStyle(string name)
	{
		return meshTextDict.Get(name);
	}

	public string[] GetMeshTextStyles()
	{
		return meshTextStyleNames;
	}

	public ButtonStyle GetButtonStyle(string name)
	{
		return buttonDict.Get(name);
	}

	public string[] GetButtonStyles()
	{
		return buttonStyleNames;
	}

	public MeshButtonStyle GetMeshButtonStyle(string name)
	{
		return meshButtonDict.Get(name);
	}

	public string[] GetMeshButtonStyles()
	{
		return meshButtonStyleNames;
	}

	public DropdownStyle GetDropdownStyle(string name)
	{
		return dropdownDict.Get(name);
	}

	public string[] GetDropdownStyles()
	{
		return dropdownStyleNames;
	}

	public MeshDropdownStyle GetMeshDropdownStyle(string name)
	{
		return meshDropdownDict.Get(name);
	}

	public string[] GetMeshDropdownStyles()
	{
		return meshDropdownStyleNames;
	}

	public PanelStyle GetPanelStyle(string name)
	{
		return panelDict.Get(name);
	}

	public string[] GetPanelStyles()
	{
		return panelStyleNames;
	}

	public IconStyle GetIconStyle(string name)
	{
		return iconDict.Get(name);
	}

	public string[] GetIconStyles()
	{
		return iconStyleNames;
	}

	public FieldStyle GetFieldStyle(string name)
	{
		return fieldDict.Get(name);
	}

	public string[] GetFieldStyles()
	{
		return fieldStyleNames;
	}

	public ScrollbarStyle GetScrollbarStyle(string name)
	{
		return scrollbarDict.Get(name);
	}

	public string[] GetScrollbarStyles()
	{
		return scrollbarStyleNames;
	}

	public CheckboxStyle GetCheckboxStyle(string name)
	{
		return checkboxDict.Get(name);
	}

	public string[] GetCheckboxStyles()
	{
		return checkboxStyleNames;
	}

	public MeshCheckboxStyle GetMeshCheckboxStyle(string name)
	{
		return meshCheckboxDict.Get(name);
	}

	public string[] GetMeshCheckboxStyles()
	{
		return meshCheckboxStyleNames;
	}

	public ToggleButtonStyle GetToggleButtonStyle(string name)
	{
		return toggleButtonDict.Get(name);
	}

	public string[] GetToggleButtonStyles()
	{
		return toggleButtonStyleNames;
	}

	public MeshToggleButtonStyle GetMeshToggleButtonStyle(string name)
	{
		return meshToggleButtonDict.Get(name);
	}

	public string[] GetMeshToggleButtonStyles()
	{
		return meshToggleButtonStyleNames;
	}

	public SliderStyle GetSliderStyle(string name)
	{
		return sliderDict.Get(name);
	}

	public string[] GetSliderStyles()
	{
		return sliderStyleNames;
	}

	public string[] GetStyleNames(Type type)
	{
		if (type == typeof(ButtonStyle))
		{
			return GetButtonStyles();
		}
		if (type == typeof(MeshButtonStyle))
		{
			return GetMeshButtonStyles();
		}
		if (type == typeof(DropdownStyle))
		{
			return GetDropdownStyles();
		}
		if (type == typeof(MeshDropdownStyle))
		{
			return GetMeshDropdownStyles();
		}
		if (type == typeof(TextStyle))
		{
			return GetTextStyles();
		}
		if (type == typeof(MeshTextStyle))
		{
			return GetMeshTextStyles();
		}
		if (type == typeof(PanelStyle))
		{
			return GetPanelStyles();
		}
		if (type == typeof(IconStyle))
		{
			return GetIconStyles();
		}
		if (type == typeof(FieldStyle))
		{
			return GetFieldStyles();
		}
		if (type == typeof(ScrollbarStyle))
		{
			return GetScrollbarStyles();
		}
		if (type == typeof(CheckboxStyle))
		{
			return GetCheckboxStyles();
		}
		if (type == typeof(MeshCheckboxStyle))
		{
			return GetMeshCheckboxStyles();
		}
		if (type == typeof(ToggleButtonStyle))
		{
			return GetToggleButtonStyles();
		}
		if (type == typeof(MeshToggleButtonStyle))
		{
			return GetMeshToggleButtonStyles();
		}
		if (type == typeof(SliderStyle))
		{
			return GetSliderStyles();
		}
		throw new Exception("Invalid type provided to style");
	}

	private static UIStyleDirector CreateInstance()
	{
		return UnityEngine.Object.Instantiate(Resources.Load("UIStyleDirector") as GameObject).GetComponent<UIStyleDirector>();
	}
}
