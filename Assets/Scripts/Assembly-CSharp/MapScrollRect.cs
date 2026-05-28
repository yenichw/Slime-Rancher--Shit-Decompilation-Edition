using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapScrollRect : ScrollRect
{
	public delegate void OnZoomEvent(float zoomLevel);

	private const float DEFAULT_ZOOM = 1f;

	private const float ZOOM_CHANGE_PER_FRAME = 0.04f;

	private float currentZoom = 1f;

	public static float MinZoom = 0.55f;

	public static float MaxZoom = 2f;

	public OnZoomEvent onZoom;

	public float GetCurrentZoom()
	{
		return currentZoom;
	}

	public override void OnScroll(PointerEventData data)
	{
		if (data.scrollDelta.y < 0f)
		{
			ZoomOut();
		}
		else if (data.scrollDelta.y > 0f)
		{
			ZoomIn();
		}
	}

	public void Scroll(Vector2 scrollDelta)
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.scrollDelta = scrollDelta;
		base.OnScroll(pointerEventData);
	}

	public void ScrollUp()
	{
		Scroll(new Vector2(0f, base.scrollSensitivity));
	}

	public void ScrollDown()
	{
		Scroll(new Vector2(0f, 0f - base.scrollSensitivity));
	}

	public void ScrollLeft()
	{
		Scroll(new Vector2(base.scrollSensitivity, 0f));
	}

	public void ScrollRight()
	{
		Scroll(new Vector2(0f - base.scrollSensitivity, 0f));
	}

	public void ClampContentToScrollView()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.scrollDelta = new Vector2(0f, 0f);
		base.OnScroll(pointerEventData);
	}

	public void ZoomIn()
	{
		ZoomTo(currentZoom + 0.04f);
	}

	public void ZoomOut()
	{
		ZoomTo(currentZoom - 0.04f);
	}

	public void ResetToDefaultZoom()
	{
		ZoomTo(1f);
	}

	public void ZoomTo(float requestedZoomTarget)
	{
		float num = Mathf.Clamp(requestedZoomTarget, MinZoom, MaxZoom);
		if (num != currentZoom)
		{
			float num2 = num / currentZoom;
			Vector3 localPosition = base.content.localPosition;
			base.content.transform.localScale = Vector3.one * num;
			base.content.localPosition = localPosition * num2;
			new PointerEventData(null).scrollDelta = new Vector2(0f, 0f);
			ClampContentToScrollView();
			currentZoom = num;
			if (onZoom != null)
			{
				onZoom(currentZoom);
			}
		}
	}
}
