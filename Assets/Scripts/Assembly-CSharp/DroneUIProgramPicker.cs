using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DroneUIProgramPicker : BaseUI
{
	[Tooltip("Title text.")]
	public TMP_Text title;

	[Tooltip("Title icon.")]
	public Image icon;

	[Tooltip("Transform to add content children to.")]
	public Transform contentGrid;
}
