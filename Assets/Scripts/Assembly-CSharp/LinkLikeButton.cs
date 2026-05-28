using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class LinkLikeButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Color highlightColor;

	private TMP_Text theText;

	private Color normalColor = Color.blue;

	public void Awake()
	{
		theText = GetComponent<TMP_Text>();
		normalColor = theText.color;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		theText.color = highlightColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		theText.color = normalColor;
	}
}
