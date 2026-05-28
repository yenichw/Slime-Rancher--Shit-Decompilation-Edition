using UnityEngine;
using UnityEngine.UI;

public class RecenterableScroll : MonoBehaviour
{
	public float border = 8f;

	private ScrollRect scroll;

	private RectTransform scrollTransform;

	private void Awake()
	{
		scroll = GetComponent<ScrollRect>();
		scrollTransform = GetComponent<RectTransform>();
	}

	public void ScrollToItem(RectTransform itemTransform)
	{
		Vector3[] array = new Vector3[4];
		scrollTransform.GetWorldCorners(array);
		Vector3[] array2 = new Vector3[4];
		scroll.content.GetWorldCorners(array2);
		Vector3[] array3 = new Vector3[4];
		itemTransform.GetWorldCorners(array3);
		float num = array2[1].y - array2[0].y;
		float num2 = array[1].y - array[0].y;
		float num3 = array3[0].y - array2[0].y - border;
		float num4 = array3[1].y - array2[0].y + border;
		float num5 = Mathf.Clamp01(num3 / (num - num2));
		float num6 = Mathf.Clamp01((num4 - num2) / (num - num2));
		if (num6 > scroll.verticalNormalizedPosition)
		{
			scroll.verticalNormalizedPosition = num6;
		}
		else if (num5 < scroll.verticalNormalizedPosition)
		{
			scroll.verticalNormalizedPosition = num5;
		}
		float num7 = array2[2].x - array2[0].x;
		float num8 = array[2].x - array[0].x;
		float num9 = array3[0].x - array2[0].x - border;
		float num10 = array3[2].x - array2[0].x + border;
		float num11 = Mathf.Clamp01(num9 / (num7 - num8));
		float num12 = Mathf.Clamp01((num10 - num8) / (num7 - num8));
		if (num12 > scroll.horizontalNormalizedPosition)
		{
			scroll.horizontalNormalizedPosition = num12;
		}
		else if (num11 < scroll.horizontalNormalizedPosition)
		{
			scroll.horizontalNormalizedPosition = num11;
		}
	}
}
