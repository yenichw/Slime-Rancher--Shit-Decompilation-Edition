using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class ProgressDirector : MonoBehaviour, ProgressModel.Participant
{
	public delegate void OnProgressChanged();

	public enum ProgressType
	{
		NONE = -1,
		UNLOCK_QUARRY = 0,
		UNLOCK_MOSS = 1,
		UNLOCK_DESERT = 2,
		UNLOCK_LAB = 3,
		UNLOCK_RUINS = 4,
		UNLOCK_DOCKS = 5,
		UNLOCK_GROTTO = 6,
		UNLOCK_OVERGROWTH = 7,
		UNLOCK_VALLEY = 8,
		UNLOCK_LAB_DOCKS_EXTRA = 9,
		UNLOCK_OGDEN_MISSIONS = 200,
		UNLOCK_WILDS = 201,
		UNLOCK_MOCHI_MISSIONS = 202,
		UNLOCK_VIKTOR_MISSIONS = 300,
		UNLOCK_SLIMULATIONS = 301,
		HOBSON_END = 990,
		HOBSON_END_UNLOCK = 991,
		SLIME_DOORS = 1000,
		PLORT_DOOR = 1001,
		EXCHANGE_THORA = 2000,
		EXCHANGE_VIKTOR = 2001,
		EXCHANGE_OGDEN = 2002,
		EXCHANGE_MOCHI = 2003,
		EXCHANGE_BOB = 2004,
		CORPORATE_PARTNER = 3000,
		CORPORATE_PARTNER_UNLOCK = 3001,
		OGDEN_REWARDS = 3100,
		OGDEN_SEEN_FINAL_CHAT = 3101,
		MOCHI_REWARDS = 3102,
		MOCHI_SEEN_FINAL_CHAT = 3103,
		VIKTOR_REWARDS = 3104,
		VIKTOR_SEEN_FINAL_CHAT = 3105,
		ENTER_ZONE_OGDEN_RANCH = 3200,
		ENTER_ZONE_MOCHI_RANCH = 3201,
		ENTER_ZONE_VIKTOR_LAB = 3202,
		ENTER_ZONE_SLIMULATION = 3203
	}

	public class ProgressTypeComparer : IEqualityComparer<ProgressType>
	{
		public bool Equals(ProgressType x, ProgressType y)
		{
			return x == y;
		}

		public int GetHashCode(ProgressType obj)
		{
			return (int)obj;
		}
	}

	public enum ProgressTrackerId
	{
		CASEY_1 = 0,
		CASEY_2 = 1,
		CASEY_3 = 2,
		CASEY_4 = 3,
		CASEY_5 = 4,
		CASEY_6 = 5,
		CASEY_7 = 6,
		CASEY_8 = 7,
		CASEY_9 = 8,
		CASEY_10 = 9,
		CASEY_11 = 10,
		HOBSON_1 = 1000,
		TIME_AFTER_LAB_DOCKS = 2000
	}

	public class ProgressTrackerIdComparer : IEqualityComparer<ProgressTrackerId>
	{
		public bool Equals(ProgressTrackerId x, ProgressTrackerId y)
		{
			return x == y;
		}

		public int GetHashCode(ProgressTrackerId obj)
		{
			return (int)obj;
		}
	}

	public delegate void DelayedProgressDelegate(ProgressTrackerId trackerId);

	private class TimedTracker
	{
		public double unlockAt = double.NaN;

		public void MaybeComplete()
		{
		}
	}

	public class DelayedProgressTracker
	{
		private ProgressTrackerId trackerId;

		private MailDirector.Mail requiredReadMail;

		private ProgressType[] progressTypes;

		private int[] progressCounts;

		private DelayedProgressDelegate onUnlockDel;

		private ProgressDirector progressDir;

		private float delayHrs;

		private bool alreadyUnlocked;

		public DelayedProgressTracker(ProgressDirector progressDir, ProgressTrackerId trackerId, MailDirector.Mail hasReadMail, ProgressType progressType, int progressCount, float delayHrs, DelayedProgressDelegate del)
			: this(progressDir, trackerId, hasReadMail, new ProgressType[1] { progressType }, new int[1] { progressCount }, delayHrs, del)
		{
		}

		public DelayedProgressTracker(ProgressDirector progressDir, ProgressTrackerId trackerId, MailDirector.Mail hasReadMail, ProgressType[] progressTypes, int[] progressCounts, float delayHrs, DelayedProgressDelegate del)
		{
			this.progressDir = progressDir;
			this.trackerId = trackerId;
			requiredReadMail = hasReadMail;
			this.progressTypes = progressTypes;
			this.progressCounts = progressCounts;
			this.delayHrs = delayHrs;
			onUnlockDel = del;
		}

		public void Update()
		{
			if (!alreadyUnlocked && progressDir.timeDir.HasReached(progressDir.model.GetDelayedProgressTime(trackerId)))
			{
				alreadyUnlocked = true;
				onUnlockDel(trackerId);
			}
		}

		public void CheckReady()
		{
			if (double.IsPositiveInfinity(progressDir.model.GetDelayedProgressTime(trackerId)))
			{
				bool num = IsProgressOk();
				bool flag = requiredReadMail == null || progressDir.mailDir.HasReadMail(requiredReadMail);
				if (num && flag)
				{
					progressDir.model.SetDelayedProgressTime(trackerId, progressDir.timeDir.HoursFromNowOrStart(delayHrs));
				}
			}
		}

		private bool IsProgressOk()
		{
			return !progressTypes.Where((ProgressType t, int i) => progressDir.GetProgress(t) < progressCounts[i]).Any();
		}
	}

	public OnProgressChanged onProgressChanged;

	private TimeDirector timeDir;

	private MailDirector mailDir;

	private PlayerState playerState;

	private bool readyForWistfulMusic;

	private bool readyForCredits;

	public static ProgressTypeComparer progressTypeComparer = new ProgressTypeComparer();

	private ProgressModel model;

	public static ProgressTrackerIdComparer progressTrackerIdComparer = new ProgressTrackerIdComparer();

	private Dictionary<ProgressTrackerId, DelayedProgressTracker> delayedProgressTrackerDict = new Dictionary<ProgressTrackerId, DelayedProgressTracker>(progressTrackerIdComparer);

	private const int CORPORATE_PARTNER_MAX = 28;

	private const int OGDEN_REWARD_EXPANSION = 3;

	private const int MOCHI_REWARD_EXPANSION = 3;

	private const int VIKTOR_REWARD_EXPANSION = 3;

	private const int RUN_EFFICIENCY_2_CORPORATE_UNLOCK_PROGRESS_AMOUNT = 19;

	private const int AMMO_4_CORPORATE_UNLOCK_PROGRESS_AMOUNT = 20;

	private const int HEALTH_4_CORPORATE_UNLOCK_PROGRESS_AMOUNT = 21;

	private const int GOLDEN_SURESHOT_CORPORATE_UNLOCK_PROGRESS_AMOUNT = 22;

	private const int MARKET_LINK_PROGRESS_AMOUNT = 11;

	private const int MASTER_GORDO_SNARE_PROGRESS_AMOUNT = 20;

	private const int TITAN_DRILL_PROGRESS_AMOUNT = 23;

	private const int ABYSSAL_PUMP_PROGRESS_AMOUNT = 24;

	private const int ROYAL_APIARY_PROGRESS_AMOUNT = 25;

	private const int GOLD_SLIME_LAMP_PROGRESS_AMOUNT = 26;

	private const int GOLD_WARP_DEPOT_PROGRESS_AMOUNT = 27;

	private const int GOLD_TELEPORTER_PROGRESS_AMOUNT = 28;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterProgress(this);
	}

	public void InitModel(ProgressModel model)
	{
		model.Reset();
		InitDelayedProgressTrackers();
		foreach (ProgressTrackerId key in delayedProgressTrackerDict.Keys)
		{
			model.delayedProgressTimeDict[key] = double.PositiveInfinity;
		}
	}

	public void SetModel(ProgressModel model)
	{
		this.model = model;
		onProgressChanged = (OnProgressChanged)Delegate.Combine(onProgressChanged, new OnProgressChanged(CheckProgressUpgrades));
	}

	public void GameFullyLoaded()
	{
		if (!model.HasProgress(ProgressType.UNLOCK_LAB_DOCKS_EXTRA) && model.HasProgress(ProgressType.UNLOCK_LAB) && model.HasProgress(ProgressType.UNLOCK_DOCKS) && double.IsPositiveInfinity(model.GetDelayedProgressTime(ProgressTrackerId.TIME_AFTER_LAB_DOCKS)))
		{
			AddProgress(ProgressType.UNLOCK_LAB_DOCKS_EXTRA);
		}
		if (onProgressChanged != null)
		{
			onProgressChanged();
		}
		CheckTrackers();
	}

	private void InitDelayedProgressTrackers()
	{
		delayedProgressTrackerDict[ProgressTrackerId.TIME_AFTER_LAB_DOCKS] = new DelayedProgressTracker(this, ProgressTrackerId.TIME_AFTER_LAB_DOCKS, null, new ProgressType[2]
		{
			ProgressType.UNLOCK_LAB,
			ProgressType.UNLOCK_DOCKS
		}, new int[2] { 1, 1 }, 120f, delegate
		{
			SetUniqueProgress(ProgressType.UNLOCK_LAB_DOCKS_EXTRA);
		});
		if (!SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().suppressStory)
		{
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_1] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_1, null, ProgressType.SLIME_DOORS, 0, 3f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_2] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_2, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_1"), ProgressType.SLIME_DOORS, 0, 48f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_3] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_3, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_2"), ProgressType.SLIME_DOORS, 1, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_4] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_4, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_3"), ProgressType.SLIME_DOORS, 2, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_5] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_5, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_4"), ProgressType.SLIME_DOORS, 3, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_6] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_6, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_5"), ProgressType.PLORT_DOOR, 1, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_7] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_7, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_6"), ProgressType.SLIME_DOORS, 4, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_8] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_8, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_7"), ProgressType.SLIME_DOORS, 5, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_9] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_9, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_8"), ProgressType.UNLOCK_DESERT, 1, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_10] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_10, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_9"), ProgressType.SLIME_DOORS, 5, 24f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.CASEY_11] = new DelayedProgressTracker(this, ProgressTrackerId.CASEY_11, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_10"), ProgressType.HOBSON_END, 1, 6f, SendProgressMail);
			delayedProgressTrackerDict[ProgressTrackerId.HOBSON_1] = new DelayedProgressTracker(this, ProgressTrackerId.HOBSON_1, new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_11"), ProgressType.HOBSON_END, 1, 0.167f, SendProgressMail);
		}
	}

	public void QueueCredits()
	{
		readyForCredits = true;
		SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.FINISH_ADVENTURE, 1);
	}

	public void QueueRanchWistfulMusic()
	{
		readyForWistfulMusic = true;
	}

	public void NoteReturnedToRanch()
	{
		if (readyForCredits)
		{
			SRSingleton<GameContext>.Instance.UITemplates.CreateCreditsPrefab(aboutCredits: false);
			readyForCredits = false;
		}
		else if (readyForWistfulMusic)
		{
			SRSingleton<GameContext>.Instance.MusicDirector.SetWistfulRanchMode();
			readyForWistfulMusic = false;
		}
	}

	public void SendProgressMail(ProgressTrackerId trackerId)
	{
		mailDir.SendMailIfExists(MailDirector.Type.PERSONAL, trackerId.ToString().ToLowerInvariant());
	}

	public void MaybeUnlockOgdenMissions()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().enableOgdenMissions && !HasProgress(ProgressType.UNLOCK_OGDEN_MISSIONS) && HasProgress(ProgressType.UNLOCK_MOSS) && HasProgress(ProgressType.UNLOCK_OVERGROWTH))
		{
			AddProgress(ProgressType.UNLOCK_OGDEN_MISSIONS);
			mailDir.SendMail(MailDirector.Type.PERSONAL, "ogden_invite");
		}
	}

	public void MaybeUnlockMochiMissions()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().enableMochiMissions && !HasProgress(ProgressType.UNLOCK_MOCHI_MISSIONS) && HasProgress(ProgressType.UNLOCK_QUARRY) && HasProgress(ProgressType.UNLOCK_GROTTO))
		{
			AddProgress(ProgressType.UNLOCK_MOCHI_MISSIONS);
			mailDir.SendMail(MailDirector.Type.PERSONAL, "mochi_invite");
		}
	}

	public void MaybeUnlockViktorMissions()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().enableViktorMissions && !HasProgress(ProgressType.UNLOCK_VIKTOR_MISSIONS) && HasProgress(ProgressType.UNLOCK_LAB) && HasProgress(ProgressType.UNLOCK_RUINS) && playerState.HasUpgrade(PlayerState.Upgrade.TREASURE_CRACKER_2))
		{
			AddProgress(ProgressType.UNLOCK_VIKTOR_MISSIONS);
			mailDir.SendMail(MailDirector.Type.PERSONAL, "viktor_invite");
		}
	}

	public void Update()
	{
		foreach (DelayedProgressTracker value in delayedProgressTrackerDict.Values)
		{
			value.Update();
		}
	}

	public bool SetUniqueProgress(ProgressType type)
	{
		if (!model.progressDict.ContainsKey(type))
		{
			model.progressDict[type] = 1;
			NoteProgressChanged(type);
			return true;
		}
		return false;
	}

	public void SetProgress(ProgressType type, int count)
	{
		if (!model.progressDict.ContainsKey(type) || model.progressDict[type] != count)
		{
			model.progressDict[type] = count;
			NoteProgressChanged(type);
		}
	}

	public void AddProgress(ProgressType type)
	{
		if (model.progressDict.ContainsKey(type))
		{
			Dictionary<ProgressType, int> progressDict = model.progressDict;
			int value = progressDict[type] + 1;
			progressDict[type] = value;
		}
		else
		{
			model.progressDict[type] = 1;
		}
		NoteProgressChanged(type);
	}

	private void NoteProgressChanged(ProgressType type)
	{
		if (type == ProgressType.CORPORATE_PARTNER)
		{
			int num = model.progressDict[type];
			mailDir.SendMailIfExists(MailDirector.Type.PERSONAL, "rewards_level_" + num);
			if (num >= 28)
			{
				mailDir.SendMailIfExists(MailDirector.Type.PERSONAL, "rewards_level_max");
			}
		}
		switch (type)
		{
		case ProgressType.OGDEN_REWARDS:
			if (model.progressDict[type] >= 3)
			{
				SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(PediaDirector.Id.OGDEN_RETREAT);
			}
			break;
		case ProgressType.MOCHI_REWARDS:
			if (model.progressDict[type] >= 3)
			{
				SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(PediaDirector.Id.MOCHI_MANOR);
			}
			break;
		default:
			_ = 3104;
			break;
		}
		CheckTrackers();
		SRSingleton<SceneContext>.Instance.GadgetDirector.OnProgress(type);
		SRSingleton<SceneContext>.Instance.TutorialDirector.OnProgress(type);
		if (type == ProgressType.HOBSON_END)
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.FIND_HOBSONS_END, 1);
		}
		if (onProgressChanged != null)
		{
			onProgressChanged();
		}
	}

	public void CheckTrackers()
	{
		foreach (DelayedProgressTracker value in delayedProgressTrackerDict.Values)
		{
			value.CheckReady();
		}
	}

	public bool HasProgress(ProgressType type)
	{
		return model.HasProgress(type);
	}

	public int GetProgress(ProgressType type)
	{
		return model.GetProgress(type);
	}

	public static ProgressType GetRancherProgressType(string rancherName)
	{
		return (ProgressType)Enum.Parse(typeof(ProgressType), "EXCHANGE_" + rancherName.ToUpperInvariant());
	}

	private void CheckProgressUpgrades()
	{
		int progress = GetProgress(ProgressType.CORPORATE_PARTNER);
		CheckVacpackUpgrades(progress);
		CheckBlueprintUpgrades(progress);
	}

	private void CheckVacpackUpgrades(int progress)
	{
		if (20 <= progress)
		{
			playerState.AddUpgrade(PlayerState.Upgrade.AMMO_1);
			playerState.AddUpgrade(PlayerState.Upgrade.AMMO_2);
			playerState.AddUpgrade(PlayerState.Upgrade.AMMO_3);
			playerState.AddUpgrade(PlayerState.Upgrade.AMMO_4);
		}
		if (21 <= progress)
		{
			playerState.AddUpgrade(PlayerState.Upgrade.HEALTH_1);
			playerState.AddUpgrade(PlayerState.Upgrade.HEALTH_2);
			playerState.AddUpgrade(PlayerState.Upgrade.HEALTH_3);
			playerState.AddUpgrade(PlayerState.Upgrade.HEALTH_4);
		}
		if (19 <= progress)
		{
			playerState.AddUpgrade(PlayerState.Upgrade.RUN_EFFICIENCY);
			playerState.AddUpgrade(PlayerState.Upgrade.RUN_EFFICIENCY_2);
		}
		if (22 <= progress)
		{
			playerState.AddUpgrade(PlayerState.Upgrade.GOLDEN_SURESHOT);
		}
	}

	private void CheckBlueprintUpgrades(int progress)
	{
		if (20 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.GORDO_SNARE_MASTER);
		}
		if (23 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.EXTRACTOR_DRILL_TITAN);
		}
		if (24 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.EXTRACTOR_PUMP_ABYSSAL);
		}
		if (25 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.EXTRACTOR_APIARY_ROYAL);
		}
		if (11 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.MARKET_LINK);
		}
		if (26 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.LAMP_GOLD);
		}
		if (27 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.WARP_DEPOT_GOLD);
		}
		if (28 <= progress)
		{
			SRSingleton<SceneContext>.Instance.GadgetDirector.AddBlueprint(Gadget.Id.TELEPORTER_GOLD);
		}
	}
}
