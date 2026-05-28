using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class InputDirector : SRBehaviour
{
	public class ScalableMouseBindingSource : MouseBindingSource
	{
		private float sensitivity;

		public ScalableMouseBindingSource(Mouse control, float sensitivity)
			: base(control)
		{
			this.sensitivity = sensitivity;
		}

		public override float GetValue(InputDevice device)
		{
			return base.GetValue(device) * sensitivity;
		}
	}

	public delegate void OnKeysChanged();

	private class DefaultBinding
	{
		public PlayerAction bindTo;

		public Mouse primMouse;

		public Key primKey;

		public Key secKey;

		public InputControlType primBtn;

		public InputControlType secBtn;

		public InputControlType tertBtn;

		public DefaultBinding(PlayerAction bindTo, Key primKey, Key secKey, InputControlType primBtn, InputControlType secBtn = InputControlType.None, InputControlType tertBtn = InputControlType.None)
		{
			this.bindTo = bindTo;
			this.primKey = primKey;
			this.secKey = secKey;
			this.primBtn = primBtn;
			this.secBtn = secBtn;
			this.tertBtn = tertBtn;
		}

		public DefaultBinding(PlayerAction bindTo, Mouse primMouse, Key secKey, InputControlType primBtn, InputControlType secBtn = InputControlType.None, InputControlType tertBtn = InputControlType.None)
		{
			this.bindTo = bindTo;
			this.primMouse = primMouse;
			this.secKey = secKey;
			this.primBtn = primBtn;
			this.secBtn = secBtn;
			this.tertBtn = tertBtn;
		}

		public void ApplyDefaultBinding()
		{
			bindTo.ClearBindings();
			if (primKey != 0)
			{
				bindTo.AddDefaultBinding(primKey);
			}
			if (primMouse != 0)
			{
				bindTo.AddDefaultBinding(primMouse);
			}
			if (secKey != 0)
			{
				bindTo.AddDefaultBinding(secKey);
			}
			if (primBtn != 0)
			{
				bindTo.AddDefaultBinding(primBtn);
			}
			if (secBtn != 0)
			{
				bindTo.AddDefaultBinding(secBtn);
			}
			if (tertBtn != 0)
			{
				bindTo.AddDefaultBinding(tertBtn);
			}
		}
	}

	public OnKeysChanged onKeysChanged;

	public GameObject bugReportPrefab;

	private float mouseLookSensitivity;

	private float gamepadLookSensitivityX;

	private float gamepadLookSensitivityY = -0.2f;

	private float gamepadLookSensitivityXFactor = 1f;

	private float gamepadLookSensitivityYFactor = 1f;

	private float mouseLookSensitivityFactor = 1f;

	private float controllerStickDeadZone;

	private bool oldUsingGamepad;

	private bool swapSticks;

	private bool invertGamepadLookY;

	private bool invertMouseLookY;

	private bool disableMouseLookSmooth;

	private SRInput input;

	private HashSet<KeyCode> protectedKeyCodes = new HashSet<KeyCode>();

	private HashSet<string> protectedButtons = new HashSet<string>();

	private static DefaultBinding[] DEFAULTS;

	private static DefaultBinding[] EDITOR_DEFAULTS;

	public float ControllerStickDeadZone
	{
		get
		{
			return controllerStickDeadZone;
		}
		set
		{
			controllerStickDeadZone = value;
		}
	}

	public float MouseLookSensitivity
	{
		get
		{
			return mouseLookSensitivity;
		}
		set
		{
			mouseLookSensitivity = value;
			NoteNewPlayer(SRSingleton<SceneContext>.Instance.Player);
		}
	}

	public float GamepadLookSensitivityX
	{
		get
		{
			return gamepadLookSensitivityX;
		}
		set
		{
			gamepadLookSensitivityX = value;
			NoteNewPlayer(SRSingleton<SceneContext>.Instance.Player);
		}
	}

	public float GamepadLookSensitivityY
	{
		get
		{
			return gamepadLookSensitivityY;
		}
		set
		{
			gamepadLookSensitivityY = value;
			NoteNewPlayer(SRSingleton<SceneContext>.Instance.Player);
		}
	}

	public void Awake()
	{
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
		input = SRInput.Instance;
		InitBindings();
		vp_Utility.SetUsingGamepad(UsingGamepad());
	}

	public void OnDestroy()
	{
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Remove(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
	}

	private void OnBindingAdded(PlayerAction action, BindingSource binding)
	{
		NoteKeysChanged();
	}

	public void Update()
	{
		if (SRInput.Actions.reportIssue.WasPressed)
		{
			UnityEngine.Object.Instantiate(bugReportPrefab);
		}
		else if (SRInput.Actions.screenshot.WasPressed)
		{
			SRSingleton<GameContext>.Instance.TakeScreenshot();
		}
		else if (SRInput.Actions.recordGif.WasPressed)
		{
			SRSingleton<GameContext>.Instance.TakeGifScreenshot();
		}
		if (Mathf.Abs(Input.GetAxisRaw("mouse x")) > Mathf.Epsilon || Mathf.Abs(Input.GetAxisRaw("mouse y")) > Mathf.Epsilon)
		{
			SRInput.Actions.LastInputType = BindingSourceType.MouseBindingSource;
			SRInput.PauseActions.LastInputType = BindingSourceType.MouseBindingSource;
			SRInput.LookActions.LastInputType = BindingSourceType.MouseBindingSource;
		}
		bool flag = UsingGamepad();
		if (oldUsingGamepad != flag)
		{
			NoteKeysChanged();
			vp_Utility.SetUsingGamepad(flag);
		}
		oldUsingGamepad = flag;
		switch (input.GetInputMode())
		{
		case SRInput.InputMode.PAUSE:
			if (Time.timeScale != 0f && !Levels.isSpecialNonAlloc())
			{
				input.ClearInputMode(base.gameObject.GetInstanceID());
			}
			break;
		case SRInput.InputMode.DEFAULT:
			if (Time.timeScale == 0f || Levels.isSpecialNonAlloc())
			{
				input.SetInputMode(SRInput.InputMode.PAUSE, base.gameObject.GetInstanceID());
			}
			break;
		}
	}

	public bool IsProtected(string button)
	{
		return protectedButtons.Contains(button);
	}

	public bool IsProtected(KeyCode key)
	{
		return protectedKeyCodes.Contains(key);
	}

	private void InitBindings()
	{
		SetDefaultBindings();
	}

	private void InitializeDefaultBindings()
	{
		if (DEFAULTS == null)
		{
			DEFAULTS = new DefaultBinding[51]
			{
				new DefaultBinding(SRInput.Actions.verticalPos, Key.W, Key.UpArrow, InputControlType.LeftStickUp),
				new DefaultBinding(SRInput.Actions.verticalNeg, Key.S, Key.DownArrow, InputControlType.LeftStickDown),
				new DefaultBinding(SRInput.Actions.horizontalPos, Key.D, Key.RightArrow, InputControlType.LeftStickRight),
				new DefaultBinding(SRInput.Actions.horizontalNeg, Key.A, Key.LeftArrow, InputControlType.LeftStickLeft),
				new DefaultBinding(SRInput.Actions.lookYPos, Mouse.PositiveY, Key.None, InputControlType.RightStickDown),
				new DefaultBinding(SRInput.Actions.lookYNeg, Mouse.NegativeY, Key.None, InputControlType.RightStickUp),
				new DefaultBinding(SRInput.Actions.lookXPos, Mouse.PositiveX, Key.None, InputControlType.RightStickRight),
				new DefaultBinding(SRInput.Actions.lookXNeg, Mouse.NegativeX, Key.None, InputControlType.RightStickLeft),
				new DefaultBinding(SRInput.Actions.attack, Mouse.LeftButton, Key.None, InputControlType.RightTrigger),
				new DefaultBinding(SRInput.Actions.vac, Mouse.RightButton, Key.None, InputControlType.LeftTrigger),
				new DefaultBinding(SRInput.Actions.slimeFilter, Key.H, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.jump, Key.Space, Key.None, InputControlType.Action1),
				new DefaultBinding(SRInput.Actions.run, Key.LeftShift, Key.None, InputControlType.LeftStickButton),
				new DefaultBinding(SRInput.Actions.interact, Key.E, Key.None, InputControlType.Action3),
				new DefaultBinding(SRInput.Actions.accept, Key.Return, Key.PadEnter, InputControlType.None),
				new DefaultBinding(SRInput.Actions.menu, Key.Pause, SRInput.GetDefaultMenuKey(), InputControlType.Start, InputControlType.Options, InputControlType.Menu),
				new DefaultBinding(SRInput.Actions.radarToggle, Key.R, Key.None, InputControlType.RightStickButton),
				new DefaultBinding(SRInput.Actions.openMap, Key.M, Key.None, InputControlType.DPadRight),
				new DefaultBinding(SRInput.Actions.pedia, Key.F1, Key.Slash, InputControlType.DPadUp),
				new DefaultBinding(SRInput.Actions.reportIssue, Key.F2, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.screenshot, Key.Backslash, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.recordGif, Key.G, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.slot1, Key.Key1, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.slot2, Key.Key2, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.slot3, Key.Key3, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.slot4, Key.Key4, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.slot5, Key.Key5, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.Actions.light, Key.F, Key.None, InputControlType.Action4),
				new DefaultBinding(SRInput.Actions.burst, Mouse.MiddleButton, Key.Q, InputControlType.Action2),
				new DefaultBinding(SRInput.Actions.prevSlot, Mouse.PositiveScrollWheel, Key.None, InputControlType.LeftBumper),
				new DefaultBinding(SRInput.Actions.nextSlot, Mouse.NegativeScrollWheel, Key.None, InputControlType.RightBumper),
				new DefaultBinding(SRInput.Actions.toggleGadgetMode, Key.T, Key.None, InputControlType.DPadDown),
				new DefaultBinding(SRInput.PauseActions.submit, Key.Space, Key.None, InputControlType.Action1),
				new DefaultBinding(SRInput.PauseActions.altSubmit, Key.Space, Key.None, InputControlType.Action3),
				new DefaultBinding(SRInput.PauseActions.cancel, Key.Escape, Key.None, InputControlType.Action2),
				new DefaultBinding(SRInput.PauseActions.menuUp, Key.W, Key.UpArrow, InputControlType.DPadUp, InputControlType.LeftStickUp),
				new DefaultBinding(SRInput.PauseActions.menuDown, Key.S, Key.DownArrow, InputControlType.DPadDown, InputControlType.LeftStickDown),
				new DefaultBinding(SRInput.PauseActions.menuLeft, Key.A, Key.LeftArrow, InputControlType.DPadLeft, InputControlType.LeftStickLeft),
				new DefaultBinding(SRInput.PauseActions.menuRight, Key.D, Key.RightArrow, InputControlType.DPadRight, InputControlType.LeftStickRight),
				new DefaultBinding(SRInput.PauseActions.menuTabLeft, Key.Minus, Key.None, InputControlType.LeftBumper),
				new DefaultBinding(SRInput.PauseActions.menuTabRight, Key.Equals, Key.None, InputControlType.RightBumper),
				new DefaultBinding(SRInput.PauseActions.menuScrollUp, Key.PageUp, Key.None, InputControlType.RightStickUp),
				new DefaultBinding(SRInput.PauseActions.menuScrollDown, Key.PageDown, Key.None, InputControlType.RightStickDown),
				new DefaultBinding(SRInput.PauseActions.unmenu, Key.Pause, SRInput.GetDefaultMenuKey(), InputControlType.Start, InputControlType.Options, InputControlType.Menu),
				new DefaultBinding(SRInput.PauseActions.closeMap, Key.M, Key.None, InputControlType.None),
				new DefaultBinding(SRInput.EngageActions.engage, Key.None, Key.None, InputControlType.Menu),
				new DefaultBinding(SRInput.PauseActions.switchUser, Key.Space, Key.None, InputControlType.Action4),
				new DefaultBinding(SRInput.LookActions.lookYPos, Mouse.PositiveY, Key.None, InputControlType.RightStickDown),
				new DefaultBinding(SRInput.LookActions.lookYNeg, Mouse.NegativeY, Key.None, InputControlType.RightStickUp),
				new DefaultBinding(SRInput.LookActions.lookXPos, Mouse.PositiveX, Key.None, InputControlType.RightStickRight),
				new DefaultBinding(SRInput.LookActions.lookXNeg, Mouse.NegativeX, Key.None, InputControlType.RightStickLeft)
			};
		}
		if (EDITOR_DEFAULTS == null)
		{
			EDITOR_DEFAULTS = new DefaultBinding[1]
			{
				new DefaultBinding(SRInput.PauseActions.unmenu, Key.Pause, Key.Backquote, InputControlType.Start, InputControlType.Options, InputControlType.Menu)
			};
		}
	}

	private void SetDefaultBindings()
	{
		InitializeDefaultBindings();
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, (Action<PlayerAction, BindingSource>)delegate
		{
			SRSingleton<GameContext>.Instance.InputDirector.NoteKeysChanged();
		});
		DefaultBinding[] dEFAULTS = DEFAULTS;
		for (int i = 0; i < dEFAULTS.Length; i++)
		{
			dEFAULTS[i].ApplyDefaultBinding();
		}
		if (Application.isEditor)
		{
			dEFAULTS = EDITOR_DEFAULTS;
			for (int i = 0; i < dEFAULTS.Length; i++)
			{
				dEFAULTS[i].ApplyDefaultBinding();
			}
		}
		UpdateGamepadStickBindings();
		NoteKeysChanged();
	}

	public void ResetProfile()
	{
		mouseLookSensitivity = 0f;
		gamepadLookSensitivityX = 0f;
		gamepadLookSensitivityY = -0.2f;
		invertGamepadLookY = false;
		swapSticks = false;
		SetDefaultBindings();
	}

	public void ResetKeyMouseDefaults()
	{
		SRInput.Actions.ResetForTypes(BindingSourceType.MouseBindingSource, BindingSourceType.KeyBindingSource);
	}

	public void ResetGamepadDefaults()
	{
		SRInput.Actions.ResetForTypes(BindingSourceType.DeviceBindingSource);
	}

	public void NoteKeysChanged()
	{
		if (onKeysChanged != null)
		{
			onKeysChanged();
		}
	}

	public static bool UsingGamepad()
	{
		return SRInput.Actions.LastInputType == BindingSourceType.DeviceBindingSource;
	}

	public string GetActiveDeviceString(string actionStr, bool isPauseAction)
	{
		bool num = UsingGamepad();
		string text = null;
		PlayerAction action = SRInput.GetAction(actionStr);
		if (num)
		{
			return GetKeyStringForGamepad(action, actionStr);
		}
		return GetKeyStringForMouseKeyboard(action);
	}

	public Sprite GetActiveDeviceIcon(string actionStr, bool isPauseAction, out bool iconFound)
	{
		return GetDefaultDeviceIcon(actionStr, isPauseAction, out iconFound);
	}

	private Sprite GetDefaultDeviceIcon(string actionStr, bool isPauseAction, out bool iconFound)
	{
		string activeDeviceString = GetActiveDeviceString(actionStr, isPauseAction);
		InputDeviceStyle lastDeviceStyle = SRInput.Actions.LastDeviceStyle;
		return SRSingleton<GameContext>.Instance.UITemplates.GetButtonIcon(lastDeviceStyle, activeDeviceString, out iconFound);
	}

	private string GetKeyStringForGamepad(PlayerAction action, string actionStr)
	{
		if (action != null)
		{
			BindingSource primGamepadBinding = SRInput.GetPrimGamepadBinding(action);
			if (primGamepadBinding != null && primGamepadBinding is DeviceBindingSource)
			{
				return ((DeviceBindingSource)primGamepadBinding).Control.ToString();
			}
		}
		else
		{
			if (actionStr == (swapSticks ? "Look" : "Move"))
			{
				return "LeftStickMove";
			}
			if (actionStr == ((!swapSticks) ? "Look" : "Move"))
			{
				return "RightStickMove";
			}
		}
		return null;
	}

	private string GetKeyStringForMouseKeyboard(PlayerAction action)
	{
		if (action != null)
		{
			BindingSource bindingSource = SRInput.GetPrimKeyBinding(action);
			if (bindingSource == null)
			{
				bindingSource = SRInput.GetSecKeyBinding(action);
			}
			if (bindingSource != null && bindingSource is MouseBindingSource)
			{
				return ((MouseBindingSource)bindingSource).Control.ToString();
			}
			if (bindingSource != null && bindingSource is KeyBindingSource)
			{
				return ((KeyBindingSource)bindingSource).Control.ToString();
			}
		}
		return null;
	}

	public bool UsingSteamController()
	{
		return false;
	}

	public void ShowSteamControllerConfig()
	{
	}

	public bool GetSwapSticks()
	{
		return swapSticks;
	}

	public bool GetDisableGamepad()
	{
		return DeviceBindingSource.DevicesDisabled;
	}

	public void SetDisableGamepad(bool disable)
	{
		DeviceBindingSource.DevicesDisabled = disable;
	}

	public void SetSwapSticks(bool swap)
	{
		swapSticks = swap;
		UpdateGamepadStickBindings();
	}

	public bool GetInvertGamepadLookY()
	{
		return invertGamepadLookY;
	}

	public void SetInvertGamepadLookY(bool invert)
	{
		invertGamepadLookY = invert;
		UpdateGamepadStickBindings();
	}

	public bool GetInvertMouseLookY()
	{
		return invertMouseLookY;
	}

	public void SetInvertMouseLookY(bool invert)
	{
		invertMouseLookY = invert;
		UpdateMouseYAxis();
	}

	public bool GetDisableMouseLookSmooth()
	{
		return disableMouseLookSmooth;
	}

	public void SetDisableMouseLookSmooth(bool smooth)
	{
		disableMouseLookSmooth = smooth;
	}

	private void UpdateMouseYAxis()
	{
		UpdateMouseYAxis(SRInput.Actions);
		UpdateMouseYAxis(SRInput.LookActions);
	}

	private void UpdateMouseYAxis(SRInput.PlayerLookActions actions)
	{
		actions.lookXNeg.ClearBindingsOfTypes(BindingSourceType.MouseBindingSource);
		actions.lookXPos.ClearBindingsOfTypes(BindingSourceType.MouseBindingSource);
		actions.lookYNeg.ClearBindingsOfTypes(BindingSourceType.MouseBindingSource);
		actions.lookYPos.ClearBindingsOfTypes(BindingSourceType.MouseBindingSource);
		actions.lookXNeg.AddBinding(new ScalableMouseBindingSource(Mouse.NegativeX, mouseLookSensitivityFactor));
		actions.lookXPos.AddBinding(new ScalableMouseBindingSource(Mouse.PositiveX, mouseLookSensitivityFactor));
		actions.lookYNeg.AddBinding(new ScalableMouseBindingSource(invertMouseLookY ? Mouse.PositiveY : Mouse.NegativeY, mouseLookSensitivityFactor));
		actions.lookYPos.AddBinding(new ScalableMouseBindingSource(invertMouseLookY ? Mouse.NegativeY : Mouse.PositiveY, mouseLookSensitivityFactor));
	}

	private void UpdateGamepadStickBindings()
	{
		UpdateGamepadStickBindings(SRInput.Actions);
		UpdateGamepadStickBindings(SRInput.LookActions);
		SRInput.Actions.horizontalNeg.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		SRInput.Actions.horizontalPos.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		SRInput.Actions.verticalNeg.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		SRInput.Actions.verticalPos.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		if (swapSticks)
		{
			SRInput.Actions.horizontalNeg.AddBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			SRInput.Actions.horizontalPos.AddBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			SRInput.Actions.verticalNeg.AddBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			SRInput.Actions.verticalPos.AddBinding(new DeviceBindingSource(InputControlType.RightStickUp));
		}
		else
		{
			SRInput.Actions.horizontalNeg.AddBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			SRInput.Actions.horizontalPos.AddBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			SRInput.Actions.verticalNeg.AddBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			SRInput.Actions.verticalPos.AddBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
		}
	}

	private void UpdateGamepadStickBindings(SRInput.PlayerLookActions actions)
	{
		actions.lookXNeg.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		actions.lookXPos.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		actions.lookYNeg.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		actions.lookYPos.ClearBindingsOfTypes(BindingSourceType.DeviceBindingSource);
		if (swapSticks)
		{
			actions.lookXNeg.AddBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			actions.lookXPos.AddBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			actions.lookYNeg.AddBinding(new DeviceBindingSource(invertGamepadLookY ? InputControlType.LeftStickUp : InputControlType.LeftStickDown));
			actions.lookYPos.AddBinding(new DeviceBindingSource((!invertGamepadLookY) ? InputControlType.LeftStickUp : InputControlType.LeftStickDown));
		}
		else
		{
			actions.lookXNeg.AddBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			actions.lookXPos.AddBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			actions.lookYNeg.AddBinding(new DeviceBindingSource(invertGamepadLookY ? InputControlType.RightStickUp : InputControlType.RightStickDown));
			actions.lookYPos.AddBinding(new DeviceBindingSource(invertGamepadLookY ? InputControlType.RightStickDown : InputControlType.RightStickUp));
		}
	}

	public void NoteNewPlayer(GameObject player)
	{
		SetPlayerMouseSensitivity(player);
		SetPlayerGamepadXSensitivity(player);
		SetPlayerGamepadYSensitivity(player);
	}

	public float GetGamepadLookSensitivityXFactor()
	{
		return gamepadLookSensitivityXFactor;
	}

	public float GetGamepadLookSensitivityYFactor()
	{
		return gamepadLookSensitivityYFactor;
	}

	public void SetGamepadLookSensitivityXFactor(float factor)
	{
		gamepadLookSensitivityXFactor = factor;
		UpdateGamepadStickBindings();
	}

	public void SetGamepadLookSensitivityYFactor(float factor)
	{
		gamepadLookSensitivityYFactor = factor;
		UpdateGamepadStickBindings();
	}

	public void SetMouseLookSensitivityFactor(float factor)
	{
		mouseLookSensitivityFactor = factor;
		UpdateMouseYAxis();
	}

	private void SetPlayerMouseSensitivity(GameObject player)
	{
		if (player != null)
		{
			float num = Mathf.Pow(3f, mouseLookSensitivity);
			SetMouseLookSensitivityFactor(num);
		}
	}

	private void SetPlayerGamepadXSensitivity(GameObject player)
	{
		if (player != null)
		{
			float num = Mathf.Pow(3f, gamepadLookSensitivityX);
			SetGamepadLookSensitivityXFactor(num);
		}
	}

	private void SetPlayerGamepadYSensitivity(GameObject player)
	{
		if (player != null)
		{
			float num = Mathf.Pow(3f, gamepadLookSensitivityY);
			SetGamepadLookSensitivityYFactor(num);
		}
	}
}
