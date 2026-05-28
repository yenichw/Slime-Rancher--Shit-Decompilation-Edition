using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class PanelStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.PanelStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Image bg;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		bg = GetComponent<Image>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.PanelStyle panelStyle = styleDir.GetPanelStyle(styleName);
		if (panelStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown panel style: " + styleName);
			}
			return;
		}
		if (panelStyle.bgSprite.apply)
		{
			bg.enabled = panelStyle.bgSprite != null;
		}
		if (panelStyle.bgColor.apply)
		{
			bg.color = panelStyle.bgColor.value;
		}
		if (panelStyle.bgSprite.apply)
		{
			bg.sprite = panelStyle.bgSprite.value;
		}
	}
}
