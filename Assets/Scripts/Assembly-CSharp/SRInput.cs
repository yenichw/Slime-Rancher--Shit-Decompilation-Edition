using System.Collections.ObjectModel;
using InControl;
using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Utility;
using UnityEngine;

public class SRInput
{
	public enum ButtonType
	{
		PRIMARY = 0,
		SECONDARY = 1,
		GAMEPAD = 2,
		GAMEPAD_SEC = 3
	}

	public class PlayerLookActions : PlayerActionSet
	{
		public PlayerAction lookXPos;

		public PlayerAction lookXNeg;

		public PlayerOneAxisAction lookX;

		public PlayerAction lookYPos;

		public PlayerAction lookYNeg;

		public PlayerOneAxisAction lookY;

		public PlayerLookActions()
		{
			lookXNeg = CreatePlayerAction("LookXNeg");
			lookXPos = CreatePlayerAction("LookXPos");
			lookX = CreateOneAxisPlayerAction(lookXNeg, lookXPos);
			lookYNeg = CreatePlayerAction("LookYNeg");
			lookYPos = CreatePlayerAction("LookYPos");
			lookY = CreateOneAxisPlayerAction(lookYNeg, lookYPos);
		}

		public Vector2 GetMouseLook()
		{
			return new Vector2(lookX, lookY);
		}

		public Vector2 GetMouseLookRaw()
		{
			return new Vector2(lookX.RawValue, lookY.RawValue);
		}
	}

	public class PlayerActions : PlayerLookActions
	{
		public PlayerAction attack;

		public PlayerAction vac;

		public PlayerAction slimeFilter;

		public PlayerAction jump;

		public PlayerAction run;

		public PlayerAction interact;

		public PlayerAction accept;

		public PlayerAction menu;

		public PlayerAction radarToggle;

		public PlayerAction openMap;

		public PlayerAction pedia;

		public PlayerAction reportIssue;

		public PlayerAction screenshot;

		public PlayerAction recordGif;

		public PlayerAction verticalNeg;

		public PlayerAction verticalPos;

		public PlayerOneAxisAction vertical;

		public PlayerAction horizontalNeg;

		public PlayerAction horizontalPos;

		public PlayerOneAxisAction horizontal;

		public PlayerAction slot1;

		public PlayerAction slot2;

		public PlayerAction slot3;

		public PlayerAction slot4;

		public PlayerAction slot5;

		public PlayerAction prevSlot;

		public PlayerAction nextSlot;

		public PlayerAction light;

		public PlayerAction burst;

		public PlayerAction toggleGadgetMode;

		public PlayerActions(SRInput srInput)
		{
			attack = CreatePlayerAction("Attack");
			vac = CreatePlayerAction("Vac");
			slimeFilter = CreatePlayerAction("SlimeFilter");
			jump = CreatePlayerAction("Jump");
			run = CreatePlayerAction("Run");
			interact = CreatePlayerAction("Interact");
			accept = CreatePlayerAction("Accept1");
			menu = CreatePlayerAction("Menu");
			radarToggle = CreatePlayerAction("RadarToggle");
			openMap = CreatePlayerAction("OpenMap");
			pedia = CreatePlayerAction("Pedia");
			reportIssue = CreatePlayerAction("ReportIssue");
			screenshot = CreatePlayerAction("Screenshot");
			recordGif = CreatePlayerAction("RecordGif");
			verticalNeg = CreatePlayerAction("VerticalNeg");
			verticalPos = CreatePlayerAction("VerticalPos");
			vertical = CreateOneAxisPlayerAction(verticalNeg, verticalPos);
			horizontalNeg = CreatePlayerAction("HorizontalNeg");
			horizontalPos = CreatePlayerAction("HorizontalPos");
			horizontal = CreateOneAxisPlayerAction(horizontalNeg, horizontalPos);
			slot1 = CreatePlayerAction("Slot1");
			slot2 = CreatePlayerAction("Slot2");
			slot3 = CreatePlayerAction("Slot3");
			slot4 = CreatePlayerAction("Slot4");
			slot5 = CreatePlayerAction("Slot5");
			prevSlot = CreatePlayerAction("PrevSlot");
			nextSlot = CreatePlayerAction("NextSlot");
			light = CreatePlayerAction("Light");
			burst = CreatePlayerAction("Burst");
			toggleGadgetMode = CreatePlayerAction("ToggleGadgetMode");
			base.ListenOptions.IncludeMouseButtons = true;
			base.ListenOptions.IncludeModifiersAsFirstClassKeys = true;
			base.ListenOptions.UnsetDuplicateBindingsOnSet = true;
			base.ListenOptions.DisallowBindingKeys = srInput.PROTECTED_KEYS;
			base.ListenOptions.DisallowBindingControls = srInput.PROTECTED_CONTROLS;
		}
	}

