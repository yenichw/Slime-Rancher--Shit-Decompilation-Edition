using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class AchievementsDirector : MonoBehaviour, GameAchievesModel.Participant, ProfileAchievesModel.Participant
{
	public enum Achievement
	{
		SELL_PLORTS_A = 0,
		SELL_PLORTS_B = 1,
		SELL_PLORTS_C = 2,
		SELL_PLORTS_D = 3,
		SELL_PLORTS_E = 4,
		FEED_SLIMES_CHICKENS = 5,
		FRUIT_TREE_TYPES = 6,
		VEGGIE_PATCH_TYPES = 7,
		EARN_CURRENCY_A = 8,
		EARN_CURRENCY_B = 9,
		EARN_CURRENCY_C = 10,
		DAY_CURRENCY = 11,
		AWAKE_UNTIL_MORNING = 12,
		KNOCKOUT_MORNING = 13,
		AWAY_FROM_RANCH = 14,
		PINK_SLIMES_FOOD_TYPES = 15,
		FEED_AIRBORNE = 16,
		DISCOVERED_QUARRY = 17,
		DISCOVERED_MOSS = 18,
		DISCOVERED_DESERT = 19,
		BURST_GORDO = 20,
		OPEN_SLIME_GATE = 21,
		INCINERATE_ELDER_CHICKEN = 22,
		FEED_FAVORITES = 23,
		FILLED_SILO = 24,
		RANCH_UPGRADED_STORAGE = 25,
		FULFILL_EXCHANGE_EARLY = 26,
		DAY_COLLECT_PLORTS = 27,
		GOLD_SLIME_TRIPLE_PLORT = 28,
		EXTENDED_RAD_EXPOSURE = 29,
		EXTENDED_TARR_HOLD = 30,
		TABBY_HEADBUTT = 31,
		LAUNCHED_BOOM_EXPLODE = 32,
		MANY_SLIMES_IN_VAC = 33,
		CORRAL_SLIME_TYPES = 34,
		CORRAL_LARGO_TYPES = 35,
		POND_SLIME_TYPES = 36,
		RANCH_LARGO_TYPES = 37,
		ENTERED_CORRAL_SLIMES = 38,
		INCINERATE_CHICK = 39,
		TIME_LIMIT_CURRENCY_A = 40,
		TIME_LIMIT_CURRENCY_B = 41,
		TIME_LIMIT_CURRENCY_C = 42,
		DISCOVERED_RUINS = 43,
		FABRICATE_GADGETS_A = 44,
		FABRICATE_GADGETS_B = 45,
		FABRICATE_GADGETS_C = 46,
		SLIME_STAGE_TARR = 47,
		SLIMEBALL_SCORE = 48,
		JOIN_REWARDS_CLUB = 49,
		USE_CHROMAS = 50,
		COLLECT_SLIME_TOYS = 51,
		FIND_HOBSONS_END = 52,
		SNARE_HUNTER_GORDO = 53,
		ACTIVATE_OASIS = 54,
		FINISH_ADVENTURE = 55,
		COMPLETE_SLIMEPEDIA = 56
	}

	public class AchievementComparer : IEqualityComparer<Achievement>
	{
		public static AchievementComparer Instance = new AchievementComparer();

		public bool Equals(Achievement a, Achievement b)
		{
			return a == b;
		}

		public int GetHashCode(Achievement a)
		{
			return (int)a;
		}
	}

	public enum BoolStat
	{

	}

	public enum IntStat
	{
		PLORTS_SOLD = 0,
		CHICKENS_FED_SLIMES = 1,
		DAY_CURRENCY = 2,
		CURRENCY = 3,
		DEATH_BEFORE_10AM = 4,
		FED_AIRBORNE = 5,
		VISITED_QUARRY = 6,
		VISITED_MOSS = 7,
		VISITED_DESERT = 8,
		BURST_GORDOS = 9,
		OPENED_SLIME_GATES = 10,
		INCINERATED_ELDER_CHICKENS = 11,
		FED_FAVORITE = 12,
		FILLED_SILO = 13,
		FULFILL_EXCHANGE_EARLY = 14,
		DAY_COLLECT_PLORTS = 15,
		GOLD_SLIME_TRIPLE_PLORT = 16,
		EXTENDED_RAD_EXPOSURE = 17,
		EXTENDED_TARR_HOLD = 18,
		TABBY_HEADBUTT = 19,
		LAUNCHED_BOOM_EXPLODE = 20,
		SLIMES_IN_VAC = 21,
		CORRAL_SLIME_TYPES = 22,
		CORRAL_LARGO_TYPES = 23,
		POND_SLIME_TYPES = 24,
		RANCH_LARGO_TYPES = 25,
		ENTERED_CORRAL_SLIMES = 26,
		INCINERATED_CHICKS = 27,
		[Obsolete("use TIME_LIMIT_V2_CURRENCY", true)]
		TIME_LIMIT_CURRENCY = 28,
		SLIMEBALL_SCORE = 29,
		SLIME_STAGE_TARRS = 30,
		VISITED_RUINS = 31,
		REWARD_LEVELS = 32,
		SNARED_HUNTER_GORDOS = 33,
		ACTIVATED_OASES = 34,
		COMPLETED_SLIMEPEDIA = 35,
		FIND_HOBSONS_END = 36,
		FINISH_ADVENTURE = 37,
		TIME_LIMIT_V2_CURRENCY = 38
	}

	public enum EnumStat
	{
		PINK_SLIMES_FOOD_TYPES = 0,
		RANCH_FRUIT_TYPES = 1,
		RANCH_VEGGIE_TYPES = 2,
		SLIME_TOYS_BOUGHT = 3,
		USE_CHROMAS = 4
	}

	public enum GameFloatStat
	{

	}

	public enum GameDoubleStat
	{
		LAST_LEFT_RANCH = 0,
		LAST_ENTERED_RANCH = 1,
		LAST_SLEPT = 2,
		LAST_AWOKE = 3
	}

	public enum GameIntStat
	{
		DEATHS = 0,
		UPGRADES_PURCHASED = 1,
		CURRENCY_SPENT = 2,
		FABRICATED_GADGETS = 3
	}

	public enum GameIdDictStat
	{
		PLORT_TYPES_SOLD = 0
	}

	public interface Updatable
	{
		void Update();
	}

	public abstract class Tracker
	{
		public AchievementsDirector dir { get; private set; }

		public Achievement achievement { get; private set; }

		public Tracker(AchievementsDirector dir, Achievement achievement)
		{
			this.dir = dir;
			this.achievement = achievement;
		}

		public abstract bool Reached();

		public abstract void GetProgress(out int progress, out int outOf);

		public virtual bool IsTracking(BoolStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(IntStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(EnumStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(GameFloatStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(GameDoubleStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(GameIntStat stat)
		{
			return false;
		}

		public virtual bool IsTracking(GameIdDictStat stat)
		{
			return false;
		}
	}

	public class BoolTracker : Tracker
	{
		protected BoolStat stat;

		public BoolTracker(AchievementsDirector dir, Achievement achievement, BoolStat stat)
			: base(dir, achievement)
		{
			this.stat = stat;
		}

		public override bool Reached()
		{
			if (base.dir.profileAchievesModel.boolStatDict.ContainsKey(stat))
			{
				return base.dir.profileAchievesModel.boolStatDict[stat];
			}
			return false;
		}

		public override void GetProgress(out int progress, out int outOf)
		{
			progress = ((base.dir.profileAchievesModel.boolStatDict.ContainsKey(stat) && base.dir.profileAchievesModel.boolStatDict[stat]) ? 1 : 0);
			outOf = 1;
		}

		public override bool IsTracking(BoolStat stat)
		{
			return this.stat == stat;
		}
	}

	public class CountTracker : Tracker
	{
		protected int count;

		protected IntStat stat;

		public CountTracker(AchievementsDirector dir, Achievement achievement, IntStat stat, int count)
			: base(dir, achievement)
		{
			this.count = count;
			this.stat = stat;
		}

		public override bool Reached()
		{
			if (base.dir.profileAchievesModel.intStatDict.ContainsKey(stat))
			{
				return base.dir.profileAchievesModel.intStatDict[stat] >= count;
			}
			return false;
		}

		public override void GetProgress(out int progress, out int outOf)
		{
			progress = (base.dir.profileAchievesModel.intStatDict.ContainsKey(stat) ? Math.Min(count, base.dir.profileAchievesModel.intStatDict[stat]) : 0);
			outOf = count;
		}

		public override bool IsTracking(IntStat stat)
		{
			return this.stat == stat;
		}
	}

	public class GameCountTracker : Tracker
	{
		protected int count;

		protected GameIntStat stat;

		public GameCountTracker(AchievementsDirector dir, Achievement achievement, GameIntStat stat, int count)
			: base(dir, achievement)
		{
			this.count = count;
			this.stat = stat;
		}

		public override bool Reached()
		{
			if (base.dir.gameAchievesModel.gameIntStatDict.ContainsKey(stat))
			{
				return base.dir.gameAchievesModel.gameIntStatDict[stat] >= count;
			}
			return false;
		}

		public override void GetProgress(out int progress, out int outOf)
		{
			progress = (base.dir.gameAchievesModel.gameIntStatDict.ContainsKey(stat) ? Math.Min(count, base.dir.gameAchievesModel.gameIntStatDict[stat]) : 0);
			outOf = count;
		}

		public override bool IsTracking(GameIntStat stat)
		{
			return this.stat == stat;
		}
	}

	public class DailyCountTracker : CountTracker, Updatable
	{
		private int lastDay;

		public DailyCountTracker(AchievementsDirector dir, Achievement achievement, IntStat stat, int count)
			: base(dir, achievement, stat, count)
		{
			dir.RegisterUpdatable(this);
		}

		public void Update()
		{
			int num = base.dir.timeDir.CurrDay();
			if (num > lastDay)
			{
				base.dir.ResetStat(stat);
			}
			lastDay = num;
		}
	}

	public class CountEnumsTracker : Tracker
	{
		protected int count;

		protected EnumStat stat;

		public CountEnumsTracker(AchievementsDirector dir, Achievement achievement, EnumStat stat, int count)
			: base(dir, achievement)
		{
			this.count = count;
			this.stat = stat;
		}

		public override bool Reached()
		{
			if (base.dir.profileAchievesModel.enumStatDict.ContainsKey(stat))
			{
				return base.dir.profileAchievesModel.enumStatDict[stat].Count >= count;
			}
			return false;
		}

		public override void GetProgress(out int progress, out int outOf)
		{
			progress = (base.dir.profileAchievesModel.enumStatDict.ContainsKey(stat) ? Math.Min(count, base.dir.profileAchievesModel.enumStatDict[stat].Count) : 0);
			outOf = count;
		}

		public override bool IsTracking(EnumStat stat)
		{
			return this.stat == stat;
		}
	}

	public class SimpleTracker : Tracker
	{
		public delegate bool ReachedDelegate();

		private ReachedDelegate reachedDel;

		private GameDoubleStat[] stats;

		public SimpleTracker(AchievementsDirector dir, Achievement achievement, ReachedDelegate reachedDel, params GameDoubleStat[] stats)
			: base(dir, achievement)
		{
			this.reachedDel = reachedDel;
			this.stats = stats;
		}

		public override bool Reached()
		{
			return reachedDel();
		}

		public override void GetProgress(out int progress, out int outOf)
		{
			progress = (base.dir.profileAchievesModel.earnedAchievements.Contains(base.achievement) ? 1 : 0);
			outOf = 1;
		}

		public override bool IsTracking(GameDoubleStat stat)
		{
			GameDoubleStat[] array = stats;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == stat)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class UpdatableSimpleTracker : SimpleTracker, Updatable
	{
		public delegate void UpdateDelegate();

		private UpdateDelegate updateDel;

		public UpdatableSimpleTracker(AchievementsDirector dir, Achievement achievement, ReachedDelegate reachedDel, UpdateDelegate updateDel, params GameDoubleStat[] stats)
			: base(dir, achievement, reachedDel, stats)
		{
			dir.RegisterUpdatable(this);
			this.updateDel = updateDel;
		}

		public void Update()
		{
			updateDel();
		}
	}

	public GameObject achievementAwardUIPrefab;

	public GameObject achievementsPanelPrefab;

	public Sprite tier1DefaultIcon;

	public Sprite tier2DefaultIcon;

	public Sprite tier3DefaultIcon;

	private float availableAchievementCountDivisor = 1f;

	public HashSet<Achievement> TIER_1 = new HashSet<Achievement>
	{
		Achievement.SELL_PLORTS_A,
		Achievement.SELL_PLORTS_B,
		Achievement.FEED_SLIMES_CHICKENS,
		Achievement.FRUIT_TREE_TYPES,
		Achievement.VEGGIE_PATCH_TYPES,
		Achievement.EARN_CURRENCY_A,
		Achievement.AWAKE_UNTIL_MORNING,
		Achievement.KNOCKOUT_MORNING,
		Achievement.AWAY_FROM_RANCH,
		Achievement.FEED_AIRBORNE,
		Achievement.PINK_SLIMES_FOOD_TYPES,
		Achievement.TABBY_HEADBUTT,
		Achievement.INCINERATE_ELDER_CHICKEN,
		Achievement.FEED_FAVORITES,
		Achievement.FILLED_SILO,
		Achievement.INCINERATE_CHICK,
		Achievement.TIME_LIMIT_CURRENCY_A,
		Achievement.FABRICATE_GADGETS_A,
		Achievement.SLIME_STAGE_TARR,
		Achievement.JOIN_REWARDS_CLUB,
		Achievement.USE_CHROMAS
	};

	public HashSet<Achievement> TIER_2 = new HashSet<Achievement>
	{
		Achievement.SELL_PLORTS_C,
		Achievement.SELL_PLORTS_D,
		Achievement.EARN_CURRENCY_B,
		Achievement.LAUNCHED_BOOM_EXPLODE,
		Achievement.MANY_SLIMES_IN_VAC,
		Achievement.DISCOVERED_QUARRY,
		Achievement.DISCOVERED_MOSS,
		Achievement.DISCOVERED_RUINS,
		Achievement.OPEN_SLIME_GATE,
		Achievement.BURST_GORDO,
		Achievement.EXTENDED_RAD_EXPOSURE,
		Achievement.DAY_COLLECT_PLORTS,
		Achievement.FULFILL_EXCHANGE_EARLY,
		Achievement.RANCH_UPGRADED_STORAGE,
		Achievement.ENTERED_CORRAL_SLIMES,
		Achievement.POND_SLIME_TYPES,
		Achievement.CORRAL_SLIME_TYPES,
		Achievement.CORRAL_LARGO_TYPES,
		Achievement.TIME_LIMIT_CURRENCY_B,
		Achievement.FABRICATE_GADGETS_B,
		Achievement.COLLECT_SLIME_TOYS,
		Achievement.SNARE_HUNTER_GORDO,
		Achievement.ACTIVATE_OASIS
	};

	public HashSet<Achievement> TIER_3 = new HashSet<Achievement>
	{
		Achievement.SELL_PLORTS_E,
		Achievement.EARN_CURRENCY_C,
		Achievement.DAY_CURRENCY,
		Achievement.DISCOVERED_DESERT,
		Achievement.EXTENDED_TARR_HOLD,
		Achievement.GOLD_SLIME_TRIPLE_PLORT,
		Achievement.RANCH_LARGO_TYPES,
		Achievement.TIME_LIMIT_CURRENCY_C,
		Achievement.FABRICATE_GADGETS_C,
		Achievement.SLIMEBALL_SCORE,
		Achievement.FIND_HOBSONS_END,
		Achievement.FINISH_ADVENTURE,
		Achievement.COMPLETE_SLIMEPEDIA
	};

	private Dictionary<PlayerState.GameMode, HashSet<Achievement>> GAME_MODE_ACHIEVEMENTS = new Dictionary<PlayerState.GameMode, HashSet<Achievement>>(PlayerState.GameModeComparer.Instance) { 
	{
		PlayerState.GameMode.TIME_LIMIT_V2,
		new HashSet<Achievement>(AchievementComparer.Instance)
		{
			Achievement.TIME_LIMIT_CURRENCY_A,
			Achievement.TIME_LIMIT_CURRENCY_B,
			Achievement.TIME_LIMIT_CURRENCY_C
		}
	} };

	private Dictionary<PlayerState.GameMode, HashSet<IntStat>> GAME_MODE_INT_STATS = new Dictionary<PlayerState.GameMode, HashSet<IntStat>>(PlayerState.GameModeComparer.Instance) { 
	{
		PlayerState.GameMode.TIME_LIMIT_V2,
		new HashSet<IntStat> { IntStat.TIME_LIMIT_V2_CURRENCY }
	} };

	private Dictionary<PlayerState.GameMode, HashSet<BoolStat>> GAME_MODE_BOOL_STATS = new Dictionary<PlayerState.GameMode, HashSet<BoolStat>>(PlayerState.GameModeComparer.Instance) { 
	{
		PlayerState.GameMode.TIME_LIMIT_V2,
		new HashSet<BoolStat>()
	} };

	private Dictionary<PlayerState.GameMode, HashSet<EnumStat>> GAME_MODE_ENUM_STATS = new Dictionary<PlayerState.GameMode, HashSet<EnumStat>>(PlayerState.GameModeComparer.Instance) { 
	{
		PlayerState.GameMode.TIME_LIMIT_V2,
		new HashSet<EnumStat>()
	} };

	private Dictionary<Achievement, Tracker> trackers = new Dictionary<Achievement, Tracker>();

	private TimeDirector timeDir;

	private GameModel gameModel;

	private AchievementAwardUI currPopup;

	private bool quitting;

	private int suppressors;

	private Queue<Achievement> popupQueue = new Queue<Achievement>();

	private List<Updatable> updatables = new List<Updatable>();

	private HashSet<Achievement> postUpdateAchievementChecks = new HashSet<Achievement>();

	private ProfileAchievesModel profileAchievesModel;

	private GameAchievesModel gameAchievesModel;

	public void Awake()
	{
		availableAchievementCountDivisor = 1f / (float)Enum.GetNames(typeof(Achievement)).Length;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		gameModel = SRSingleton<SceneContext>.Instance.GameModel;
	}

	public void InitForLevel()
	{
		gameModel.RegisterProfileAchievements(this);
		gameModel.RegisterGameAchievements(this);
	}

	public void InitModel(GameAchievesModel gameAchievesModel)
	{
		gameAchievesModel.Reset();
	}

	public void SetModel(GameAchievesModel gameAchievesModel)
	{
		this.gameAchievesModel = gameAchievesModel;
		if (this.gameAchievesModel != null && profileAchievesModel != null)
		{
			InitTrackers();
		}
	}

	public void InitModel(ProfileAchievesModel profileAchievesModel)
	{
		profileAchievesModel.Reset();
	}

	public void SetModel(ProfileAchievesModel profileAchievesModel)
	{
		this.profileAchievesModel = profileAchievesModel;
		if (gameAchievesModel != null && this.profileAchievesModel != null)
		{
			InitTrackers();
		}
	}

	public static void SyncAchievements(ProfileAchievesModel profileAchievesModel)
	{
		Log.Debug("Syncing achievements", "count", profileAchievesModel.earnedAchievements.Count);
	}

	public void ResetProfile()
	{
		profileAchievesModel.Reset();
		InitTrackers();
	}

	public void Update()
	{
		foreach (Updatable updatable in updatables)
		{
			updatable.Update();
		}
	}

	public void LateUpdate()
	{
		if (!postUpdateAchievementChecks.Any() || Levels.isSpecialNonAlloc())
		{
			return;
		}
		foreach (Achievement postUpdateAchievementCheck in postUpdateAchievementChecks)
		{
			if (HasAchievement(postUpdateAchievementCheck) || trackers[postUpdateAchievementCheck].Reached())
			{
				AwardAchievement(postUpdateAchievementCheck);
			}
		}
		postUpdateAchievementChecks.Clear();
	}

	private float CalculateGameProgress(int earnedAchievements)
	{
		return (float)earnedAchievements * availableAchievementCountDivisor;
	}

	private void RegisterUpdatable(Updatable updatable)
	{
		updatables.Add(updatable);
	}

	private bool AllowStatUpdate<T>(Dictionary<PlayerState.GameMode, HashSet<T>> gameModeStats, T statId)
	{
		if (gameModeStats.TryGetValue(SRSingleton<SceneContext>.Instance.GameModel.currGameMode, out var value))
		{
			return value.Contains(statId);
		}
		return true;
	}

	public void AddToStat(IntStat stat, int amount)
	{
		if (AllowStatUpdate(GAME_MODE_INT_STATS, stat))
		{
			if (!profileAchievesModel.intStatDict.ContainsKey(stat))
			{
				profileAchievesModel.intStatDict[stat] = amount;
			}
			else
			{
				profileAchievesModel.intStatDict[stat] += amount;
			}
			CheckAchievements(stat);
		}
	}

	public void ResetStat(IntStat stat)
	{
		if (AllowStatUpdate(GAME_MODE_INT_STATS, stat))
		{
			profileAchievesModel.intStatDict[stat] = 0;
		}
	}

	public void MaybeUpdateMaxStat(IntStat stat, int val)
	{
		if (AllowStatUpdate(GAME_MODE_INT_STATS, stat))
		{
			if (!profileAchievesModel.intStatDict.ContainsKey(stat))
			{
				profileAchievesModel.intStatDict[stat] = val;
			}
			else if (val > profileAchievesModel.intStatDict[stat])
			{
				profileAchievesModel.intStatDict[stat] = val;
			}
			CheckAchievements(stat);
		}
	}

	public int? GetStat(IntStat stat)
	{
		if (profileAchievesModel.intStatDict.TryGetValue(stat, out var value))
		{
			return value;
		}
		return null;
	}

	public void AddToStat(EnumStat stat, Enum val)
	{
		if (AllowStatUpdate(GAME_MODE_ENUM_STATS, stat))
		{
			HashSet<Enum> hashSet;
			if (profileAchievesModel.enumStatDict.ContainsKey(stat))
			{
				hashSet = profileAchievesModel.enumStatDict[stat];
			}
			else
			{
				hashSet = new HashSet<Enum>();
				profileAchievesModel.enumStatDict[stat] = hashSet;
			}
			hashSet.Add(val);
			CheckAchievements(stat);
		}
	}

	public void SetStat(BoolStat stat)
	{
		if (AllowStatUpdate(GAME_MODE_BOOL_STATS, stat))
		{
			profileAchievesModel.boolStatDict[stat] = true;
			CheckAchievements(stat);
		}
	}

	public void SetStat(GameFloatStat stat, float val)
	{
		gameAchievesModel.gameFloatStatDict[stat] = val;
		CheckAchievements(stat);
	}

	public void SetStat(GameDoubleStat stat, double val)
	{
		gameAchievesModel.gameDoubleStatDict[stat] = val;
		CheckAchievements(stat);
	}

	public void AddToStat(GameIntStat stat, int amt)
	{
		int num = (gameAchievesModel.gameIntStatDict.ContainsKey(stat) ? gameAchievesModel.gameIntStatDict[stat] : 0);
		gameAchievesModel.gameIntStatDict[stat] = num + amt;
		SRSingleton<SceneContext>.Instance.PlayerState.OnGameIntStatChanged(stat, gameAchievesModel.gameIntStatDict[stat]);
		CheckAchievements(stat);
	}

	public int GetGameIntStat(GameIntStat stat)
	{
		return gameAchievesModel.gameIntStatDict.Get(stat);
	}

	public void AddToStat(GameIdDictStat stat, Identifiable.Id id, int amt)
	{
		Dictionary<Identifiable.Id, int> dictionary;
		if (gameAchievesModel.gameIdDictStatDict.ContainsKey(stat))
		{
			dictionary = gameAchievesModel.gameIdDictStatDict[stat];
		}
		else
		{
			dictionary = new Dictionary<Identifiable.Id, int>(Identifiable.idComparer);
			gameAchievesModel.gameIdDictStatDict[stat] = dictionary;
		}
		int num = (dictionary.ContainsKey(id) ? dictionary[id] : 0);
		dictionary[id] = num + amt;
		CheckAchievements(stat);
	}

	public Dictionary<Identifiable.Id, int> GetGameIdDictStat(GameIdDictStat stat)
	{
		Dictionary<Identifiable.Id, int> dictionary = gameAchievesModel.gameIdDictStatDict.Get(stat);
		if (dictionary == null)
		{
			return new Dictionary<Identifiable.Id, int>(Identifiable.idComparer);
		}
		return new Dictionary<Identifiable.Id, int>(dictionary, Identifiable.idComparer);
	}

	private void InitTrackers()
	{
		updatables.Clear();
		trackers[Achievement.SELL_PLORTS_A] = new CountTracker(this, Achievement.SELL_PLORTS_A, IntStat.PLORTS_SOLD, 100);
		trackers[Achievement.SELL_PLORTS_B] = new CountTracker(this, Achievement.SELL_PLORTS_B, IntStat.PLORTS_SOLD, 500);
		trackers[Achievement.SELL_PLORTS_C] = new CountTracker(this, Achievement.SELL_PLORTS_C, IntStat.PLORTS_SOLD, 1000);
		trackers[Achievement.SELL_PLORTS_D] = new CountTracker(this, Achievement.SELL_PLORTS_D, IntStat.PLORTS_SOLD, 2500);
		trackers[Achievement.SELL_PLORTS_E] = new CountTracker(this, Achievement.SELL_PLORTS_E, IntStat.PLORTS_SOLD, 5000);
		trackers[Achievement.DAY_CURRENCY] = new DailyCountTracker(this, Achievement.DAY_CURRENCY, IntStat.DAY_CURRENCY, 5000);
		trackers[Achievement.EARN_CURRENCY_A] = new CountTracker(this, Achievement.EARN_CURRENCY_A, IntStat.CURRENCY, 5000);
		trackers[Achievement.EARN_CURRENCY_B] = new CountTracker(this, Achievement.EARN_CURRENCY_B, IntStat.CURRENCY, 25000);
		trackers[Achievement.EARN_CURRENCY_C] = new CountTracker(this, Achievement.EARN_CURRENCY_C, IntStat.CURRENCY, 100000);
		trackers[Achievement.FEED_SLIMES_CHICKENS] = new CountTracker(this, Achievement.FEED_SLIMES_CHICKENS, IntStat.CHICKENS_FED_SLIMES, 100);
		trackers[Achievement.PINK_SLIMES_FOOD_TYPES] = new CountEnumsTracker(this, Achievement.PINK_SLIMES_FOOD_TYPES, EnumStat.PINK_SLIMES_FOOD_TYPES, 10);
		trackers[Achievement.FEED_AIRBORNE] = new CountTracker(this, Achievement.FEED_AIRBORNE, IntStat.FED_AIRBORNE, 1);
		trackers[Achievement.FEED_FAVORITES] = new CountTracker(this, Achievement.FEED_FAVORITES, IntStat.FED_FAVORITE, 50);
		trackers[Achievement.AWAY_FROM_RANCH] = new SimpleTracker(this, Achievement.AWAY_FROM_RANCH, () => gameAchievesModel.gameDoubleStatDict.ContainsKey(GameDoubleStat.LAST_LEFT_RANCH) && gameAchievesModel.gameDoubleStatDict.ContainsKey(GameDoubleStat.LAST_ENTERED_RANCH) && gameAchievesModel.gameDoubleStatDict[GameDoubleStat.LAST_ENTERED_RANCH] - gameAchievesModel.gameDoubleStatDict[GameDoubleStat.LAST_LEFT_RANCH] >= 86400.0, GameDoubleStat.LAST_LEFT_RANCH, GameDoubleStat.LAST_ENTERED_RANCH);
		trackers[Achievement.AWAKE_UNTIL_MORNING] = new UpdatableSimpleTracker(this, Achievement.AWAKE_UNTIL_MORNING, () => gameAchievesModel.gameDoubleStatDict.ContainsKey(GameDoubleStat.LAST_SLEPT) && gameAchievesModel.gameDoubleStatDict.ContainsKey(GameDoubleStat.LAST_AWOKE) && gameAchievesModel.gameDoubleStatDict[GameDoubleStat.LAST_AWOKE] > gameAchievesModel.gameDoubleStatDict[GameDoubleStat.LAST_SLEPT] && gameAchievesModel.gameDoubleStatDict[GameDoubleStat.LAST_AWOKE] < timeDir.GetHourAfter(-2, 6f) + 3600.0, delegate
		{
			if (timeDir.OnPassedHour(6f))
			{
				CheckAchievement(Achievement.AWAKE_UNTIL_MORNING);
			}
		}, GameDoubleStat.LAST_SLEPT, GameDoubleStat.LAST_AWOKE);
		trackers[Achievement.KNOCKOUT_MORNING] = new CountTracker(this, Achievement.KNOCKOUT_MORNING, IntStat.DEATH_BEFORE_10AM, 1);
		trackers[Achievement.FRUIT_TREE_TYPES] = new SimpleTracker(this, Achievement.FRUIT_TREE_TYPES, () => SRSingleton<SceneContext>.Instance.GameModel.GetRanchResourceTypes(Identifiable.FRUIT_CLASS).Count >= 3);
		trackers[Achievement.VEGGIE_PATCH_TYPES] = new SimpleTracker(this, Achievement.VEGGIE_PATCH_TYPES, () => SRSingleton<SceneContext>.Instance.GameModel.GetRanchResourceTypes(Identifiable.VEGGIE_CLASS).Count >= 3);
		trackers[Achievement.DISCOVERED_QUARRY] = new CountTracker(this, Achievement.DISCOVERED_QUARRY, IntStat.VISITED_QUARRY, 1);
		trackers[Achievement.DISCOVERED_MOSS] = new CountTracker(this, Achievement.DISCOVERED_MOSS, IntStat.VISITED_MOSS, 1);
		trackers[Achievement.DISCOVERED_DESERT] = new CountTracker(this, Achievement.DISCOVERED_DESERT, IntStat.VISITED_DESERT, 1);
		trackers[Achievement.DISCOVERED_RUINS] = new CountTracker(this, Achievement.DISCOVERED_RUINS, IntStat.VISITED_RUINS, 1);
		trackers[Achievement.BURST_GORDO] = new CountTracker(this, Achievement.BURST_GORDO, IntStat.BURST_GORDOS, 1);
		trackers[Achievement.OPEN_SLIME_GATE] = new CountTracker(this, Achievement.OPEN_SLIME_GATE, IntStat.OPENED_SLIME_GATES, 1);
		trackers[Achievement.INCINERATE_ELDER_CHICKEN] = new CountTracker(this, Achievement.INCINERATE_ELDER_CHICKEN, IntStat.INCINERATED_ELDER_CHICKENS, 1);
		trackers[Achievement.INCINERATE_CHICK] = new CountTracker(this, Achievement.INCINERATE_CHICK, IntStat.INCINERATED_CHICKS, 1);
		trackers[Achievement.FILLED_SILO] = new CountTracker(this, Achievement.FILLED_SILO, IntStat.FILLED_SILO, 1);
		trackers[Achievement.RANCH_UPGRADED_STORAGE] = new SimpleTracker(this, Achievement.RANCH_UPGRADED_STORAGE, () => gameModel.IncludesFullyUpgradedCorralCoopAndSilo());
		trackers[Achievement.FULFILL_EXCHANGE_EARLY] = new CountTracker(this, Achievement.FULFILL_EXCHANGE_EARLY, IntStat.FULFILL_EXCHANGE_EARLY, 1);
		trackers[Achievement.DAY_COLLECT_PLORTS] = new DailyCountTracker(this, Achievement.DAY_COLLECT_PLORTS, IntStat.DAY_COLLECT_PLORTS, 50);
		trackers[Achievement.GOLD_SLIME_TRIPLE_PLORT] = new CountTracker(this, Achievement.GOLD_SLIME_TRIPLE_PLORT, IntStat.GOLD_SLIME_TRIPLE_PLORT, 1);
		trackers[Achievement.EXTENDED_RAD_EXPOSURE] = new CountTracker(this, Achievement.EXTENDED_RAD_EXPOSURE, IntStat.EXTENDED_RAD_EXPOSURE, 15);
		trackers[Achievement.EXTENDED_TARR_HOLD] = new CountTracker(this, Achievement.EXTENDED_TARR_HOLD, IntStat.EXTENDED_TARR_HOLD, 15);
		trackers[Achievement.TABBY_HEADBUTT] = new CountTracker(this, Achievement.TABBY_HEADBUTT, IntStat.TABBY_HEADBUTT, 1);
		trackers[Achievement.LAUNCHED_BOOM_EXPLODE] = new CountTracker(this, Achievement.LAUNCHED_BOOM_EXPLODE, IntStat.LAUNCHED_BOOM_EXPLODE, 1);
		trackers[Achievement.MANY_SLIMES_IN_VAC] = new CountTracker(this, Achievement.MANY_SLIMES_IN_VAC, IntStat.SLIMES_IN_VAC, 15);
		trackers[Achievement.CORRAL_SLIME_TYPES] = new CountTracker(this, Achievement.CORRAL_SLIME_TYPES, IntStat.CORRAL_SLIME_TYPES, 6);
		trackers[Achievement.CORRAL_LARGO_TYPES] = new CountTracker(this, Achievement.CORRAL_LARGO_TYPES, IntStat.CORRAL_LARGO_TYPES, 3);
		trackers[Achievement.POND_SLIME_TYPES] = new CountTracker(this, Achievement.POND_SLIME_TYPES, IntStat.POND_SLIME_TYPES, 5);
		trackers[Achievement.RANCH_LARGO_TYPES] = new CountTracker(this, Achievement.RANCH_LARGO_TYPES, IntStat.RANCH_LARGO_TYPES, 10);
		trackers[Achievement.ENTERED_CORRAL_SLIMES] = new CountTracker(this, Achievement.ENTERED_CORRAL_SLIMES, IntStat.ENTERED_CORRAL_SLIMES, 40);
		trackers[Achievement.TIME_LIMIT_CURRENCY_A] = new CountTracker(this, Achievement.TIME_LIMIT_CURRENCY_A, IntStat.TIME_LIMIT_V2_CURRENCY, 10000);
		trackers[Achievement.TIME_LIMIT_CURRENCY_B] = new CountTracker(this, Achievement.TIME_LIMIT_CURRENCY_B, IntStat.TIME_LIMIT_V2_CURRENCY, 35000);
		trackers[Achievement.TIME_LIMIT_CURRENCY_C] = new CountTracker(this, Achievement.TIME_LIMIT_CURRENCY_C, IntStat.TIME_LIMIT_V2_CURRENCY, 75000);
		trackers[Achievement.FABRICATE_GADGETS_A] = new GameCountTracker(this, Achievement.FABRICATE_GADGETS_A, GameIntStat.FABRICATED_GADGETS, 1);
		trackers[Achievement.FABRICATE_GADGETS_B] = new GameCountTracker(this, Achievement.FABRICATE_GADGETS_B, GameIntStat.FABRICATED_GADGETS, 35);
		trackers[Achievement.FABRICATE_GADGETS_C] = new GameCountTracker(this, Achievement.FABRICATE_GADGETS_C, GameIntStat.FABRICATED_GADGETS, 100);
		trackers[Achievement.SLIMEBALL_SCORE] = new CountTracker(this, Achievement.SLIMEBALL_SCORE, IntStat.SLIMEBALL_SCORE, 50);
		trackers[Achievement.SLIME_STAGE_TARR] = new CountTracker(this, Achievement.SLIME_STAGE_TARR, IntStat.SLIME_STAGE_TARRS, 1);
		trackers[Achievement.JOIN_REWARDS_CLUB] = new CountTracker(this, Achievement.JOIN_REWARDS_CLUB, IntStat.REWARD_LEVELS, 1);
		trackers[Achievement.USE_CHROMAS] = new CountEnumsTracker(this, Achievement.USE_CHROMAS, EnumStat.USE_CHROMAS, 3);
		trackers[Achievement.COLLECT_SLIME_TOYS] = new CountEnumsTracker(this, Achievement.COLLECT_SLIME_TOYS, EnumStat.SLIME_TOYS_BOUGHT, 10);
		trackers[Achievement.SNARE_HUNTER_GORDO] = new CountTracker(this, Achievement.SNARE_HUNTER_GORDO, IntStat.SNARED_HUNTER_GORDOS, 1);
		trackers[Achievement.ACTIVATE_OASIS] = new CountTracker(this, Achievement.ACTIVATE_OASIS, IntStat.ACTIVATED_OASES, 1);
		trackers[Achievement.COMPLETE_SLIMEPEDIA] = new CountTracker(this, Achievement.COMPLETE_SLIMEPEDIA, IntStat.COMPLETED_SLIMEPEDIA, 1);
		trackers[Achievement.FIND_HOBSONS_END] = new CountTracker(this, Achievement.FIND_HOBSONS_END, IntStat.FIND_HOBSONS_END, 1);
		trackers[Achievement.FINISH_ADVENTURE] = new CountTracker(this, Achievement.FINISH_ADVENTURE, IntStat.FINISH_ADVENTURE, 1);
	}

	public void CheckAchievement(Achievement achievement)
	{
		CheckAchievements(Enumerable.Repeat(achievement, 1));
	}

	private void CheckAchievements(IEnumerable<Achievement> achievements)
	{
		postUpdateAchievementChecks.UnionWith(achievements);
	}

	private void CheckAchievements(BoolStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(IntStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(EnumStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(GameFloatStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(GameDoubleStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(GameIntStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private void CheckAchievements(GameIdDictStat stat)
	{
		CheckAchievements(from p in trackers
			where p.Value.IsTracking(stat)
			select p.Key);
	}

	private bool AwardAchievement(Achievement achievement)
	{
		if (GAME_MODE_ACHIEVEMENTS.TryGetValue(SRSingleton<SceneContext>.Instance.GameModel.currGameMode, out var value) && !value.Contains(achievement))
		{
			return false;
		}
		bool num = profileAchievesModel.earnedAchievements.Add(achievement);
		if (num)
		{
			MaybeShowPopup(achievement);
			AnalyticsUtil.CustomEvent("Achievement", new Dictionary<string, object> { 
			{
				"id",
				achievement.ToString()
			} });
		}
		return num;
	}

	private void MaybeShowPopup(Achievement achievement)
	{
		popupQueue.Enqueue(achievement);
		MaybePopupNext();
	}

	public void RegisterSuppressor()
	{
		suppressors++;
	}

	public void UnregisterSuppressor()
	{
		suppressors--;
		if (suppressors <= 0)
		{
			MaybePopupNext();
		}
	}

	private void MaybePopupNext()
	{
		if (popupQueue.Count > 0 && currPopup == null && suppressors <= 0)
		{
			Achievement idEntry = popupQueue.Dequeue();
			UnityEngine.Object.Instantiate(achievementAwardUIPrefab).GetComponent<AchievementAwardUI>().Init(idEntry);
		}
	}

	public void OnApplicationQuit()
	{
		quitting = true;
	}

	public void PopupActivated(AchievementAwardUI popup)
	{
		if (currPopup != null)
		{
			Log.Warning("Popup arrived with already-active popup.");
		}
		currPopup = popup;
	}

	public void PopupDeactivated(AchievementAwardUI popup)
	{
		if (currPopup == popup && !quitting)
		{
			currPopup = null;
			timeDir.OnUnpause(OnUnpause);
		}
		else
		{
			Log.Warning("Popup deactivated, but wasn't current popup.");
		}
	}

	public void OnDestroy()
	{
		timeDir.ClearOnUnpause(OnUnpause);
	}

	public void OnUnpause()
	{
		MaybePopupNext();
	}

	public bool HasAchievement(Achievement achievement)
	{
		return profileAchievesModel.earnedAchievements.Contains(achievement);
	}

	public void GetProgress(Achievement achievement, out int progress, out int outOf)
	{
		if (!trackers.ContainsKey(achievement))
		{
			progress = 0;
			outOf = 1;
		}
		else
		{
			trackers[achievement].GetProgress(out progress, out outOf);
		}
	}

	public void GetOverallProgress(out int progress, out int outOf)
	{
		progress = profileAchievesModel.earnedAchievements.Count;
		outOf = Enum.GetValues(typeof(Achievement)).Length;
	}

	public Sprite GetAchievementImage(string achievementKey, Achievement achieve)
	{
		Sprite sprite = Resources.Load("Achievements/" + achievementKey, typeof(Sprite)) as Sprite;
		if (sprite == null)
		{
			if (TIER_1.Contains(achieve))
			{
				return tier1DefaultIcon;
			}
			if (TIER_2.Contains(achieve))
			{
				return tier2DefaultIcon;
			}
			if (TIER_3.Contains(achieve))
			{
				return tier3DefaultIcon;
			}
			return tier1DefaultIcon;
		}
		return sprite;
	}

	protected bool HasMissingTieredAchieves()
	{
		foreach (Achievement value in Enum.GetValues(typeof(Achievement)))
		{
			if (!TIER_1.Contains(value) && !TIER_2.Contains(value) && !TIER_3.Contains(value))
			{
				Log.Error("Missing achieve tier: " + value);
				return true;
			}
		}
		return false;
	}
}
