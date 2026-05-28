using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SRInputField : InputField
{
	public string descKey;

	private bool requestingKeyboard;

	private bool keyboardActive;

	protected override void Start()
	{
		base.Start();
	}

	public override void OnUpdateSelected(BaseEventData data)
	{
		bool flag = InputDirector.UsingGamepad();
		if (flag && (bool)SRInput.PauseActions.submit)
		{
			requestingKeyboard = true;
			data.Use();
		}
		else if (flag && (bool)SRInput.PauseActions.cancel)
		{
			requestingKeyboard = false;
			data.Use();
		}
		else if (!flag || (!SRInput.PauseActions.menuDown && !SRInput.PauseActions.menuUp))
		{
			base.OnUpdateSelected(data);
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (requestingKeyboard && !keyboardActive)
		{
			ActivateKeyboard();
		}
		else if (!requestingKeyboard && keyboardActive)
		{
			DeactivateKeyboard();
		}
	}

	private void ActivateKeyboard()
	{
		keyboardActive = true;
	}

	private void ProcessVirtualKeyboardInput(string inputString)
	{
		if (inputString != null)
		{
			base.text = inputString;
		}
		DeactivateInputField();
		requestingKeyboard = false;
	}

	private void DeactivateKeyboard()
	{
		keyboardActive = false;
	}
}
