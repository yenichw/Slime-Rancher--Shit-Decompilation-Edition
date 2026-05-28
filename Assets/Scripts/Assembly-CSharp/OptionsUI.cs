using System;
using System.Collections.Generic;
using System.Globalization;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsUI : BaseUI
{
	private delegate void OnQualityChanged();

	public GameObject videoPanel;

	public GameObject audioPanel;

	public GameObject inputPanel;

	public GameObject gamepadPanel;

	public GameObject modsPanel;

	public GameObject otherPanel;

	public GameObject tabsPanel;

	public GameObject bindingLinePrefab;

	public GameObject bindingsPanel;

	public GameObject title;

	public Toggle disableCameraBobToggle;

	public TMP_Dropdown tutorialsDropdown;

	public Toggle bufferForGifToggle;

	public Toggle vacLockToggle;

	public Toggle sprintHoldToggle;

	public SECTR_AudioBus masterBus;

	public SECTR_AudioBus musicBus;

	public SECTR_AudioBus sfxBus;

	public Slider masterSlider;

	public Slider musicSlider;

	public Slider sfxSlider;

	public Slider sensitivitySlider;

	public GameObject modTogglePrefab;

	public RectTransform modListPanel;

	public TMP_Dropdown overallDropdown;

	public TMP_Dropdown lightingDropdown;

	public TMP_Dropdown shadowsDropdown;

	public TMP_Dropdown texturesDropdown;

	public TMP_Dropdown particlesDropdown;

	public TMP_Dropdown modelDetailDropdown;

	public TMP_Dropdown waterDetailDropdown;

	public TMP_Dropdown antialiasingDropdown;

	public Toggle ambientOcclusionToggle;

	public Toggle bloomToggle;

	public Slider fovSlider;

	public Slider otherTabFovSlider;

	public TMP_Text fovValText;

	public TMP_Text otherTabFovValText;

	public GameObject otherTabFovRow;

	public Slider overscanSlider;

	public TMP_Text overscanValText;

	public GameObject overscanFovRow;

	public Button resetProfileButton;

	public Toggle enableVsyncToggle;

	public Toggle enableVsyncOtherRow;

	public Toggle enableVsyncVideoToggle;

	public TMP_Dropdown resolutionDropdown;

	public Toggle fullscreenToggle;

	public Button resolutionApplyButton;

	public Toggle disableGamepadToggle;

	public Toggle swapSticksToggle;

	public Toggle invertGamepadLookYToggle;

	public Toggle invertMouseLookYToggle;

	public Toggle disableMouseLookSmooth;

	public Toggle showMinimalHUDToggle;

	public Button defaultKeyBtn;

	public Button defaultGamepadBtn;

	public GameObject confirmResolutionDialogPrefab;

	public GameObject videoTab;

	public GameObject audioTab;

	public GameObject inputTab;

	public GameObject gamepadTab;

	public XlateText gamepadTabText;

	public GameObject modsTab;

	public GameObject otherTab;

	public TMP_Dropdown languageDropdown;

	private bool initializing;

	private List<Resolution> dropdownResolutions = new List<Resolution>();

	private Dictionary<BindingLineUI, string> labelKeyDict = new Dictionary<BindingLineUI, string>();

	private bool preventClosing;

	private OptionsDirector optionsDirector;

	public const int MIN_WIDTH = 800;

	public const int MIN_HEIGHT = 600;

	private OnQualityChanged onQualityChanged;

	private bool notifyingQualityChanged;

	public bool IsInitialzing => initializing;

	public override void Awake()
	{
		optionsDirector = SRSingleton<GameContext>.Instance.OptionsDirector;
		base.Awake();
		modsTab.SetActive(value: false);
		title.GetComponent<XlateText>().key = "t.options";
		gamepadPanel.SetActive(value: false);
		tabsPanel.SetActive(value: true);
		videoPanel.SetActive(value: true);
		videoTab.SetActive(value: true);
		inputTab.SetActive(value: true);
		SelectVideoTab();
		bufferForGifToggle.gameObject.SetActive(value: true);
		otherTabFovRow.SetActive(value: false);
		otherTabFovSlider.gameObject.SetActive(value: false);
		enableVsyncOtherRow.gameObject.SetActive(value: false);
		enableVsyncToggle.gameObject.SetActive(value: false);
		SetupVertNav(languageDropdown, tutorialsDropdown, disableCameraBobToggle, showMinimalHUDToggle, bufferForGifToggle, vacLockToggle, sprintHoldToggle, enableVsyncToggle, otherTabFovSlider, overscanSlider, resetProfileButton);
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
		BindingListenOptions listenOptions2 = SRInput.Actions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(OnBindingRejected));
	}

	private void OnBindingAdded(PlayerAction action, BindingSource binding)
	{
		RefreshBindings();
	}

	private void OnBindingRejected(PlayerAction action, BindingSource binding, BindingSourceRejectionType rejection)
	{
		RefreshBindings();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Remove(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
		BindingListenOptions listenOptions2 = SRInput.Actions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Remove(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(OnBindingRejected));
	}

	private void SetupOptionsUI()
	{
		initializing = true;
		SetupOtherOptions();
		SetupMods();
		SetupVideoSettings();
		SetupAudio();
		SetupInput();
		SetupLanguages();
		initializing = false;
	}

	private void SetupAudio()
	{
		masterSlider.value = masterBus.UserVolume;
		musicSlider.value = musicBus.UserVolume;
		sfxSlider.value = sfxBus.UserVolume;
		sensitivitySlider.value = SRSingleton<GameContext>.Instance.InputDirector.MouseLookSensitivity;
	}

	public override void OnBundlesAvailable(MessageDirector msgDir)
	{
		base.OnBundlesAvailable(msgDir);
		SetupOptionsUI();
	}

	private void SetupVideoSettings()
	{
		SetupDropdown(overallDropdown, "l.quality_", (SRQualitySettings.Level level) => level == SRQualitySettings.CurrentLevel, delegate(SRQualitySettings.Level level)
		{
			SRQualitySettings.CurrentLevel = level;
		});
		SetupDropdown(lightingDropdown, "l.lighting_", (SRQualitySettings.LightingLevel level) => level == SRQualitySettings.Lighting, delegate(SRQualitySettings.LightingLevel level)
		{
			SRQualitySettings.Lighting = level;
		});
		SetupDropdown(shadowsDropdown, "l.shadows_", (SRQualitySettings.ShadowsLevel level) => level == SRQualitySettings.Shadows, delegate(SRQualitySettings.ShadowsLevel level)
		{
			SRQualitySettings.Shadows = level;
		});
		SetupDropdown(texturesDropdown, "l.textures_", (SRQualitySettings.TextureLevel level) => level == SRQualitySettings.Textures, delegate(SRQualitySettings.TextureLevel level)
		{
			SRQualitySettings.Textures = level;
		});
		SetupDropdown(particlesDropdown, "l.particles_", (SRQualitySettings.ParticlesLevel level) => level == SRQualitySettings.Particles, delegate(SRQualitySettings.ParticlesLevel level)
		{
			SRQualitySettings.Particles = level;
		});
		SetupDropdown(modelDetailDropdown, "l.model_detail_", (SRQualitySettings.ModelDetailLevel level) => level == SRQualitySettings.ModelDetail, delegate(SRQualitySettings.ModelDetailLevel level)
		{
			SRQualitySettings.ModelDetail = level;
		});
		SetupDropdown(waterDetailDropdown, "l.water_detail_", (SRQualitySettings.WaterDetailLevel level) => level == SRQualitySettings.WaterDetail, delegate(SRQualitySettings.WaterDetailLevel level)
		{
			SRQualitySettings.WaterDetail = level;
		});
		SetupDropdown(antialiasingDropdown, "l.antialiasing_", (SRQualitySettings.AntialiasingMode level) => level == SRQualitySettings.Antialiasing, delegate(SRQualitySettings.AntialiasingMode level)
		{
			SRQualitySettings.Antialiasing = level;
		});
		ambientOcclusionToggle.isOn = SRQualitySettings.AmbientOcclusion;
		bloomToggle.isOn = SRQualitySettings.Bloom;
		onQualityChanged = (OnQualityChanged)Delegate.Combine(onQualityChanged, (OnQualityChanged)delegate
		{
			ambientOcclusionToggle.isOn = SRQualitySettings.AmbientOcclusion;
			bloomToggle.isOn = SRQualitySettings.Bloom;
		});
		fullscreenToggle.isOn = Screen.fullScreen;
		fovSlider.value = optionsDirector.GetFOV();
		fovValText.text = Mathf.RoundToInt(fovSlider.value).ToString();
		otherTabFovSlider.value = optionsDirector.GetFOV();
		otherTabFovValText.text = Mathf.RoundToInt(otherTabFovSlider.value).ToString();
		overscanSlider.value = optionsDirector.GetOverscanAdjustment() * 100f;
		overscanValText.text = Mathf.RoundToInt(overscanSlider.value).ToString();
		enableVsyncToggle.isOn = optionsDirector.enableVsync;
		enableVsyncVideoToggle.isOn = optionsDirector.enableVsync;
		int num = 0;
		Resolution currentResolution = Screen.currentResolution;
		currentResolution.width = Screen.width;
		currentResolution.height = Screen.height;
		Log.Debug("Current resolution", "height", currentResolution.height, "width", currentResolution.width, "refreshRate", currentResolution.refreshRate, "fullScreenMode", Screen.fullScreenMode);
		int num2 = 0;
		int num3 = 0;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution item = resolutions[i];
			if (num2 == item.height && num3 == item.width)
			{
				continue;
			}
			bool flag = currentResolution.width == item.width && currentResolution.height == item.height;
			num2 = item.height;
			num3 = item.width;
			if (flag || (item.width >= 800 && item.height >= 600))
			{
				string text = uiBundle.Get("m.resolution", item.width, item.height);
				resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(text));
				if (flag)
				{
					resolutionDropdown.value = num;
					resolutionDropdown.captionText.text = text;
				}
				dropdownResolutions.Add(item);
				num++;
			}
		}
		if (Application.isEditor)
		{
			resolutionDropdown.interactable = false;
			resolutionDropdown.captionText.text = "Disabled in Editor";
			fullscreenToggle.interactable = false;
			resolutionApplyButton.interactable = false;
			Navigation navigation = overallDropdown.navigation;
			navigation.selectOnUp = null;
			overallDropdown.navigation = navigation;
		}
	}

	private void SetupLanguages()
	{
		SetupDropdown(languageDropdown, "l.lang_", delegate(MessageDirector.Lang lang)
		{
			CultureInfo culture = SRSingleton<GameContext>.Instance.MessageDirector.GetCulture();
			return Enum.GetName(typeof(MessageDirector.Lang), lang).ToLowerInvariant() == culture.TwoLetterISOLanguageName;
		}, delegate(MessageDirector.Lang lang)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.SetCulture(lang);
		});
	}

	private void SetupOtherOptions()
	{
		disableCameraBobToggle.isOn = optionsDirector.disableCameraBob;
		bufferForGifToggle.isOn = optionsDirector.bufferForGif;
		vacLockToggle.isOn = optionsDirector.vacLockOnHold;
		sprintHoldToggle.isOn = optionsDirector.sprintHold;
		showMinimalHUDToggle.isOn = optionsDirector.GetShowMinimalHUD();
		SetupDropdown(tutorialsDropdown, "l.tutorials.", (OptionsDirector.EnabledTutorials value) => optionsDirector.enabledTutorials == value, delegate(OptionsDirector.EnabledTutorials value)
		{
			optionsDirector.enabledTutorials = value;
		});
	}

	private void SetupInput()
	{
		for (int i = 0; i < bindingsPanel.transform.childCount; i++)
		{
			Destroyer.Destroy(bindingsPanel.transform.GetChild(i).gameObject, "OptionsUI.SetupInput");
		}
		CreateKeyBindingLine("key.forward", SRInput.Actions.verticalPos);
		CreateKeyBindingLine("key.left", SRInput.Actions.horizontalNeg);
		CreateKeyBindingLine("key.back", SRInput.Actions.verticalNeg);
		CreateKeyBindingLine("key.right", SRInput.Actions.horizontalPos);
		CreateKeyBindingLine("key.shoot", SRInput.Actions.attack);
		CreateKeyBindingLine("key.vac", SRInput.Actions.vac);
		CreateKeyBindingLine("key.burst", SRInput.Actions.burst);
		CreateKeyBindingLine("key.jump", SRInput.Actions.jump);
		CreateKeyBindingLine("key.run", SRInput.Actions.run);
		CreateKeyBindingLine("key.interact", SRInput.Actions.interact);
		CreateKeyBindingLine("key.gadgetMode", SRInput.Actions.toggleGadgetMode);
		CreateKeyBindingLine("key.flashlight", SRInput.Actions.light);
		CreateKeyBindingLine("key.radar", SRInput.Actions.radarToggle);
		CreateKeyBindingLine("key.map", SRInput.Actions.openMap);
		CreateKeyBindingLine("key.slot_1", SRInput.Actions.slot1);
		CreateKeyBindingLine("key.slot_2", SRInput.Actions.slot2);
		CreateKeyBindingLine("key.slot_3", SRInput.Actions.slot3);
		CreateKeyBindingLine("key.slot_4", SRInput.Actions.slot4);
		CreateKeyBindingLine("key.slot_5", SRInput.Actions.slot5);
		CreateKeyBindingLine("key.prev_slot", SRInput.Actions.prevSlot);
		CreateKeyBindingLine("key.next_slot", SRInput.Actions.nextSlot);
		CreateKeyBindingLine("key.reportissue", SRInput.Actions.reportIssue);
		CreateKeyBindingLine("key.screenshot", SRInput.Actions.screenshot);
		CreateKeyBindingLine("key.recordgif", SRInput.Actions.recordGif);
		CreateKeyBindingLine("key.pedia", SRInput.Actions.pedia);
		Button[] componentsInChildren = bindingsPanel.GetComponentsInChildren<Button>(includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j += 2)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnRight = componentsInChildren[j + 1];
			if (j < componentsInChildren.Length - 2)
			{
				navigation.selectOnDown = componentsInChildren[j + 2];
			}
			else
			{
				navigation.selectOnDown = sensitivitySlider;
				Navigation navigation2 = sensitivitySlider.navigation;
				navigation2.mode = Navigation.Mode.Explicit;
				navigation2.selectOnUp = componentsInChildren[j];
				sensitivitySlider.navigation = navigation2;
			}
			if (j > 0)
			{
				navigation.selectOnUp = componentsInChildren[j - 2];
			}
			else
			{
				navigation.selectOnUp = defaultKeyBtn;
				Navigation navigation3 = defaultKeyBtn.navigation;
				navigation3.mode = Navigation.Mode.Explicit;
				navigation3.selectOnDown = componentsInChildren[j];
				defaultKeyBtn.navigation = navigation3;
			}
			componentsInChildren[j].navigation = navigation;
			Navigation navigation4 = default(Navigation);
			navigation4.mode = Navigation.Mode.Explicit;
			navigation4.selectOnLeft = componentsInChildren[j];
			if (j < componentsInChildren.Length - 2)
			{
				navigation4.selectOnDown = componentsInChildren[j + 3];
			}
			else
			{
				navigation4.selectOnDown = sensitivitySlider;
			}
			if (j > 0)
			{
				navigation4.selectOnUp = componentsInChildren[j - 1];
			}
			else
			{
				navigation4.selectOnUp = defaultKeyBtn;
			}
			componentsInChildren[j + 1].navigation = navigation4;
		}
	}

	private void SetupMods()
	{
		Toggle[] componentsInChildren = modListPanel.GetComponentsInChildren<Toggle>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Destroyer.Destroy(componentsInChildren[i].gameObject, "OptionsUI.SetupMods");
		}
		foreach (ModDirector.Mod value in Enum.GetValues(typeof(ModDirector.Mod)))
		{
			CreateModToggle(value);
		}
		Toggle[] componentsInChildren2 = modListPanel.GetComponentsInChildren<Toggle>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (j < componentsInChildren2.Length - 1)
			{
				navigation.selectOnDown = componentsInChildren2[j + 1];
			}
			if (j > 0)
			{
				navigation.selectOnUp = componentsInChildren2[j - 1];
			}
			componentsInChildren2[j].navigation = navigation;
		}
		invertMouseLookYToggle.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetInvertMouseLookY();
		disableMouseLookSmooth.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetDisableMouseLookSmooth();
		if (componentsInChildren2.Length != 0)
		{
			componentsInChildren2[0].gameObject.AddComponent<InitSelected>();
		}
	}

	public void RefreshBindings()
	{
		if (!initializing)
		{
			BindingLineUI[] componentsInChildren = GetComponentsInChildren<BindingLineUI>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Refresh();
			}
			if (gamepadPanel != null)
			{
				gamepadPanel.GetComponent<GamepadPanel>().RefreshBindings();
			}
		}
	}

	private GameObject CreateKeyBindingLine(string label, PlayerAction action)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(bindingLinePrefab);
		gameObject.transform.SetParent(bindingsPanel.transform, worldPositionStays: false);
		BindingPanel.CreateBindingLine(label, action, gameObject, uiBundle, labelKeyDict, null);
		return gameObject;
	}

	public void ToggleDisableCameraBob()
	{
		optionsDirector.disableCameraBob = disableCameraBobToggle.isOn;
	}

	public void ToggleEnableVsync()
	{
		optionsDirector.enableVsync = enableVsyncToggle.isOn;
		optionsDirector.UpdateVsync();
	}

	public void ToggleEnableVsyncVideo()
	{
		optionsDirector.enableVsync = enableVsyncVideoToggle.isOn;
		optionsDirector.UpdateVsync();
	}

	public void ToggleBufferForGif()
	{
		optionsDirector.bufferForGif = bufferForGifToggle.isOn;
	}

	public void ToggleVacLock()
	{
		optionsDirector.vacLockOnHold = vacLockToggle.isOn;
	}

	public void ToggleSprintHold()
	{
		optionsDirector.sprintHold = sprintHoldToggle.isOn;
	}

	public override void Close()
	{
		base.Close();
		SRInput.AddOrReplaceBinding(SRInput.PauseActions.closeMap, SRInput.Actions.openMap);
		SRSingleton<SceneContext>.Instance.TutorialDirector.MaybeShowStatusTutorials();
		SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveProfile();
	}

	private void DeselectAll()
	{
		videoPanel.SetActive(value: false);
		audioPanel.SetActive(value: false);
		inputPanel.SetActive(value: false);
		gamepadPanel.SetActive(value: false);
		modsPanel.SetActive(value: false);
		otherPanel.SetActive(value: false);
	}

	public void SelectVideoTab()
	{
		DeselectAll();
		videoPanel.SetActive(value: true);
	}

	public void SelectAudioTab()
	{
		DeselectAll();
		audioPanel.SetActive(value: true);
	}

	public void SelectInputTab()
	{
		DeselectAll();
		inputPanel.SetActive(value: true);
	}

	public void SelectGamepadTab()
	{
		DeselectAll();
		gamepadPanel.SetActive(value: true);
	}

	public void SelectModsTab()
	{
		DeselectAll();
		modsPanel.SetActive(value: true);
	}

	public void SelectOtherTab()
	{
		DeselectAll();
		otherPanel.SetActive(value: true);
	}

	public void OnAudioLevelsChanged()
	{
		if (!initializing)
		{
			masterBus.UserVolume = masterSlider.value;
			musicBus.UserVolume = musicSlider.value;
			sfxBus.UserVolume = sfxSlider.value;
		}
	}

	public void OnSensitivityChanged()
	{
		float value = sensitivitySlider.value;
		SRSingleton<GameContext>.Instance.InputDirector.MouseLookSensitivity = value;
	}

	public void OnAmbientOcclusionChanged()
	{
		SRQualitySettings.AmbientOcclusion = ambientOcclusionToggle.isOn;
		onQualityChanged();
	}

	public void OnBloomChanged()
	{
		SRQualitySettings.Bloom = bloomToggle.isOn;
		onQualityChanged();
	}

	public void OnFOVChanged()
	{
		optionsDirector.SetFOV(fovSlider.value);
		fovValText.text = Mathf.RoundToInt(fovSlider.value).ToString();
	}

	public void OnOtherTabFOVChanged()
	{
		optionsDirector.SetFOV(otherTabFovSlider.value);
		otherTabFovValText.text = Mathf.RoundToInt(otherTabFovSlider.value).ToString();
	}

	public void OnOverscanAdjustmentChanged()
	{
		float overscanAdjustment = Mathf.Clamp(overscanSlider.value, 0f, 15f) * 0.01f;
		optionsDirector.SetOverscanAdjustment(overscanAdjustment);
		overscanValText.text = Mathf.RoundToInt(overscanSlider.value).ToString();
	}

	public void OnApplyResolution()
	{
		CreateConfirmResolutionDialog();
		Resolution resolution = dropdownResolutions[resolutionDropdown.value];
		optionsDirector.SetScreenResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
	}

	public void ToggleInvertMouseLookY()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetInvertMouseLookY(invertMouseLookYToggle.isOn);
	}

	public void ToggleDisableMouseLookSmooth()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetDisableMouseLookSmooth(disableMouseLookSmooth.isOn);
	}

	public void ToggleShowMinimalHUD()
	{
		SRSingleton<GameContext>.Instance.OptionsDirector.SetShowMinimalHUD(showMinimalHUDToggle.isOn);
	}

	public void ResetKeyMouseDefaults()
	{
		SRSingleton<GameContext>.Instance.InputDirector.ResetKeyMouseDefaults();
		RefreshBindings();
	}

	public void ResetProfile()
	{
		SRSingleton<GameContext>.Instance.UITemplates.CreateConfirmDialog("m.confirm_reset_profile", delegate
		{
			Close();
			SRSingleton<GameContext>.Instance.AutoSaveDirector.ResetProfile();
		});
	}

	private void CreateModToggle(ModDirector.Mod mod)
	{
		GameObject obj = UnityEngine.Object.Instantiate(modTogglePrefab);
		obj.transform.SetParent(modListPanel, worldPositionStays: false);
		Toggle component = obj.GetComponent<Toggle>();
		component.isOn = SRSingleton<SceneContext>.Instance.ModDirector.IsModActive(mod);
		obj.transform.Find("Label").GetComponent<TMP_Text>().text = uiBundle.Get("l.mod_" + Enum.GetName(typeof(ModDirector.Mod), mod).ToLowerInvariant());
		component.onValueChanged.AddListener(delegate(bool selected)
		{
			if (selected)
			{
				SRSingleton<SceneContext>.Instance.ModDirector.ActivateMod(mod);
			}
			else
			{
				SRSingleton<SceneContext>.Instance.ModDirector.DeactivateMod(mod);
			}
		});
	}

	private void SetupDropdown<T>(TMP_Dropdown dropdown, string msgPrefix, Predicate<T> isLevel, UnityAction<T> assignLevel)
	{
		int num = 0;
		dropdown.options.Clear();
		dropdown.onValueChanged.RemoveAllListeners();
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			string text = Enum.GetName(typeof(T), value);
			string levelMsg = uiBundle.Xlate(msgPrefix + text.ToLowerInvariant());
			dropdown.options.Add(new TMP_Dropdown.OptionData(levelMsg));
			T fLevel = value;
			int fIdx = num;
			OnQualityChanged onQualityChanged = delegate
			{
				if (isLevel(fLevel))
				{
					dropdown.value = fIdx;
					dropdown.captionText.text = levelMsg;
				}
			};
			this.onQualityChanged = (OnQualityChanged)Delegate.Combine(this.onQualityChanged, onQualityChanged);
			onQualityChanged();
			dropdown.onValueChanged.AddListener(delegate(int val)
			{
				if (val == fIdx)
				{
					assignLevel(fLevel);
					NotifyQualityChanged();
				}
			});
			num++;
		}
	}

	private void NotifyQualityChanged()
	{
		if (!notifyingQualityChanged)
		{
			try
			{
				notifyingQualityChanged = true;
				onQualityChanged();
			}
			finally
			{
				notifyingQualityChanged = false;
			}
		}
	}

	private GameObject CreateConfirmResolutionDialog()
	{
		Resolution oldRes = default(Resolution);
		oldRes.width = Screen.width;
		oldRes.height = Screen.height;
		bool oldFullscreen = Screen.fullScreen;
		oldRes.refreshRate = Screen.currentResolution.refreshRate;
		PreventClosing(prevent: true);
		ConfirmResolutionUI.OnCancel onCancel = delegate
		{
			optionsDirector.SetScreenResolution(oldRes.width, oldRes.height, oldFullscreen);
			fullscreenToggle.isOn = Screen.fullScreen;
			for (int i = 0; i < dropdownResolutions.Count; i++)
			{
				Resolution resolution = dropdownResolutions[i];
				if (oldRes.width == resolution.width && oldRes.height == resolution.height)
				{
					resolutionDropdown.value = i;
					string text = uiBundle.Get("m.resolution", resolution.width, resolution.height);
					resolutionDropdown.captionText.text = text;
				}
			}
			resolutionApplyButton.Select();
			PreventClosing(prevent: false);
		};
		ConfirmResolutionUI.OnConfirm onConfirm = delegate
		{
			resolutionApplyButton.Select();
			PreventClosing(prevent: false);
		};
		GameObject obj = UnityEngine.Object.Instantiate(confirmResolutionDialogPrefab);
		obj.GetComponent<ConfirmResolutionUI>().onCancel = onCancel;
		obj.GetComponent<ConfirmResolutionUI>().onConfirm = onConfirm;
		return obj;
	}

	protected override bool Closeable()
	{
		if (base.Closeable())
		{
			return !preventClosing;
		}
		return false;
	}

	public void PreventClosing(bool prevent)
	{
		preventClosing = prevent;
	}

	private void SetupVertNav(params Selectable[] selectables)
	{
		List<Selectable> list = new List<Selectable>();
		foreach (Selectable selectable in selectables)
		{
			if (selectable.gameObject.activeSelf)
			{
				list.Add(selectable);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Navigation navigation = list[j].navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (j == 0)
			{
				navigation.selectOnUp = null;
			}
			else
			{
				navigation.selectOnUp = list[j - 1];
			}
			if (j == list.Count - 1)
			{
				navigation.selectOnDown = null;
			}
			else
			{
				navigation.selectOnDown = list[j + 1];
			}
			list[j].navigation = navigation;
		}
		if (list.Count > 0)
		{
			list[0].gameObject.AddComponent<InitSelected>();
		}
	}
}
