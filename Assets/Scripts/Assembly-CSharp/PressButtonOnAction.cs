using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PressButtonOnAction : MonoBehaviour
{
	public string action;

	[Tooltip("If true, the mouse up/down events will not trigger unless the button is currently selected.")]
	public bool requiresCurrentSelection;

	private Button button;

	private bool isPressed;

	public void Start()
	{
		button = GetComponentsInParent<Button>(includeInactive: true)[0];
	}

	public void Update()
	{
		if (!isPressed && SRInput.GetAction(action).WasPressed && IsButtonAvailable())
		{
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
			isPressed = true;
		}
		else if (isPressed && SRInput.GetAction(action).WasReleased && IsButtonAvailable())
		{
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
			isPressed = false;
		}
		else if (isPressed && !IsButtonAvailable())
		{
			ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
			isPressed = false;
		}
	}

	private bool IsButtonAvailable()
	{
		if (button.IsInteractable() && button.isActiveAndEnabled)
		{
			if (requiresCurrentSelection)
			{
				return EventSystem.current.currentSelectedGameObject == base.gameObject;
			}
			return true;
		}
		return false;
	}
}
