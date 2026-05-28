using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SRStandaloneInputModule : PointerInputModule
{
	private float m_NextAction;

	private Vector2 m_LastMousePosition;

	private Vector2 m_MousePosition;

	[SerializeField]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	private bool m_AllowActivationOnMobileDevice;

	private bool lastNavigatedViaButtons;

	private GameObject lastSelectedViaButtons;

	private bool m_ProcessMouseEvents = true;

	[SerializeField]
	private float m_repeatDelay = 0.5f;

	private int m_ConsecutiveMovementCount;

	private Vector2 m_LastMoveVector;

	private float m_PrevActionTime;

	public bool allowActivationOnMobileDevice
	{
		get
		{
			return m_AllowActivationOnMobileDevice;
		}
		set
		{
			m_AllowActivationOnMobileDevice = value;
		}
	}

	public bool processMouseEvents
	{
		get
		{
			return m_ProcessMouseEvents;
		}
		set
		{
			m_ProcessMouseEvents = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
		}
	}

	public float repeatDelay
	{
		get
		{
			return m_repeatDelay;
		}
		set
		{
			m_repeatDelay = value;
		}
	}

	protected SRStandaloneInputModule()
	{
	}

	public override void UpdateModule()
	{
		m_LastMousePosition = m_MousePosition;
		m_MousePosition = Input.mousePosition;
		if (base.eventSystem.currentSelectedGameObject == null && lastNavigatedViaButtons)
		{
			if (lastSelectedViaButtons == null || !lastSelectedViaButtons.activeInHierarchy)
			{
				InitSelected current = InitSelected.Current;
				if (current != null)
				{
					base.eventSystem.SetSelectedGameObject(current.gameObject, GetBaseEventData());
				}
			}
			else
			{
				base.eventSystem.SetSelectedGameObject(lastSelectedViaButtons, GetBaseEventData());
			}
		}
		else if (base.eventSystem.currentSelectedGameObject != null && base.eventSystem.currentSelectedGameObject.GetComponent<InputField>() == null && !lastNavigatedViaButtons)
		{
			lastSelectedViaButtons = base.eventSystem.currentSelectedGameObject;
			base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
		}
	}

	public override bool IsModuleSupported()
	{
		if (!m_AllowActivationOnMobileDevice)
		{
			return Input.mousePresent;
		}
		return true;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		return SRInput.PauseActions.submit.WasReleased | SRInput.PauseActions.cancel.WasReleased | SRInput.PauseActions.menuUp.WasReleased | SRInput.PauseActions.menuDown.WasReleased | SRInput.PauseActions.menuLeft.WasReleased | SRInput.PauseActions.menuRight.WasReleased | ((m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f) | Input.GetMouseButtonDown(0);
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		m_MousePosition = Input.mousePosition;
		m_LastMousePosition = Input.mousePosition;
		lastNavigatedViaButtons = InputDirector.UsingGamepad();
		if (lastNavigatedViaButtons)
		{
			GameObject gameObject = base.eventSystem.currentSelectedGameObject;
			if (gameObject == null)
			{
				gameObject = base.eventSystem.firstSelectedGameObject;
			}
			base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
		}
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		if (m_ProcessMouseEvents && Cursor.visible)
		{
			ProcessMouseEvent();
		}
	}

	private bool SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		if (SRInput.PauseActions.submit.WasReleased)
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
		}
		if (SRInput.PauseActions.cancel.WasReleased)
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
		}
		return baseEventData.used;
	}

	private bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		Vector2 vector = new Vector2(SRInput.PauseActions.menuRight.RawValue - SRInput.PauseActions.menuLeft.RawValue, SRInput.PauseActions.menuUp.RawValue - SRInput.PauseActions.menuDown.RawValue);
		if (Mathf.Approximately(vector.x, 0f) && Mathf.Approximately(vector.y, 0f))
		{
			m_ConsecutiveMovementCount = 0;
			return false;
		}
		bool flag = Vector2.Dot(vector, m_LastMoveVector) > 0f;
		if (flag && m_ConsecutiveMovementCount == 1)
		{
			if (unscaledTime <= m_PrevActionTime + m_repeatDelay)
			{
				return false;
			}
		}
		else if (unscaledTime <= m_PrevActionTime + 1f / m_InputActionsPerSecond)
		{
			return false;
		}
		AxisEventData axisEventData = GetAxisEventData(vector.x, vector.y, 0.5f);
		if (vector.sqrMagnitude > 0.25f)
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			if (!flag)
			{
				m_ConsecutiveMovementCount = 0;
			}
			m_ConsecutiveMovementCount++;
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = vector;
			lastNavigatedViaButtons = true;
			if (axisEventData.moveDir == MoveDirection.None)
			{
				m_ConsecutiveMovementCount = 0;
			}
			return true;
		}
		if (axisEventData.moveDir == MoveDirection.None)
		{
			m_ConsecutiveMovementCount = 0;
		}
		return false;
	}

	protected void ProcessMouseEvent()
	{
		ProcessMouseEvent(0);
	}

	private void ProcessMouseEvent(int id)
	{
		MouseState mousePointerEventData = GetMousePointerEventData(id);
		bool pressed = mousePointerEventData.AnyPressesThisFrame();
		bool released = mousePointerEventData.AnyReleasesThisFrame();
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		if (UseMouse(pressed, released, eventData.buttonData))
		{
			if (Cursor.visible)
			{
				lastNavigatedViaButtons = false;
			}
			ProcessMousePress(eventData);
			ProcessMove(eventData.buttonData);
			ProcessDrag(eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
	}

	private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
	{
		if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
		{
			return true;
		}
		return false;
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private void ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				if (unscaledTime - buttonData.clickTime < 0.3f)
				{
					int clickCount = buttonData.clickCount + 1;
					buttonData.clickCount = clickCount;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}
}
