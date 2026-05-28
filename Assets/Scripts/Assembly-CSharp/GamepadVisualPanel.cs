using InControl;
using TMPro;
using UnityEngine;

public class GamepadVisualPanel : MonoBehaviour
{
	public TMP_Text aText;

	public TMP_Text bText;

	public TMP_Text xText;

	public TMP_Text yText;

	public TMP_Text lbText;

	public TMP_Text ltText;

	public TMP_Text rbText;

	public TMP_Text rtText;

	public TMP_Text upText;

	public TMP_Text rightText;

	public TMP_Text downText;

	public TMP_Text leftText;

	public TMP_Text backText;

	public TMP_Text startText;

	public TMP_Text leftStickText;

	public TMP_Text rightStickText;

	public void ClearAllGamepadText(MessageBundle uiBundle)
	{
		ClearGamepadText(uiBundle, aText, isStick: false, InputControlType.Action1);
		ClearGamepadText(uiBundle, bText, isStick: false, InputControlType.Action2);
		ClearGamepadText(uiBundle, xText, isStick: false, InputControlType.Action3);
		ClearGamepadText(uiBundle, yText, isStick: false, InputControlType.Action4);
		ClearGamepadText(uiBundle, ltText, isStick: false, InputControlType.LeftTrigger);
		ClearGamepadText(uiBundle, lbText, isStick: false, InputControlType.LeftBumper);
		ClearGamepadText(uiBundle, rtText, isStick: false, InputControlType.RightTrigger);
		ClearGamepadText(uiBundle, rbText, isStick: false, InputControlType.RightBumper);
		ClearGamepadText(uiBundle, upText, isStick: false, InputControlType.DPadUp);
		ClearGamepadText(uiBundle, rightText, isStick: false, InputControlType.DPadRight);
		ClearGamepadText(uiBundle, downText, isStick: false, InputControlType.DPadDown);
		ClearGamepadText(uiBundle, leftText, isStick: false, InputControlType.DPadLeft);
		ClearGamepadText(uiBundle, backText, isStick: false, InputControlType.Back);
		ClearGamepadText(uiBundle, startText, isStick: false, InputControlType.Start);
		ClearGamepadText(uiBundle, leftStickText, isStick: true, InputControlType.LeftStickButton);
		ClearGamepadText(uiBundle, rightStickText, isStick: true, InputControlType.RightStickButton);
	}

	private void ClearGamepadText(MessageBundle uiBundle, TMP_Text text, bool isStick, InputControlType key)
	{
		if (isStick)
		{
			text.text = uiBundle.Get("m.gamepad_stick", XlateKeyText.XlateKey((key == InputControlType.LeftStickButton) ? "LeftStick" : "RightStick"), uiBundle.Get(((key == InputControlType.LeftStickButton) ^ SRSingleton<GameContext>.Instance.InputDirector.GetSwapSticks()) ? "l.move" : "l.view"), uiBundle.Get("l.none"), uiBundle.Get(string.Format("l.gamepad_{0}_stick_press_action", (key == InputControlType.LeftStickButton) ? "left" : "right")));
			return;
		}
		if (text == null)
		{
			Log.Error("Test was null!");
		}
		if (uiBundle == null)
		{
			Log.Error("uiBundle was null!");
		}
		text.text = uiBundle.Get("m.gamepad_button", XlateKeyText.XlateKey(key), uiBundle.Get("l.none"));
	}

	public TMP_Text GetTextForGamepadKey(InputControlType btn)
	{
		switch (btn)
		{
		case InputControlType.Action1:
			return aText;
		case InputControlType.Action2:
			return bText;
		case InputControlType.Action3:
			return xText;
		case InputControlType.Action4:
			return yText;
		case InputControlType.LeftBumper:
			return lbText;
		case InputControlType.LeftTrigger:
			return ltText;
		case InputControlType.RightBumper:
			return rbText;
		case InputControlType.RightTrigger:
			return rtText;
		case InputControlType.Back:
		case InputControlType.Share:
		case InputControlType.View:
			return backText;
		case InputControlType.Start:
		case InputControlType.Options:
		case InputControlType.Menu:
			return startText;
		case InputControlType.DPadUp:
			return upText;
		case InputControlType.DPadRight:
			return rightText;
		case InputControlType.DPadDown:
			return downText;
		case InputControlType.DPadLeft:
			return leftText;
		default:
			return null;
		}
	}

	public TMP_Text GetTextForGamepadStickKey(InputControlType key)
	{
		switch (key)
		{
		case InputControlType.LeftStickButton:
			return leftStickText;
		case InputControlType.RightStickButton:
			return rightStickText;
		default:
			return null;
		}
	}
}
