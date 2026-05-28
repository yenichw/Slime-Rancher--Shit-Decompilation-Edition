using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class IconStyler : SRBehaviour
{
	[StyleName(typeof(UIStyleDirector.IconStyle))]
	public string styleName = "Default";

	private UIStyleDirector styleDir;

	private Image img;

	public void OnEnable()
	{
		styleDir = UIStyleDirector.Instance;
		img = GetComponent<Image>();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		UIStyleDirector.IconStyle iconStyle = styleDir.GetIconStyle(styleName);
		if (iconStyle == null)
		{
			if (Application.isPlaying)
			{
				Log.Warning("Unknown icon style: " + styleName);
			}
			return;
		}
		if (iconStyle.color.apply)
		{
			img.color = iconStyle.color.value;
		}
		if (iconStyle.sprite.apply)
		{
			img.sprite = iconStyle.sprite.value;
		}
	}
}
