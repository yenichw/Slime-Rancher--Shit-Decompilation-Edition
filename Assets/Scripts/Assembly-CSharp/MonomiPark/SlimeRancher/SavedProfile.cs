using System;
using System.Collections.Generic;
using System.IO;
using DLCPackage;
using InControl;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

namespace MonomiPark.SlimeRancher
{
	public class SavedProfile
	{
		private SettingsV01 currentSettings;

		private ProfileV07 currentProfile;

		private string continueGameName = "";

		public SettingsV01 Settings => currentSettings;

		public ProfileV07 Profile => currentProfile;

		public string ContinueGameName
		{
			get
			{
				return continueGameName;
			}
			set
			{
				continueGameName = value;
			}
		}

		public void LoadProfile(Stream stream)
		{
			Log.Debug("Loading profile.");
			ProfileV07 profileV = new ProfileV07();
			try
			{
				profileV.Load(stream);
				currentProfile = profileV;
				Log.Debug("Loaded profile.", "achievement count", profileV.achievements.earnedAchievements.Count, "continue game", profileV.continueGameName);
			}
			catch (Exception ex)
			{
				Log.Warning("Failed to load profile.", "Error Message", ex.Message, "Error", ex.ToString());
			}
		}

		public void SaveProfile(Stream stream)
		{
			Log.Debug("Saving profile.", "achievement count", currentProfile.achievements.earnedAchievements.Count, "continue game", currentProfile.continueGameName);
			currentProfile.Write(stream);
		}

		public void LoadSettings(Stream stream)
		{
			Log.Debug("Loading settings.");
			SettingsV01 settingsV = new SettingsV01();
			try
			{
				settingsV.Load(stream);
				currentSettings = settingsV;
			}
			catch (Exception ex)
			{
				Log.Warning("Failed to load settings.", "Error Message", ex.Message, "Error", ex.ToString());
			}
		}

		public void SaveSettings(Stream stream)
		{
			Log.Debug("Saving settings.");
			currentSettings.Write(stream);
		}

		public void LoadLegacySettings(Stream stream)
		{
			Log.Debug("Attempting to load settings from legacy profile file.");
			ProfileV04 profileV = new ProfileV04();
			SettingsV01 settingsV = new SettingsV01();
			try
			{
				profileV.Load(stream);
				settingsV.SetLegacyProfileOptions(profileV);
				currentSettings = settingsV;
				Log.Debug("Loaded legacy settings.");
			}
			catch (Exception)
			{
				Log.Warning("Failed to load legacy profile.");
			}
		}

		public void Push()
		{
			if (currentProfile == null)
			{
				Log.Debug("No profile was set. Using default profile.");
				currentProfile = new ProfileV07();
			}
			if (currentSettings == null)
			{
				Log.Debug("No settings were set. Using default settings.");
				currentSettings = new SettingsV01();
				if ((bool)SRSingleton<GameContext>.Instance && (bool)SRSingleton<GameContext>.Instance.MessageDirector)
				{
					currentSettings.options.language = (int)SRSingleton<GameContext>.Instance.MessageDirector.GetCultureLang();
				}
			}
			PushOptions(currentSettings.options);
			PushAchievements(currentProfile.achievements);
			continueGameName = currentProfile.continueGameName;
		}

