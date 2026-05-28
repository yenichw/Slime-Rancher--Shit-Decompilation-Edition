using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PlayerState : SRBehaviour, PlayerModel.Participant
{
	public delegate bool UnlockCondition();

	public enum CoinsType
	{
		NONE = -1,
		NORM = 0,
		MOCHI = 1,
		DRONE = 2
	}

	public class UpgradeLockData
	{
		public bool timedLock;

		public double lockedUntil;

		public UpgradeLockData(bool timedLock, double lockedUntil)
		{
			this.timedLock = timedLock;
			this.lockedUntil = lockedUntil;
		}

		public UpgradeLockData()
		{
		}
	}

	public class UpgradeLocker
	{
		private PlayerState playerState;

		private UnlockCondition unlockCondition;

		private float unlockDelayHrs;

		private bool timedLock;

		private double lockedUntil;

		public UpgradeLocker(PlayerState playerState, UnlockCondition unlockCondition, float unlockDelayHrs)
		{
			this.playerState = playerState;
			this.unlockCondition = unlockCondition;
			this.unlockDelayHrs = unlockDelayHrs;
		}

		public bool CheckUnlockCondition()
		{
			if (!timedLock)
			{
				return unlockCondition();
			}
			return false;
		}

		public bool ReachedUnlockTime()
		{
			if (timedLock)
			{
				return playerState.timeDir.HasReached(lockedUntil);
			}
			return false;
		}

		public void Unlock()
		{
			timedLock = true;
			lockedUntil = playerState.timeDir.HoursFromNow(unlockDelayHrs);
		}

		public void Push(UpgradeLockData data)
		{
			timedLock = data.timedLock;
			lockedUntil = data.lockedUntil;
		}

		public void Pull(out UpgradeLockData data)
		{
			data = new UpgradeLockData(timedLock, lockedUntil);
		}
	}

	public delegate void OnEndGame();

	private enum GameState
	{
		DEFAULT = 0,
		GAME_OVER = 1
	}

	public delegate void OnEndGameTimeChanged();

	public enum AmmoMode
	{
		DEFAULT = 0,
		NIMBLE_VALLEY = 1
	}

	public class AmmoModeComparer : IEqualityComparer<AmmoMode>
	{
		public static AmmoModeComparer Instance = new AmmoModeComparer();

		public bool Equals(AmmoMode a, AmmoMode b)
		{
			return a == b;
		}

		public int GetHashCode(AmmoMode a)
		{
			return (int)a;
		}
	}

	public delegate void OnAmmoModeChanged(AmmoMode mode);

	public enum Upgrade
	{
		HEALTH_1 = 0,
		HEALTH_2 = 1,
		HEALTH_3 = 2,
		ENERGY_1 = 3,
		ENERGY_2 = 4,
		ENERGY_3 = 5,
		AMMO_1 = 6,
		AMMO_2 = 7,
		AMMO_3 = 8,
		JETPACK = 9,
		JETPACK_EFFICIENCY = 10,
		AIR_BURST = 11,
		RUN_EFFICIENCY = 12,
		LIQUID_SLOT = 13,
		AMMO_4 = 14,
		HEALTH_4 = 15,
		RUN_EFFICIENCY_2 = 16,
		GOLDEN_SURESHOT = 17,
		SPARE_KEY = 18,
		TREASURE_CRACKER_1 = 100,
		TREASURE_CRACKER_2 = 101,
		TREASURE_CRACKER_3 = 102,
		TREASURE_CRACKER_4 = 103
	}

	public class UpgradeComparer : IEqualityComparer<Upgrade>
	{
		public bool Equals(Upgrade x, Upgrade y)
		{
			return x == y;
		}

		public int GetHashCode(Upgrade obj)
		{
			return (int)obj;
		}
	}

	public enum GameMode
	{
		CLASSIC = 0,
		TIME_LIMIT = 1,
		CASUAL = 2,
		TIME_LIMIT_V2 = 3
	}

	public class GameModeComparer : IEqualityComparer<GameMode>
	{
		public static GameModeComparer Instance = new GameModeComparer();

		public bool Equals(GameMode a, GameMode b)
		{
			return a == b;
		}

		public int GetHashCode(GameMode a)
		{
			return (int)a;
		}
	}

	private class AvailUpgradePopupCreator : PopupDirector.PopupCreator
	{
		private Upgrade id;

		public AvailUpgradePopupCreator(Upgrade id)
		{
			this.id = id;
		}

		public override void Create()
		{
			AvailUpgradePopupUI.CreateAvailUpgradePopup(id);
		}

		public override bool Equals(object other)
		{
			if (other is AvailUpgradePopupCreator)
			{
				return ((AvailUpgradePopupCreator)other).id == id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override bool ShouldClear()
		{
			return false;
		}
	}

	private GameState gameState;

	public GameObject[] potentialAmmo;

	private PlayerModel model;

	[Tooltip("SFX played each time the player's energy is depleted. (optional)")]
	public SECTR_AudioCue onEnergyDepletedCue;

	public OnAmmoModeChanged onAmmoModeChanged;

	private Dictionary<AmmoMode, Ammo> ammoDict;

	private AmmoMode ammoMode;

	public static UpgradeComparer upgradeComparer = new UpgradeComparer();

	private TimeDirector timeDir;

	private PopupDirector popupDir;

	private AchievementsDirector achieveDir;

	private MailDirector mailDir;

	private MetadataDirector metadataDirector;

	private static readonly Predicate<Identifiable.Id> NO_LIQUID = (Identifiable.Id id) => !Identifiable.IsLiquid(id);

	private static readonly Predicate<Identifiable.Id> ONLY_LIQUID = (Identifiable.Id id) => Identifiable.IsLiquid(id);

	public static readonly Predicate<Identifiable.Id>[] PLAYER_AMMO_PREDS = new Predicate<Identifiable.Id>[5] { NO_LIQUID, NO_LIQUID, NO_LIQUID, NO_LIQUID, ONLY_LIQUID };

	public bool PointedAtVaccable { get; set; }

	public GameObject Targeting { get; set; }

	public bool InGadgetMode { get; set; }

	public Ammo Ammo => ammoDict[ammoMode];

	public double nextAmmoLossDamageTime { get; set; }

	public event OnEndGame onEndGame = delegate
	{
	};

	public event OnEndGameTimeChanged onEndGameTimeChanged = delegate
	{
	};

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		popupDir = SRSingleton<SceneContext>.Instance.PopupDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		metadataDirector = SRSingleton<SceneContext>.Instance.MetadataDirector;
	}

	public void InitModel(PlayerModel model)
	{
		Reset(model);
		model.ammoDict[AmmoMode.DEFAULT] = new AmmoModel();
		model.ammoDict[AmmoMode.NIMBLE_VALLEY] = new AmmoModel();
		ammoDict[AmmoMode.DEFAULT].InitModel(model.ammoDict[AmmoMode.DEFAULT]);
		ammoDict[AmmoMode.NIMBLE_VALLEY].InitModel(model.ammoDict[AmmoMode.NIMBLE_VALLEY]);
	}

	public void SetModel(PlayerModel model)
	{
		this.model = model;
		RegisteredPotentialAmmoChanged(this.model.registeredPotentialAmmo);
		ammoDict[AmmoMode.DEFAULT].SetModel(model.ammoDict[AmmoMode.DEFAULT]);
		ammoDict[AmmoMode.NIMBLE_VALLEY].SetModel(model.ammoDict[AmmoMode.NIMBLE_VALLEY]);
		CheckAllUpgradeLockers();
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<AmmoMode, List<GameObject>> registeredPotentialAmmo)
	{
		if (ammoDict == null || registeredPotentialAmmo == null)
		{
			return;
		}
		foreach (KeyValuePair<AmmoMode, List<GameObject>> pair in registeredPotentialAmmo)
		{
			pair.Value.ForEach(delegate(GameObject p)
			{
				ammoDict[pair.Key].RegisterPotentialAmmo(p);
			});
		}
	}

	public void KeyAdded()
	{
		SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(PediaDirector.Id.KEYS);
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
	}

	public void TransformChanged(Vector3 pos, Quaternion rot)
	{
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
	}

	public HashSet<Identifiable.Id> GetPotentialAmmo()
	{
		return new HashSet<Identifiable.Id>(potentialAmmo.Select((GameObject go) => Identifiable.GetId(go)), Identifiable.idComparer);
	}

	private void Reset(PlayerModel model)
	{
		model.Reset(SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings());
		ammoDict = new Dictionary<AmmoMode, Ammo>(AmmoModeComparer.Instance)
		{
			{
				AmmoMode.DEFAULT,
				new Ammo(GetPotentialAmmo(), 5, 4, PLAYER_AMMO_PREDS, GetMaxAmmo_Default)
			},
			{
				AmmoMode.NIMBLE_VALLEY,
				new Ammo(new HashSet<Identifiable.Id>(Identifiable.idComparer)
				{
					Identifiable.Id.QUICKSILVER_PLORT,
					Identifiable.Id.VALLEY_AMMO_1,
					Identifiable.Id.VALLEY_AMMO_2,
					Identifiable.Id.VALLEY_AMMO_3,
					Identifiable.Id.VALLEY_AMMO_4
				}, 3, 3, new Predicate<Identifiable.Id>[3]
				{
					(Identifiable.Id id) => id == Identifiable.Id.QUICKSILVER_PLORT,
					(Identifiable.Id id) => id == Identifiable.Id.VALLEY_AMMO_1,
					(Identifiable.Id id) => id == Identifiable.Id.VALLEY_AMMO_2 || id == Identifiable.Id.VALLEY_AMMO_3 || id == Identifiable.Id.VALLEY_AMMO_4
				}, GetMaxAmmo_NimbleValley)
			}
		};
		SetAmmoMode(AmmoMode.DEFAULT);
		InitUpgradeLocks(model);
		model.upgrades.Clear();
		InitZoneMaps(model);
	}

	private int GetMaxAmmo_Default(Identifiable.Id id, int index)
	{
		switch (id)
		{
		case Identifiable.Id.GLITCH_DEBUG_SPRAY_LIQUID:
			return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.debugSprayMaxAmmo;
		case Identifiable.Id.GLITCH_SLIME:
		case Identifiable.Id.GLITCH_BUG_REPORT:
			return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.slimeMaxAmmo;
		default:
			return model.maxAmmo;
		}
	}

	private int GetMaxAmmo_NimbleValley(Identifiable.Id id, int index)
	{
		switch (index)
		{
		case 0:
			return 250;
		case 1:
			return 100;
		case 2:
			return 3;
		default:
			throw new ArgumentException();
		}
	}

	private void InitZoneMaps(PlayerModel model)
	{
		model.unlockedZoneMaps.Clear();
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.RANCH);
	}

	public void UnlockMap(ZoneDirector.Zone zone)
	{
		model.unlockedZoneMaps.Add(zone);
	}

	public void LockAllMaps()
	{
		model.unlockedZoneMaps.Clear();
	}

	public void UnlockAllMaps()
	{
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.MOSS);
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.DESERT);
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.QUARRY);
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.REEF);
		model.unlockedZoneMaps.Add(ZoneDirector.Zone.RUINS);
	}

	public bool HasUnlockedMap(ZoneDirector.Zone zone)
	{
		return model.unlockedZoneMaps.Contains(zone);
	}

	private void InitUpgradeLocks(PlayerModel model)
	{
		model.availUpgrades.Clear();
		model.availUpgrades.Add(Upgrade.HEALTH_1);
		model.availUpgrades.Add(Upgrade.ENERGY_1);
		model.availUpgrades.Add(Upgrade.AMMO_1);
		model.availUpgrades.Add(Upgrade.JETPACK);
		model.availUpgrades.Add(Upgrade.LIQUID_SLOT);
		model.upgradeLocks.Clear();
		model.upgradeLocks[Upgrade.RUN_EFFICIENCY] = CreateBasicLock(null, null, 48f);
		model.upgradeLocks[Upgrade.AIR_BURST] = CreateBasicLock(null, null, 72f);
		model.upgradeLocks[Upgrade.JETPACK_EFFICIENCY] = CreateBasicLock(Upgrade.JETPACK, null, 120f);
		model.upgradeLocks[Upgrade.HEALTH_2] = CreateBasicLock(Upgrade.HEALTH_1, null, 48f);
		model.upgradeLocks[Upgrade.HEALTH_3] = CreateBasicLock(Upgrade.HEALTH_2, null, 72f);
		model.upgradeLocks[Upgrade.ENERGY_2] = CreateBasicLock(Upgrade.ENERGY_1, null, 48f);
		model.upgradeLocks[Upgrade.ENERGY_3] = CreateBasicLock(Upgrade.ENERGY_2, null, 72f);
		model.upgradeLocks[Upgrade.AMMO_2] = CreateBasicLock(Upgrade.AMMO_1, null, 48f);
		model.upgradeLocks[Upgrade.AMMO_3] = CreateBasicLock(Upgrade.AMMO_2, null, 72f);
		model.upgradeLocks[Upgrade.TREASURE_CRACKER_1] = CreateBasicLock(null, () => achieveDir.GetGameIntStat(AchievementsDirector.GameIntStat.FABRICATED_GADGETS) >= 1, 5f);
		model.upgradeLocks[Upgrade.TREASURE_CRACKER_2] = CreateBasicLock(Upgrade.TREASURE_CRACKER_1, () => achieveDir.GetGameIntStat(AchievementsDirector.GameIntStat.FABRICATED_GADGETS) >= 20, 1f);
		model.upgradeLocks[Upgrade.TREASURE_CRACKER_3] = CreateBasicLock(Upgrade.TREASURE_CRACKER_2, () => achieveDir.GetGameIntStat(AchievementsDirector.GameIntStat.FABRICATED_GADGETS) >= 50, 1f);
		model.upgradeLocks[Upgrade.SPARE_KEY] = CreateBasicLock(null, () => mailDir.HasReadMail(new MailDirector.Mail(MailDirector.Type.PERSONAL, "casey_11")), 3f);
	}

	private void CheckAllUpgradeLockers()
	{
		foreach (KeyValuePair<Upgrade, UpgradeLocker> upgradeLock in model.upgradeLocks)
		{
			if (upgradeLock.Value.CheckUnlockCondition())
			{
				upgradeLock.Value.Unlock();
			}
		}
	}

	private UpgradeLocker CreateBasicLock(Upgrade? waitForUpgrade, UnlockCondition extraCondition, float delayHrs)
	{
		return new UpgradeLocker(this, () => (!waitForUpgrade.HasValue || HasUpgrade(waitForUpgrade.Value)) && (extraCondition == null || extraCondition()), delayHrs);
	}

	public double? GetEndGameTimeRemaining()
	{
		if (model.endGameTime.HasValue)
		{
			double num = timeDir.WorldTime();
			double num2 = model.endGameTime.Value - num;
			return (num2 > 0.0) ? num2 : 0.0;
		}
		return null;
	}

	public double? GetEndGameTime()
	{
		return model.endGameTime;
	}

	public void SetEndGameTime(double time)
	{
		model.endGameTime = time;
		this.onEndGameTimeChanged();
	}

	public bool IsGameOver()
	{
		if (model.endGameTime.HasValue)
		{
			return timeDir.HasReached(model.endGameTime.Value);
		}
		return false;
	}

	public void Update()
	{
		model.Recover();
		List<Upgrade> list = new List<Upgrade>();
		foreach (KeyValuePair<Upgrade, UpgradeLocker> upgradeLock in model.upgradeLocks)
		{
			if (upgradeLock.Value.ReachedUnlockTime())
			{
				list.Add(upgradeLock.Key);
				if (!HasUpgrade(upgradeLock.Key) && !model.availUpgrades.Contains(upgradeLock.Key))
				{
					model.availUpgrades.Add(upgradeLock.Key);
					popupDir.QueueForPopup(new AvailUpgradePopupCreator(upgradeLock.Key));
					popupDir.MaybePopupNext();
				}
			}
		}
		foreach (Upgrade item in list)
		{
			model.upgradeLocks.Remove(item);
		}
		if (gameState == GameState.DEFAULT && IsGameOver())
		{
			gameState = GameState.GAME_OVER;
			this.onEndGame();
			UnityEngine.Object.Instantiate(SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().endGameUIPrefab);
		}
	}

	public int GetCurrEnergy()
	{
		return Mathf.FloorToInt(model.currEnergy);
	}

	public int GetCurrHealth()
	{
		return Mathf.FloorToInt(model.currHealth);
	}

	public int GetMaxHealth()
	{
		return model.maxHealth;
	}

	public int GetMaxEnergy()
	{
		return model.maxEnergy;
	}

	public int GetCurrRad()
	{
		return Mathf.CeilToInt(Mathf.Min(model.currRads, model.maxRads));
	}

	public AmmoMode GetAmmoMode()
	{
		return ammoMode;
	}

	public IEnumerable<KeyValuePair<AmmoMode, Ammo>> GetAmmoDict()
	{
		return ammoDict;
	}

	public Ammo GetAmmo(AmmoMode mode)
	{
		return ammoDict[mode];
	}

	public void SetEnergy(int energy)
	{
		model.SetEnergy(energy);
	}

	public void SetRad(int rad)
	{
		model.SetRad(rad);
	}

	public void SetHealth(int health)
	{
		model.SetHealth(health);
	}

	public void SetAmmoMode(AmmoMode mode)
	{
		if (ammoMode != mode)
		{
			ammoMode = mode;
			if (onAmmoModeChanged != null)
			{
				onAmmoModeChanged(mode);
			}
		}
	}

	public int AddRads(float rads)
	{
		return model.AddRads(rads);
	}

	public void RemoveRads(float rads)
	{
		model.currRads -= rads;
		model.radRecoverAfter = timeDir.WorldTime();
	}

	public bool CanBeDamaged()
	{
		if (!SRSingleton<SceneContext>.Instance.TimeDirector.IsFastForwarding())
		{
			return SRInput.Instance.GetInputMode() == SRInput.InputMode.DEFAULT;
		}
		return false;
	}

	public bool Damage(int healthLoss, GameObject source)
	{
		if (CanBeDamaged())
		{
			model.LoseHealth(healthLoss);
			if (timeDir.HasReached(nextAmmoLossDamageTime))
			{
				metadataDirector.Glitch.MaybeDamageExposure(source);
			}
			if (model.currHealth <= 0f)
			{
				model.currHealth = 0f;
				model.healthBurstAfter = double.PositiveInfinity;
				return true;
			}
			return false;
		}
		return false;
	}

	public void Heal(int healthGain)
	{
		model.currHealth = Mathf.Clamp(model.currHealth + (float)healthGain, 0f, model.maxHealth);
		model.healthBurstAfter = timeDir.WorldTime();
	}

	public void SpendEnergy(float energy)
	{
		model.SpendEnergy(energy);
		if (GetCurrEnergy() <= 0)
		{
			SECTR_AudioSystem.Play(onEnergyDepletedCue, base.transform.position, loop: false);
		}
	}

	public void AddCurrency(int adjust, CoinsType coinsType = CoinsType.NORM)
	{
		model.currency += adjust;
		model.currencyEverCollected += adjust;
		if (adjust > 0)
		{
			achieveDir.AddToStat(AchievementsDirector.IntStat.DAY_CURRENCY, adjust);
			achieveDir.AddToStat(AchievementsDirector.IntStat.CURRENCY, adjust);
		}
		SRSingleton<PopupElementsUI>.Instance.CreateCoinsPopup(adjust, coinsType);
	}

	public void AddCurrencyDisplayDelta(int adjust)
	{
		model.currencyDisplayDelta += adjust;
	}

	public void SetCurrencyDisplay(int? currencyDisplay)
	{
		model.currencyDisplayOverride = currencyDisplay;
	}

	public int GetDisplayedCurrency()
	{
		if (model.currencyDisplayOverride.HasValue)
		{
			return model.currencyDisplayOverride.Value;
		}
		return model.currency + model.currencyDisplayDelta;
	}

	public void SpendCurrency(int adjust, bool forcedLoss = false)
	{
		if (model.currency < adjust)
		{
			throw new ArgumentException("Attempting to spend more currency than we have.");
		}
		model.currency -= adjust;
		if (!forcedLoss)
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.GameIntStat.CURRENCY_SPENT, adjust);
		}
		SRSingleton<PopupElementsUI>.Instance.CreateCoinsPopup(-adjust, CoinsType.NORM);
	}

	public int GetCurrency()
	{
		return model.currency;
	}

	public void AddKey()
	{
		model.AddKey();
	}

	public bool SpendKey()
	{
		if (model.keys >= 1)
		{
			model.keys--;
			return true;
		}
		return false;
	}

	public int GetKeys()
	{
		return model.keys;
	}

	public void OnGameIntStatChanged(AchievementsDirector.GameIntStat stat, int val)
	{
		if (stat == AchievementsDirector.GameIntStat.FABRICATED_GADGETS)
		{
			CheckAllUpgradeLockers();
		}
	}

	public void OnMailRead()
	{
		CheckAllUpgradeLockers();
	}

	public void AddUpgrade(Upgrade upgrade, bool isFirstTime = false)
	{
		if (!model.upgrades.Contains(upgrade))
		{
			model.upgrades.Add(upgrade);
			model.ApplyUpgrade(upgrade, isFirstTime);
			CheckAllUpgradeLockers();
			if (upgrade == Upgrade.LIQUID_SLOT)
			{
				SRSingleton<SceneContext>.Instance.TutorialDirector.OnLiquidSlotGained();
			}
		}
	}

	public bool HasOrCanGetUpgrade(Upgrade upgrade)
	{
		if (!HasUpgrade(upgrade))
		{
			return CanGetUpgrade(upgrade);
		}
		return true;
	}

	public bool HasUpgrade(Upgrade upgrade)
	{
		return model.upgrades.Contains(upgrade);
	}

	public bool CanGetUpgrade(Upgrade upgrade)
	{
		if (!HasUpgrade(upgrade))
		{
			return model.availUpgrades.Contains(upgrade);
		}
		return false;
	}

	public void OnEnteredZone(ZoneDirector.Zone zone)
	{
		ammoDict[AmmoMode.NIMBLE_VALLEY].Clear(delegate(int ii)
		{
			Identifiable.Id slotName = ammoDict[AmmoMode.NIMBLE_VALLEY].GetSlotName(ii);
			return ((uint)(slotName - 171) <= 3u) ? true : false;
		});
	}

	public void OnExitedZone(ZoneDirector.Zone zone)
	{
	}
}
