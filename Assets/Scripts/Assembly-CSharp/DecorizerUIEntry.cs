using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorizerUIEntry : SRBehaviour
{
	[Tooltip("Main button component.")]
	public Button button;

	[Tooltip("Text component to display the item name.")]
	public new TMP_Text name;

	[Tooltip("Image component to display the item image.")]
	public Image image;

	[Tooltip("Text component to display the item content count.")]
	public TMP_Text count;
}