		private void PushOptions(OptionsV12 options)
		{
			if (options == null)
			{
				Log.Warning("Options data was null.");
				return;
			}
			InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
			OptionsDirector optionsDirector = SRSingleton<GameContext>.Instance.OptionsDirector;
			PushBindings(options.bindings, inputDirector);
			inputDirector.SetDisableGamepad(options.disableGamepad);
			inputDirector.SetSwapSticks(options.swapSticks);
			inputDirector.SetInvertGamepadLookY(options.invertGamepadLookY);
			inputDirector.SetInvertMouseLookY(options.invertMouseLookY);
			inputDirector.SetDisableMouseLookSmooth(options.disableMouseLookSmooth);
			inputDirector.MouseLookSensitivity = options.mouseSensitivity;
			inputDirector.GamepadLookSensitivityX = options.lookSensitivityX;
			inputDirector.GamepadLookSensitivityY = options.lookSensitivityY;
			inputDirector.ControllerStickDeadZone = options.controllerStickDeadZone;
			optionsDirector.disableCameraBob = options.disableCameraBob;
			optionsDirector.enabledTutorials = options.enabledTutorials;
			optionsDirector.bugReportEmail = options.bugReportEmail;
			optionsDirector.bufferForGif = options.bufferForGif;
			optionsDirector.vacLockOnHold = options.vacLockOnHold;
			optionsDirector.SetFOV(options.fieldOfView);
			optionsDirector.sprintHold = options.sprintHold;
			optionsDirector.enableVsync = options.enableVsync;
			optionsDirector.UpdateVsync();
			optionsDirector.SetOverscanAdjustment(options.overscanAdjustment);
			optionsDirector.SetShowMinimalHUD(options.showMinimalHUD);
			Log.Debug("Restoring volume.", "Music", options.musicVolume, "SFX", options.sfxVolume, "Master", options.masterVolume);
			SECTR_AudioBus masterBus = SECTR_AudioSystem.System.MasterBus;
			foreach (SECTR_AudioBus child in masterBus.Children)
			{
				if (child.name == "Music")
				{
					child.UserVolume = options.musicVolume;
				}
				else if (child.name == "SFX")
				{
					child.UserVolume = options.sfxVolume;
				}
				else
				{
					Log.Warning("Unknown top-level bus name: " + child.name);
				}
			}
			masterBus.UserVolume = options.masterVolume;
			if (options.screenWidth < 800 || options.screenHeight < 600)
			{
				optionsDirector.SetScreenResolution(800, 600, options.fullScreen);
			}
			else
			{
				optionsDirector.SetScreenResolution(options.screenWidth, options.screenHeight, options.fullScreen);
			}
			PushQualitySettings(options.qualitySettings, options.qualityLevel);
			SRSingleton<GameContext>.Instance.MessageDirector.SetCulture((MessageDirector.Lang)options.language);
		}

		private void PushQualitySettings(QualitySettingsV02 qualitySettings, int qualityLevel)
		{
			SRQualitySettings.Push(new SRQualitySettings.Settings((SRQualitySettings.LightingLevel)qualitySettings.lighting, (SRQualitySettings.TextureLevel)qualitySettings.textures, (SRQualitySettings.AntialiasingMode)qualitySettings.antialiasing, (SRQualitySettings.ShadowsLevel)qualitySettings.shadows, (SRQualitySettings.ParticlesLevel)qualitySettings.particles, (SRQualitySettings.ModelDetailLevel)qualitySettings.modelDetail, (SRQualitySettings.WaterDetailLevel)qualitySettings.waterDetail, qualitySettings.ambientOcclusion, qualitySettings.bloom), (SRQualitySettings.Level)qualityLevel);
		}

		private void PushBindings(BindingsV05 bindingsData, InputDirector inputDir)
		{
			inputDir.ResetKeyMouseDefaults();
			inputDir.ResetGamepadDefaults();
			if (bindingsData == null || bindingsData.bindings == null)
			{
				return;
			}
			foreach (BindingV01 binding in bindingsData.bindings)
			{
				PlayerAction playerAction = SRInput.Actions.Get(binding.action) ?? SRInput.PauseActions.Get(binding.action);
				if (playerAction == null)
				{
					Log.Warning("Ignoring the binding for unknown action: " + binding.action);
				}
				else
				{
					SRInput.AddOrReplaceBinding(playerAction, binding);
				}
			}
		}

