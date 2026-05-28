using UnityEngine;
using UnityEngine.EventSystems;

public class PerformWhileMouseDown : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public delegate void MouseIsDownEvent();

	private bool isMouseDown;

	public MouseIsDownEvent WhileMouseIsDown;

	public void Update()
	{
		if (isMouseDown)
		{
			WhileMouseIsDown();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isMouseDown = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isMouseDown = false;
	}

	public void OnDestroy()
	{
		WhileMouseIsDown = null;
	}
}
