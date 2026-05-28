using System;
using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ProfileV04 : VersionedPersistedDataSet<ProfileData>
	{
		public OptionsV09 options = new OptionsV09();

		public PlayerAchievementsV03 achievements = new PlayerAchievementsV03();

		public string continueGameName = "";

		public override string Identifier => "SRPF";

		public override uint Version => 4u;

		protected override void LoadData(BinaryReader reader)
		{
			options = new OptionsV09();
			options.Load(reader.BaseStream);
			ReadSectionSeparator(reader);
			achievements = new PlayerAchievementsV03();
			achievements.Load(reader.BaseStream);
			continueGameName = reader.ReadString();
		}

		protected override void UpgradeFrom(ProfileData legacyData)
		{
			UpgradeOptions(legacyData.options);
			UpgradeAchievements(legacyData.achieve);
			continueGameName = (string.IsNullOrEmpty(legacyData.continueGameName) ? string.Empty : legacyData.continueGameName);
		}

		private void UpgradeOptions(OptionsData legacyOptions)
		{
			options = new OptionsV09();
			options.screenWidth = legacyOptions.screenWidth;
			options.screenHeight = legacyOptions.screenHeight;
			options.fullScreen = legacyOptions.fullScreen;
			options.qualitySettings = new QualitySettingsV02();
			options.qualitySettings.lighting = (int)legacyOptions.qualitySettings.lighting;
			options.qualitySettings.textures = (int)legacyOptions.qualitySettings.textures;
			options.qualitySettings.antialiasing = (int)legacyOptions.qualitySettings.antialiasing;
			options.qualitySettings.shadows = (int)legacyOptions.qualitySettings.shadows;
			options.qualitySettings.particles = (int)legacyOptions.qualitySettings.particles;
			options.qualitySettings.modelDetail = (int)legacyOptions.qualitySettings.modelDetail;
			options.qualitySettings.waterDetail = (int)legacyOptions.qualitySettings.waterDetail;
			options.qualitySettings.ambientOcclusion = legacyOptions.qualitySettings.ambientOcclusion;
			options.qualitySettings.bloom = legacyOptions.qualitySettings.bloom;
			options.qualityLevel = (int)legacyOptions.qualityLevel;
			options.masterVolume = legacyOptions.masterVolume;
			options.musicVolume = legacyOptions.musicVolume;
			options.sfxVolume = legacyOptions.sfxVolume;
			options.disableCameraBob = legacyOptions.disableCameraBob;
			options.bugReportEmail = (string.IsNullOrEmpty(legacyOptions.bugReportEmail) ? "" : legacyOptions.bugReportEmail);
			options.bufferForGif = legacyOptions.bufferForGif;
			options.disableTutorials = legacyOptions.disableTutorials;
			options.vacLockOnHold = legacyOptions.vacLockOnHold;
			options.swapSticks = legacyOptions.swapSticks;
			options.invertGamepadLookY = legacyOptions.invertGamepadLookY;
			options.invertMouseLookY = legacyOptions.invertMouseLookY;
			options.disableGamepad = legacyOptions.disableGamepad;
			options.lookSensitivityX = legacyOptions.lookSensitivityX;
			options.lookSensitivityY = legacyOptions.lookSensitivityY;
			options.mouseSensitivity = legacyOptions.mouseSensitivity;
			options.bindings = UpgradeBindings(legacyOptions.bindings);
			options.language = 0;
			options.sprintHold = true;
			options.enableVsync = true;
			options.overscanAdjustment = 0f;
		}

		private BindingsV04 UpgradeBindings(List<OptionsData.BindingData> legacyBindings)
		{
			BindingsV04 bindingsV = new BindingsV04();
			bindingsV.bindings = new List<BindingsV04.Binding>();
			if (legacyBindings != null)
			{
				foreach (OptionsData.BindingData legacyBinding in legacyBindings)
				{
					bindingsV.bindings.Add(UpgradeBinding(legacyBinding));
				}
			}
			return bindingsV;
		}

		private BindingsV04.Binding UpgradeBinding(OptionsData.BindingData legacyBinding)
		{
			return new BindingsV04.Binding
			{
				action = legacyBinding.action,
				gamepad = (int)legacyBinding.gamepad,
				primKey = (int)legacyBinding.primKey,
				secKey = (int)legacyBinding.secondary,
				secMouse = 0,
				primMouse = (int)legacyBinding.primMouse
			};
		}

		private void UpgradeAchievements(AchieveData legacyAchievements)
		{
			achievements = new PlayerAchievementsV03();
			achievements.earnedAchievements = new List<int>();
			foreach (AchievementsDirector.Achievement earnedAchievement in legacyAchievements.earnedAchievements)
			{
				achievements.earnedAchievements.Add((int)earnedAchievement);
			}
			PlayerAchievementProgressV01 playerAchievementProgressV = new PlayerAchievementProgressV01();
			playerAchievementProgressV.counts = new Dictionary<int, int>();
			playerAchievementProgressV.events = new Dictionary<int, bool>();
			playerAchievementProgressV.lists = new Dictionary<int, List<Enum>>();
			foreach (KeyValuePair<AchievementsDirector.IntStat, int> item in legacyAchievements.intStatDict)
			{
				playerAchievementProgressV.counts.Add((int)item.Key, item.Value);
			}
			foreach (KeyValuePair<AchievementsDirector.BoolStat, bool> item2 in legacyAchievements.boolStatDict)
			{
				playerAchievementProgressV.events.Add((int)item2.Key, item2.Value);
			}
			foreach (KeyValuePair<AchievementsDirector.EnumStat, List<Enum>> item3 in legacyAchievements.enumStatDict)
			{
				List<Enum> list = new List<Enum>();
				foreach (Enum item4 in item3.Value)
				{
					list.Add(item4);
				}
				playerAchievementProgressV.lists.Add((int)item3.Key, list);
			}
			achievements.progress = playerAchievementProgressV;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			options.Write(writer.BaseStream);
			WriteSectionSeparator(writer);
			achievements.Write(writer.BaseStream);
			writer.Write(continueGameName);
		}
	}
}