		private void PushAchievements(PlayerAchievementsV03 achievements)
		{
			if (achievements == null)
			{
				Log.Warning("Achievements data was null.");
				return;
			}
			Dictionary<AchievementsDirector.BoolStat, bool> dictionary = new Dictionary<AchievementsDirector.BoolStat, bool>();
			foreach (KeyValuePair<int, bool> @event in achievements.progress.events)
			{
				dictionary.Add((AchievementsDirector.BoolStat)@event.Key, @event.Value);
			}
			Dictionary<AchievementsDirector.IntStat, int> dictionary2 = new Dictionary<AchievementsDirector.IntStat, int>();
			foreach (KeyValuePair<int, int> count in achievements.progress.counts)
			{
				dictionary2.Add((AchievementsDirector.IntStat)count.Key, count.Value);
			}
			Dictionary<AchievementsDirector.EnumStat, List<Enum>> dictionary3 = new Dictionary<AchievementsDirector.EnumStat, List<Enum>>();
			foreach (KeyValuePair<int, List<Enum>> list3 in achievements.progress.lists)
			{
				List<Enum> list = new List<Enum>();
				foreach (Enum item in list3.Value)
				{
					list.Add(item);
				}
				dictionary3.Add((AchievementsDirector.EnumStat)list3.Key, list);
			}
			List<AchievementsDirector.Achievement> list2 = new List<AchievementsDirector.Achievement>();
			foreach (int earnedAchievement in achievements.earnedAchievements)
			{
				list2.Add((AchievementsDirector.Achievement)earnedAchievement);
			}
			ProfileAchievesModel profileAchievesModel = SRSingleton<SceneContext>.Instance.GameModel.GetProfileAchievesModel();
			SRSingleton<SceneContext>.Instance.AchievementsDirector.InitModel(profileAchievesModel);
			profileAchievesModel.Push(dictionary, dictionary2, dictionary3, list2);
			SRSingleton<SceneContext>.Instance.AchievementsDirector.SetModel(profileAchievesModel);
		}

		public void Pull()
		{
			ProfileV07 profileV = new ProfileV07();
			profileV.achievements = new PlayerAchievementsV03();
			PullAchievements(profileV.achievements);
			profileV.continueGameName = continueGameName;
			profileV.DLC = new DLCV02
			{
				installed = new List<Id>(SRSingleton<GameContext>.Instance.DLCDirector.Installed)
			};
			Log.Debug("Profile pulled", "achievement count", profileV.achievements.earnedAchievements.Count, "continue game", profileV.continueGameName);
			SettingsV01 settingsV = new SettingsV01();
			settingsV.options = new OptionsV12();
			PullOptions(settingsV.options);
			currentProfile = profileV;
			currentSettings = settingsV;
		}