	public class PlayerPauseActions : PlayerActionSet
	{
		public PlayerAction submit;

		public PlayerAction altSubmit;

		public PlayerAction cancel;

		public PlayerAction menuUp;

		public PlayerAction menuDown;

		public PlayerAction menuLeft;

		public PlayerAction menuRight;

		public PlayerAction menuTabLeft;

		public PlayerAction menuTabRight;

		public PlayerAction menuScrollUp;

		public PlayerAction menuScrollDown;

		public PlayerAction unmenu;

		public PlayerAction closeMap;

		public PlayerAction switchUser;

		public PlayerPauseActions()
		{
			submit = CreatePlayerAction("Submit");
			altSubmit = CreatePlayerAction("AltSubmit");
			cancel = CreatePlayerAction("Cancel");
			unmenu = CreatePlayerAction("Unmenu");
			menuUp = CreatePlayerAction("MenuUp");
			menuDown = CreatePlayerAction("MenuDown");
			menuLeft = CreatePlayerAction("MenuLeft");
			menuRight = CreatePlayerAction("MenuRight");
			menuTabLeft = CreatePlayerAction("MenuTabLeft");
			menuTabRight = CreatePlayerAction("MenuTabRight");
			menuScrollUp = CreatePlayerAction("MenuScrollUp");
			menuScrollDown = CreatePlayerAction("MenuScrollDown");
			closeMap = CreatePlayerAction("CloseMap");
			switchUser = CreatePlayerAction("SwitchUser");
		}
	}

	public class PlayerEngageActions : PlayerActionSet
	{
		public PlayerAction engage;

		public PlayerEngageActions()
		{
			engage = CreatePlayerAction("Engage");
		}
	}

	public enum InputMode
	{
		NONE = 0,
		DEFAULT = 1,
		PAUSE = 2,
		ENGAGEMENT = 3,
		LOOK_ONLY = 4
	}

	public PlayerActions actions;

	public PlayerPauseActions pauseActions;

	public PlayerEngageActions engageActions;

	public PlayerLookActions lookActions;

	public readonly Key[] PROTECTED_KEYS;

	public readonly InputControlType[] PROTECTED_CONTROLS;

	public readonly PlayerAction[] PROTECTED_ACTIONS;

	protected static SRInput instance;

	private InputModeStack inputModeStack = new InputModeStack();

	public static SRInput Instance
	{
		get
		{
			if (instance == null && Application.isPlaying)
			{
				instance = new SRInput();
			}
			return instance;
		}
	}

	public static PlayerActions Actions => Instance.actions;

	public static PlayerPauseActions PauseActions => Instance.pauseActions;

	public static PlayerEngageActions EngageActions => Instance.engageActions;

	public static PlayerLookActions LookActions => Instance.lookActions;

	private SRInput()
	{
		PROTECTED_KEYS = new Key[2]
		{
			Key.Pause,
			GetDefaultMenuKey()
		};
		PROTECTED_CONTROLS = new InputControlType[2]
		{
			InputControlType.Start,
			InputControlType.Menu
		};
		actions = new PlayerActions(this);
		pauseActions = new PlayerPauseActions();
		engageActions = new PlayerEngageActions();
		lookActions = new PlayerLookActions();
		SetInputMode(InputMode.DEFAULT, 0);
		PROTECTED_ACTIONS = new PlayerAction[2] { actions.menu, pauseActions.unmenu };
	}

