using System;
using System.Collections.Generic;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamepadPanel : MonoBehaviour
{
	private bool initializing;

	public Toggle disableGamepadToggle;

	public Toggle swapSticksToggle;

	public Toggle invertGamepadLookYToggle;

	public GameObject swapStickToggleLabel;

	public GameObject ps4SwapSticksToggleLabel;

	public Slider lookSensitivityXSlider;

	public Slider lookSensitivityYSlider;

	public Slider deadZoneSlider;

	public Button defaultGamepadBtn;

	public GameObject openChangeGamepadSettingsButton;

	private Dictionary<BindingLineUI, string> labelKeyDict = new Dictionary<BindingLineUI, string>();

	public GameObject bindingGamepadLinePrefab;

	public GameObject bindingsGamepadPanel;

	public GameObject rightPanel;

	public GameObject gamepadSettingsPrefab;

	public OptionsUI optionsUi;

	public GamepadVisualPanel defaultGamepadVisualPanel;

	public GamepadVisualPanel ps4GamepadVisualPanel;

	public GameObject standardPanel;

	public GameObject steamPanel;

	private MessageBundle uiBundle;

	private InputDirector inputDir;

	public void Awake()
	{
		inputDir = SRSingleton<GameContext>.Instance.InputDirector;
		ps4SwapSticksToggleLabel.SetActive(value: false);
		swapStickToggleLabel.SetActive(value: true);
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
	}

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	private GamepadVisualPanel GetVisualPanel()
	{
		return defaultGamepadVisualPanel;
	}

	private void OnBundlesAvailable(MessageDirector msgDir)
	{
		initializing = true;
		uiBundle = msgDir.GetBundle("ui");
		SetupBindings();
		disableGamepadToggle.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetDisableGamepad();
		SetGamepadControlsInteractable(!disableGamepadToggle.isOn);
		swapSticksToggle.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetSwapSticks();
		invertGamepadLookYToggle.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetInvertGamepadLookY();
		lookSensitivityXSlider.value = SRSingleton<GameContext>.Instance.InputDirector.GamepadLookSensitivityX;
		lookSensitivityYSlider.value = SRSingleton<GameContext>.Instance.InputDirector.GamepadLookSensitivityY;
		deadZoneSlider.value = SRSingleton<GameContext>.Instance.InputDirector.ControllerStickDeadZone * 10f;
		disableGamepadToggle.gameObject.SetActive(value: true);
		defaultGamepadBtn.gameObject.SetActive(value: true);
		rightPanel.SetActive(value: true);
		openChangeGamepadSettingsButton.SetActive(value: false);
		initializing = false;
		RefreshBindings();
		Update();
	}

	public void Update()
	{
		bool flag = inputDir.UsingSteamController();
		standardPanel.SetActive(!flag);
		steamPanel.SetActive(flag);
		defaultGamepadVisualPanel.gameObject.SetActive(value: true);
		ps4GamepadVisualPanel.gameObject.SetActive(value: false);
	}

	private void SetupBindings()
	{
		Selectable selectable = deadZoneSlider;
		for (int i = 0; i < bindingsGamepadPanel.transform.childCount; i++)
		{
			Destroyer.Destroy(bindingsGamepadPanel.transform.GetChild(i).gameObject, "GamepadPanel.SetupBindings");
		}
		while (bindingsGamepadPanel.transform.childCount > 0)
		{
			bindingsGamepadPanel.transform.GetChild(0).SetParent(null, worldPositionStays: false);
		}
		CreateGamepadBindingLine("key.shoot", SRInput.Actions.attack);
		CreateGamepadBindingLine("key.vac", SRInput.Actions.vac);
		CreateGamepadBindingLine("key.burst", SRInput.Actions.burst);
		CreateGamepadBindingLine("key.jump", SRInput.Actions.jump);
		CreateGamepadBindingLine("key.run", SRInput.Actions.run);
		CreateGamepadBindingLine("key.interact", SRInput.Actions.interact);
		CreateGamepadBindingLine("key.gadgetMode", SRInput.Actions.toggleGadgetMode);
		CreateGamepadBindingLine("key.flashlight", SRInput.Actions.light);
		CreateGamepadBindingLine("key.radar", SRInput.Actions.radarToggle);
		CreateGamepadBindingLine("key.map", SRInput.Actions.openMap);
		CreateGamepadBindingLine("key.slot_1", SRInput.Actions.slot1);
		CreateGamepadBindingLine("key.slot_2", SRInput.Actions.slot2);
		CreateGamepadBindingLine("key.slot_3", SRInput.Actions.slot3);
		CreateGamepadBindingLine("key.slot_4", SRInput.Actions.slot4);
		CreateGamepadBindingLine("key.slot_5", SRInput.Actions.slot5);
		CreateGamepadBindingLine("key.prev_slot", SRInput.Actions.prevSlot);
		CreateGamepadBindingLine("key.next_slot", SRInput.Actions.nextSlot);
		CreateGamepadBindingLine("key.reportissue", SRInput.Actions.reportIssue);
		CreateGamepadBindingLine("key.screenshot", SRInput.Actions.screenshot);
		CreateGamepadBindingLine("key.recordgif", SRInput.Actions.recordGif);
		CreateGamepadBindingLine("key.pedia", SRInput.Actions.pedia);
		Button[] componentsInChildren = bindingsGamepadPanel.GetComponentsInChildren<Button>(includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (j < componentsInChildren.Length - 1)
			{
				navigation.selectOnDown = componentsInChildren[j + 1];
			}
			else
			{
				navigation.selectOnDown = defaultGamepadBtn;
				Navigation navigation2 = defaultGamepadBtn.navigation;
				navigation2.mode = Navigation.Mode.Explicit;
				navigation2.selectOnUp = componentsInChildren[j];
				defaultGamepadBtn.navigation = navigation2;
			}
			if (j > 0)
			{
				navigation.selectOnUp = componentsInChildren[j - 1];
			}
			else
			{
				navigation.selectOnUp = selectable;
				Navigation navigation3 = selectable.navigation;
				navigation3.mode = Navigation.Mode.Explicit;
				navigation3.selectOnDown = componentsInChildren[j];
				selectable.navigation = navigation3;
			}
			navigation.selectOnLeft = disableGamepadToggle;
			componentsInChildren[j].navigation = navigation;
		}
	}

	public void ToggleDisableGamepad()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetDisableGamepad(disableGamepadToggle.isOn);
		SetGamepadControlsInteractable(!disableGamepadToggle.isOn);
	}

	public void ToggleInvertGamepadLookY()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetInvertGamepadLookY(invertGamepadLookYToggle.isOn);
	}

	public void ToggleSwapSticks()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetSwapSticks(swapSticksToggle.isOn);
		RefreshBindings();
	}

	public void OnLookSensitivityXChanged()
	{
		float value = lookSensitivityXSlider.value;
		SRSingleton<GameContext>.Instance.InputDirector.GamepadLookSensitivityX = value;
	}

	public void OnLookSensitivityYChanged()
	{
		float value = lookSensitivityYSlider.value;
		SRSingleton<GameContext>.Instance.InputDirector.GamepadLookSensitivityY = value;
	}

	public void OnDeadZoneChanged()
	{
		float controllerStickDeadZone = Mathf.Clamp(deadZoneSlider.value / 10f, 0f, 0.95f);
		SRSingleton<GameContext>.Instance.InputDirector.ControllerStickDeadZone = controllerStickDeadZone;
	}

	public void OnOpenChangeSettingsClicked()
	{
		BaseUI component = UnityEngine.Object.Instantiate(gamepadSettingsPrefab).GetComponent<BaseUI>();
		optionsUi.PreventClosing(prevent: true);
		base.gameObject.SetActive(value: false);
		component.onDestroy = (BaseUI.OnDestroyDelegate)Delegate.Combine(component.onDestroy, (BaseUI.OnDestroyDelegate)delegate
		{
			base.gameObject.SetActive(value: true);
			optionsUi.PreventClosing(prevent: false);
		});
	}

	public void RefreshBindings()
	{
		if (initializing || uiBundle == null)
		{
			return;
		}
		GetVisualPanel().ClearAllGamepadText(uiBundle);
		BindingLineUI[] componentsInChildren = GetComponentsInChildren<BindingLineUI>(includeInactive: true);
		foreach (BindingLineUI bindingLineUI in componentsInChildren)
		{
			bindingLineUI.Refresh();
			if (bindingLineUI.leftBtnMode == SRInput.ButtonType.GAMEPAD)
			{
				BindingSource binding = SRInput.GetBinding(bindingLineUI.action, bindingLineUI.leftBtnMode);
				UpdateGamepadBindingText(binding, bindingLineUI);
			}
		}
		GetVisualPanel().GetTextForGamepadKey(InputControlType.Start).text = uiBundle.Get("m.gamepad_button_pause", InputControlType.Start);
	}

	private GameObject CreateGamepadBindingLine(string label, PlayerAction action)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(bindingGamepadLinePrefab);
		gameObject.transform.SetParent(bindingsGamepadPanel.transform, worldPositionStays: false);
		BindingPanel.CreateBindingLine(label, action, gameObject, uiBundle, labelKeyDict, DisableGamepads);
		BindingLineUI component = gameObject.GetComponent<BindingLineUI>();
		if (component.leftBtnMode == SRInput.ButtonType.GAMEPAD)
		{
			BindingSource binding = SRInput.GetBinding(component.action, SRInput.ButtonType.GAMEPAD);
			UpdateGamepadBindingText(binding, component);
		}
		return gameObject;
	}

	private void SetGamepadControlsInteractable(bool interactable)
	{
		Button[] componentsInChildren = bindingsGamepadPanel.GetComponentsInChildren<Button>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].interactable = interactable;
		}
		swapSticksToggle.interactable = interactable;
		invertGamepadLookYToggle.interactable = interactable;
		lookSensitivityXSlider.interactable = interactable;
		lookSensitivityYSlider.interactable = interactable;
		deadZoneSlider.interactable = interactable;
	}

	private void UpdateGamepadBindingText(BindingSource bindingSource, BindingLineUI binding)
	{
		InputControlType inputControlType = ((!(bindingSource == null)) ? (bindingSource as DeviceBindingSource).Control : InputControlType.None);
		GamepadVisualPanel visualPanel = GetVisualPanel();
		TMP_Text textForGamepadKey = visualPanel.GetTextForGamepadKey(inputControlType);
		TMP_Text textForGamepadStickKey = visualPanel.GetTextForGamepadStickKey(inputControlType);
		if (textForGamepadKey != null)
		{
			textForGamepadKey.text = uiBundle.Get("m.gamepad_button", XlateKeyText.XlateKey(inputControlType), uiBundle.Get(labelKeyDict[binding]));
		}
		else if (textForGamepadStickKey != null)
		{
			textForGamepadStickKey.text = uiBundle.Get("m.gamepad_stick", XlateKeyText.XlateKey((inputControlType == InputControlType.LeftStickButton) ? "LeftStick" : "RightStick"), uiBundle.Get(((inputControlType == InputControlType.LeftStickButton) ^ SRSingleton<GameContext>.Instance.InputDirector.GetSwapSticks()) ? "l.move" : "l.view"), uiBundle.Get(labelKeyDict[binding]), uiBundle.Get(string.Format("l.gamepad_{0}_stick_press_action", (inputControlType == InputControlType.LeftStickButton) ? "left" : "right")));
		}
	}

	private int InputControlTypeToSpriteId(InputControlType btn)
	{
		switch (btn)
		{
		case InputControlType.Action1:
			return 0;
		case InputControlType.Action2:
			return 1;
		case InputControlType.Action3:
			return 2;
		case InputControlType.Action4:
			return 3;
		default:
			return 0;
		}
	}

	private bool DisableGamepads()
	{
		return disableGamepadToggle.isOn;
	}

	public void ResetGamepadDefaults()
	{
		SRSingleton<GameContext>.Instance.InputDirector.ResetGamepadDefaults();
		SRSingleton<GameContext>.Instance.InputDirector.SetInvertGamepadLookY(invertGamepadLookYToggle.isOn);
		RefreshBindings();
	}

	public void ShowSteamControllerConfig()
	{
		inputDir.ShowSteamControllerConfig();
	}
}