		private void PullOptions(OptionsV12 options)
		{
			InputDirector inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
			OptionsDirector optionsDirector = SRSingleton<GameContext>.Instance.OptionsDirector;
			options.swapSticks = inputDirector.GetSwapSticks();
			options.invertGamepadLookY = inputDirector.GetInvertGamepadLookY();
			options.invertMouseLookY = inputDirector.GetInvertMouseLookY();
			options.disableMouseLookSmooth = inputDirector.GetDisableMouseLookSmooth();
			options.mouseSensitivity = inputDirector.MouseLookSensitivity;
			options.lookSensitivityX = inputDirector.GamepadLookSensitivityX;
			options.lookSensitivityY = inputDirector.GamepadLookSensitivityY;
			options.controllerStickDeadZone = inputDirector.ControllerStickDeadZone;
			options.disableGamepad = inputDirector.GetDisableGamepad();
			options.disableCameraBob = optionsDirector.disableCameraBob;
			options.bufferForGif = optionsDirector.bufferForGif;
			options.vacLockOnHold = optionsDirector.vacLockOnHold;
			options.fieldOfView = optionsDirector.GetFOV();
			options.sprintHold = optionsDirector.sprintHold;
			options.enableVsync = optionsDirector.enableVsync;
			options.overscanAdjustment = optionsDirector.GetOverscanAdjustment();
			options.bugReportEmail = optionsDirector.bugReportEmail;
			options.enabledTutorials = optionsDirector.enabledTutorials;
			options.showMinimalHUD = optionsDirector.GetShowMinimalHUD();
			SECTR_AudioBus masterBus = SECTR_AudioSystem.System.MasterBus;
			foreach (SECTR_AudioBus child in masterBus.Children)
			{
				if (child.name == "Music")
				{
					options.musicVolume = child.UserVolume;
				}
				else if (child.name == "SFX")
				{
					options.sfxVolume = child.UserVolume;
				}
				else
				{
					Log.Warning("Unknown top-level bus name: " + child.name);
				}
				Log.Debug("Retrieving volume from master bus", "name", child.name, "value", child.UserVolume);
			}
			options.masterVolume = masterBus.UserVolume;
			Log.Debug("Updating volume.", "Music", options.musicVolume, "SFX", options.sfxVolume, "Master", options.masterVolume);
			QualitySettingsV02 qualitySettingsV = new QualitySettingsV02();
			SRQualitySettings.Settings settings = null;
			SRQualitySettings.Level overallLevel = SRQualitySettings.Level.DEFAULT;
			SRQualitySettings.Pull(out settings, out overallLevel);
			options.qualityLevel = (int)overallLevel;
			qualitySettingsV.lighting = (int)settings.lighting;
			qualitySettingsV.antialiasing = (int)settings.antialiasing;
			qualitySettingsV.shadows = (int)settings.shadows;
			qualitySettingsV.particles = (int)settings.particles;
			qualitySettingsV.textures = (int)settings.textures;
			qualitySettingsV.modelDetail = (int)settings.modelDetail;
			qualitySettingsV.waterDetail = (int)settings.waterDetail;
			qualitySettingsV.ambientOcclusion = settings.ambientOcclusion;
			qualitySettingsV.bloom = settings.bloom;
			options.qualitySettings = qualitySettingsV;
			if (!Application.isEditor)
			{
				options.fullScreen = Screen.fullScreen;
				options.screenWidth = Screen.width;
				options.screenHeight = Screen.height;
				options.refreshRate = Screen.currentResolution.refreshRate;
			}
			options.bindings = new BindingsV05();
			PullBindings(options.bindings, SRInput.Actions.Actions);
			PullBindings(options.bindings, SRInput.PauseActions.Actions);
			options.language = (int)SRSingleton<GameContext>.Instance.MessageDirector.GetCultureLang();
		}

		private void PullBindings(BindingsV05 bindings, IEnumerable<PlayerAction> actions)
		{
			foreach (PlayerAction action in actions)
			{
				if (!SRInput.IsProtected(action))
				{
					BindingV01 bindingV = SRInput.ToBinding(action);
					if (!SRInput.IsProtected((Key)bindingV.primKey, (Key)bindingV.secKey))
					{
						bindings.bindings.Add(bindingV);
					}
				}
			}
		}

		private void PullAchievements(PlayerAchievementsV03 achievements)
		{
			SRSingleton<SceneContext>.Instance.GameModel.GetProfileAchievesModel().Pull(out var boolStatDict, out var intStatDict, out var enumStatDict, out var earnedAchievements);
			achievements.earnedAchievements = new List<int>();
			foreach (AchievementsDirector.Achievement item in earnedAchievements)
			{
				achievements.earnedAchievements.Add((int)item);
			}
			PlayerAchievementProgressV01 playerAchievementProgressV = new PlayerAchievementProgressV01();
			playerAchievementProgressV.counts = new Dictionary<int, int>();
			playerAchievementProgressV.events = new Dictionary<int, bool>();
			playerAchievementProgressV.lists = new Dictionary<int, List<Enum>>();
			foreach (KeyValuePair<AchievementsDirector.IntStat, int> item2 in intStatDict)
			{
				playerAchievementProgressV.counts.Add((int)item2.Key, item2.Value);
			}
			foreach (KeyValuePair<AchievementsDirector.BoolStat, bool> item3 in boolStatDict)
			{
				playerAchievementProgressV.events.Add((int)item3.Key, item3.Value);
			}
			foreach (KeyValuePair<AchievementsDirector.EnumStat, List<Enum>> item4 in enumStatDict)
			{
				List<Enum> list = new List<Enum>();
				foreach (Enum item5 in item4.Value)
				{
					list.Add(item5);
				}
				playerAchievementProgressV.lists.Add((int)item4.Key, list);
			}
			achievements.progress = playerAchievementProgressV;
		}
	}
}