	public static BindingSource GetPrimKeyBinding(PlayerAction action)
	{
		ReadOnlyCollection<BindingSource> readOnlyCollection = action.BindingsOfTypes(BindingSourceType.KeyBindingSource, BindingSourceType.MouseBindingSource);
		if (readOnlyCollection.Count >= 1)
		{
			return readOnlyCollection[0];
		}
		return null;
	}

	public static BindingSource GetSecKeyBinding(PlayerAction action)
	{
		ReadOnlyCollection<BindingSource> readOnlyCollection = action.BindingsOfTypes(BindingSourceType.KeyBindingSource, BindingSourceType.MouseBindingSource);
		if (readOnlyCollection.Count >= 2)
		{
			return readOnlyCollection[1];
		}
		return null;
	}

	public static BindingSource GetPrimGamepadBinding(PlayerAction action)
	{
		ReadOnlyCollection<BindingSource> readOnlyCollection = action.BindingsOfTypes(BindingSourceType.DeviceBindingSource);
		if (readOnlyCollection.Count >= 1)
		{
			return readOnlyCollection[0];
		}
		return null;
	}

	public static BindingSource GetSecGamepadBinding(PlayerAction action)
	{
		ReadOnlyCollection<BindingSource> readOnlyCollection = action.BindingsOfTypes(BindingSourceType.DeviceBindingSource);
		if (readOnlyCollection.Count >= 2)
		{
			return readOnlyCollection[1];
		}
		return null;
	}

	public static BindingSource GetBinding(PlayerAction action, ButtonType type)
	{
		switch (type)
		{
		case ButtonType.PRIMARY:
			return GetPrimKeyBinding(action);
		case ButtonType.SECONDARY:
			return GetSecKeyBinding(action);
		case ButtonType.GAMEPAD:
			return GetPrimGamepadBinding(action);
		case ButtonType.GAMEPAD_SEC:
			return GetSecGamepadBinding(action);
		default:
			return null;
		}
	}

	public static string GetButtonKey(PlayerAction action, ButtonType type)
	{
		BindingSource binding = GetBinding(action, type);
		if (binding is KeyBindingSource)
		{
			return (binding as KeyBindingSource).Control.GetInclude(0).ToString();
		}
		if (binding is MouseBindingSource)
		{
			return (binding as MouseBindingSource).Control.ToString();
		}
		if (binding is DeviceBindingSource)
		{
			return (binding as DeviceBindingSource).Control.ToString();
		}
		return null;
	}

