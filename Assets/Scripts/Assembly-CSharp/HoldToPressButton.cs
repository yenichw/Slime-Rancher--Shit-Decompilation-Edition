using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldToPressButton : Button, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	public HoldToPress holdToPress;

	public bool stillPressed;

	protected override void Awake()
	{
		base.Awake();
		holdToPress = GetComponent<HoldToPress>();
		stillPressed = false;
	}

	public void Update()
	{
		if (IsPressed() && !stillPressed)
		{
			BeginPress();
			stillPressed = true;
		}
		else if (stillPressed && !IsPressed())
		{
			EndPress();
			stillPressed = false;
		}
	}

	public void BeginPress()
	{
		holdToPress.enabled = true;
	}

	public void EndPress()
	{
		holdToPress.enabled = false;
	}

	public void OnHoldComplete()
	{
		holdToPress.enabled = false;
	}
}