	public static bool IsProtected(PlayerAction action)
	{
		PlayerAction[] pROTECTED_ACTIONS = Instance.PROTECTED_ACTIONS;
		for (int i = 0; i < pROTECTED_ACTIONS.Length; i++)
		{
			if (pROTECTED_ACTIONS[i] == action)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsProtected(params Key[] keys)
	{
		foreach (Key key in keys)
		{
			Key[] pROTECTED_KEYS = Instance.PROTECTED_KEYS;
			for (int j = 0; j < pROTECTED_KEYS.Length; j++)
			{
				if (pROTECTED_KEYS[j] == key)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static PlayerAction GetAction(string actionName)
	{
		PlayerAction playerAction = Actions.Get(actionName);
		if (playerAction == null)
		{
			playerAction = PauseActions.Get(actionName);
		}
		return playerAction;
	}

	public static Key GetDefaultMenuKey()
	{
		if (!Application.isEditor)
		{
			return Key.Escape;
		}
		return Key.Backquote;
	}

	public void SetInputMode(InputMode mode, int handle)
	{
		if (inputModeStack.Push(mode, handle))
		{
			SetInputMode(mode);
			return;
		}
		Log.Error("Failed to set input mode!", "mode", mode, "handle", handle);
	}

	public void ClearInputMode(int handle)
	{
		inputModeStack.Pop(handle);
		InputMode inputMode = inputModeStack.Peek();
		if (inputMode != GetInputMode())
		{
			SetInputMode(inputMode);
		}
	}

	private void SetInputMode(InputMode mode)
	{
		actions.Enabled = mode == InputMode.DEFAULT;
		pauseActions.Enabled = mode == InputMode.PAUSE;
		engageActions.Enabled = mode == InputMode.ENGAGEMENT;
		lookActions.Enabled = mode == InputMode.LOOK_ONLY;
		Log.Debug("Setting input mode", "mode", mode);
	}

	public InputMode GetInputMode()
	{
		if (!actions.Enabled)
		{
			if (!pauseActions.Enabled)
			{
				if (!engageActions.Enabled)
				{
					if (!lookActions.Enabled)
					{
						return InputMode.NONE;
					}
					return InputMode.LOOK_ONLY;
				}
				return InputMode.ENGAGEMENT;
			}
			return InputMode.PAUSE;
		}
		return InputMode.DEFAULT;
	}

	public Vector2 GetMouseLook()
	{
		return Actions.GetMouseLook() + LookActions.GetMouseLook();
	}

	public Vector2 GetMouseLookRaw()
	{
		return Actions.GetMouseLookRaw() + LookActions.GetMouseLookRaw();
	}

	public static BindingV01 ToBinding(PlayerAction action)
	{
		BindingV01 bindingV = new BindingV01();
		bindingV.action = action.Name;
		BindingSource primKeyBinding = GetPrimKeyBinding(action);
		if (primKeyBinding is KeyBindingSource)
		{
			bindingV.primKey = (int)((KeyBindingSource)primKeyBinding).Control.GetInclude(0);
		}
		else if (primKeyBinding is MouseBindingSource)
		{
			bindingV.primMouse = (int)((MouseBindingSource)primKeyBinding).Control;
		}
		BindingSource secKeyBinding = GetSecKeyBinding(action);
		if (secKeyBinding is KeyBindingSource)
		{
			bindingV.secKey = (int)((KeyBindingSource)secKeyBinding).Control.GetInclude(0);
		}
		else if (secKeyBinding is MouseBindingSource)
		{
			bindingV.secMouse = (int)((MouseBindingSource)secKeyBinding).Control;
		}
		BindingSource primGamepadBinding = GetPrimGamepadBinding(action);
		if (primGamepadBinding is DeviceBindingSource)
		{
			bindingV.gamepad = (int)((DeviceBindingSource)primGamepadBinding).Control;
		}
		return bindingV;
	}

	public static bool AddOrReplaceBinding(PlayerAction action, PlayerAction source)
	{
		return AddOrReplaceBinding(action, ToBinding(source));
	}

	public static bool AddOrReplaceBinding(PlayerAction action, BindingV01 binding)
	{
		if (IsProtected(action))
		{
			Log.Warning("Ignoring key override for protected binding, using defaults.", "binding", binding.action);
			return false;
		}
		if (IsProtected((Key)binding.primKey, (Key)binding.secKey))
		{
			Log.Warning("Ignoring key override for protected key, using defaults.", "binding", binding.action, "binding.primKey", binding.primKey, "binding.secKey", binding.secKey);
			return false;
		}
		if (binding.primKey != 0)
		{
			action.AddOrReplaceBinding(GetPrimKeyBinding(action), new KeyBindingSource((Key)binding.primKey));
		}
		else if (binding.primMouse != 0)
		{
			action.AddOrReplaceBinding(GetPrimKeyBinding(action), new MouseBindingSource((Mouse)binding.primMouse));
		}
		else
		{
			action.AddOrReplaceBinding(GetPrimKeyBinding(action), null);
		}
		if (binding.secKey != 0)
		{
			action.AddOrReplaceBinding(GetSecKeyBinding(action), new KeyBindingSource((Key)binding.secKey));
		}
		else if (binding.secMouse != 0)
		{
			action.AddOrReplaceBinding(GetSecKeyBinding(action), new MouseBindingSource((Mouse)binding.secMouse));
		}
		else
		{
			action.AddOrReplaceBinding(GetSecKeyBinding(action), null);
		}
		if (binding.gamepad != 0)
		{
			action.AddOrReplaceBinding(GetPrimGamepadBinding(action), new DeviceBindingSource((InputControlType)binding.gamepad));
		}
		else
		{
			action.AddOrReplaceBinding(GetPrimGamepadBinding(action), null);
		}
		return true;
	}
}
