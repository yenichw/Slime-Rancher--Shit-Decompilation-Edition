using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class ExchangeDirector : SRBehaviour, WorldModel.Participant
{
	public delegate void OnAwakeDelegate(ExchangeDirector exchangeDir);

	public delegate void OnOfferChanged();

	public interface Awarder
	{
		void AwardIfType(OfferType offerType);
	}

	public enum OfferType
	{
		GENERAL = 0,
		OGDEN = 1,
		OGDEN_RECUR = 2,
		MOCHI = 3,
		MOCHI_RECUR = 4,
		VIKTOR = 5,
		VIKTOR_RECUR = 6
	}

	[Serializable]
	public class ValueEntry
	{
		public Identifiable.Id id;

		public float value;
	}

	[Serializable]
	public class Rancher
	{
		public string name;

		public Sprite defaultImg;

		public Sprite icon;

		public Material chatBackground;

		public int numBlurbs;

		public Category[] requestCategories;

		public Identifiable.Id[] indivRequests;

		public Category[] rewardCategories;

		public Identifiable.Id[] indivRewards;

		public Category[] rareRewardCategories;

		public Identifiable.Id[] indivRareRewards;
	}

	[Serializable]
	public class RewardLevel
	{
		public NonIdentReward reward;

		public Identifiable.Id requestedItem;

		public int count;

		public RancherChatMetadata rancherChatIntro;

		public RancherChatMetadata rancherChatRepeat;
	}

	[Serializable]
	public class ProgressOfferEntry
	{
		public OfferType specialOfferType;

		public ProgressDirector.ProgressType progressType;

		public RewardLevel[] rewardLevels;

		public RancherChatMetadata rancherChatEndIntro;

		public RancherChatMetadata rancherChatEndRepeat;
	}

	public enum Category
	{
		FRUIT = 0,
		VEGGIES = 1,
		MEAT = 2,
		PLORTS = 3,
		SLIMES = 4,
		CRAFT_MATS = 5
	}

	[Serializable]
	public class UnlockList
	{
		public ProgressDirector.ProgressType unlock;

		public Identifiable.Id[] ids;
	}

	[Serializable]
	public class RequestedItemEntry : ItemEntry
	{
		public int progress;

		public RequestedItemEntry(Identifiable.Id id, int count, int progress, NonIdentReward specReward)
			: base(id, count, specReward)
		{
			this.progress = progress;
		}

		public RequestedItemEntry(Identifiable.Id id, int count, int progress)
			: base(id, count)
		{
			this.progress = progress;
		}

		public bool IsComplete()
		{
			return progress >= count;
		}
	}

	public enum NonIdentReward
	{
		NONE = 0,
		OGDEN_MIX = 100,
		OGDEN_GARDEN = 101,
		OGDEN_RANCH = 102,
		MOCHI_EXTRA_MILE = 200,
		MOCHI_COOP = 201,
		MOCHI_RANCH = 202,
		VIKTOR_CHICKEN_CLONER = 300,
		VIKTOR_DELUXE_DRONES = 301,
		VIKTOR_RANCH = 302,
		NEWBUCKS_SMALL = 10000,
		NEWBUCKS_MEDIUM = 10001,
		NEWBUCKS_LARGE = 10002,
		NEWBUCKS_HUGE = 10003,
		NEWBUCKS_MOCHI = 10004,
		TIME_EXTENSION_12H = 20000
	}

	[Serializable]
	public class NonIdentEntry
	{
		public NonIdentReward reward;

		public Sprite icon;
	}

	[Serializable]
	public class ItemEntry
	{
		public Identifiable.Id id;

		public NonIdentReward specReward;

		public int count;

		public ItemEntry(Identifiable.Id id, int count, NonIdentReward specReward)
		{
			this.id = id;
			this.specReward = specReward;
			this.count = count;
		}

		public ItemEntry(Identifiable.Id id, int count)
		{
			this.id = id;
			specReward = NonIdentReward.NONE;
			this.count = count;
		}

		public ItemEntry(NonIdentReward specReward)
		{
			id = Identifiable.Id.NONE;
			this.specReward = specReward;
			count = 1;
		}
	}

	[Serializable]
	public class Offer
	{
		public List<RequestedItemEntry> requests;

		public List<ItemEntry> rewards;

		public double expireTime;

		public double earlyExchangeTime;

		public string rancherId;

		public string offerId;

		public Offer(string offerId, string rancherId, double expireTime, double earlyExchangeTime, List<RequestedItemEntry> requests, List<ItemEntry> rewards)
		{
			this.offerId = offerId;
			this.rancherId = rancherId;
			this.expireTime = expireTime;
			this.earlyExchangeTime = earlyExchangeTime;
			this.requests = requests;
			this.rewards = rewards;
		}

		public bool TryAccept(Identifiable.Id id, Awarder[] awarders, OfferType offerType)
		{
			foreach (RequestedItemEntry request in requests)
			{
				if (request.id != id || request.IsComplete())
				{
					continue;
				}
				request.progress++;
				if (IsComplete())
				{
					for (int i = 0; i < awarders.Length; i++)
					{
						awarders[i].AwardIfType(offerType);
					}
					if (offerType == OfferType.GENERAL && !SRSingleton<SceneContext>.Instance.TimeDirector.HasReached(earlyExchangeTime))
					{
						SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.FULFILL_EXCHANGE_EARLY, 1);
					}
					if (offerType == OfferType.GENERAL)
					{
						if (rancherId == "ogden")
						{
							SRSingleton<SceneContext>.Instance.ProgressDirector.MaybeUnlockOgdenMissions();
						}
						else if (rancherId == "mochi")
						{
							SRSingleton<SceneContext>.Instance.ProgressDirector.MaybeUnlockMochiMissions();
						}
						else if (rancherId == "viktor")
						{
							SRSingleton<SceneContext>.Instance.ProgressDirector.MaybeUnlockViktorMissions();
						}
					}
					AnalyticsUtil.CustomEvent("ExchangeOfferComplete", new Dictionary<string, object>
					{
						{ "RancherId", rancherId },
						{ "ExchangeId", offerId }
					});
				}
				return true;
			}
			return false;
		}

		public bool IsComplete()
		{
			foreach (RequestedItemEntry request in requests)
			{
				if (!request.IsComplete())
				{
					return false;
				}
			}
			return true;
		}
	}

	public OnOfferChanged onOfferChanged;

	[Tooltip("Values to be used in generating offers.")]
	public ValueEntry[] values;

	public ProgressOfferEntry[] progressOffers;

	private Dictionary<Category, Identifiable.Id[]> catDict = new Dictionary<Category, Identifiable.Id[]>
	{
		{
			Category.FRUIT,
			new List<Identifiable.Id>(Identifiable.FRUIT_CLASS).ToArray()
		},
		{
			Category.VEGGIES,
			new List<Identifiable.Id>(Identifiable.VEGGIE_CLASS).ToArray()
		},
		{
			Category.MEAT,
			new List<Identifiable.Id>(Identifiable.MEAT_CLASS).ToArray()
		},
		{
			Category.PLORTS,
			new List<Identifiable.Id>(Identifiable.PLORT_CLASS).ToArray()
		},
		{
			Category.SLIMES,
			new List<Identifiable.Id>(Identifiable.SLIME_CLASS).ToArray()
		},
		{
			Category.CRAFT_MATS,
			new List<Identifiable.Id>(Identifiable.CRAFT_CLASS).ToArray()
		}
	};

	[Tooltip("The ranchers and what they request/reward the player with.")]
	public Rancher[] ranchers;

	public Identifiable.Id[] initUnlocked;

	public UnlockList[] unlockLists;

	public NonIdentEntry[] nonIdentRewards;

	public int ogdenRecurAmount = 3;

	public int mochiRecurAmount = 5;

	public int viktorRecurAmount = 5;

	private Dictionary<NonIdentReward, Sprite> nonIdentRewardDict = new Dictionary<NonIdentReward, Sprite>();

	private Dictionary<Identifiable.Id, float> valueDict = new Dictionary<Identifiable.Id, float>(Identifiable.idComparer);

	private TimeDirector timeDir;

	private ProgressDirector progressDir;

	private MailDirector mailDir;

	private PediaDirector pediaDir;

	private TutorialDirector tutorialDir;

	private Dictionary<string, OfferGenerator> offerGenerators = new Dictionary<string, OfferGenerator>();

	private WorldModel worldModel;

	private const float HOURS_BETWEEN_OFFERS = 1f / 12f;

	private const float OFFER_END_HOUR = 12f;

	private const float OFFER_HOUR = 12.083333f;

	private const float DAYS_PER_DAILY_LEVEL = 3f;

	private const float OGDEN_LEVELS = 3f;

	private const float MOCHI_LEVELS = 3f;

	private const float VIKTOR_LEVELS = 3f;

	private const float EARLY_EXCHANGE_HOURS = 2f;

	public bool HasPendingOffers(OfferType offerType)
	{
		if (worldModel == null)
		{
			return false;
		}
		if (offerType == OfferType.GENERAL)
		{
			return worldModel.pendingOfferRancherIds.Count > 0;
		}
		return false;
	}

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		tutorialDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
		ValueEntry[] array = values;
		foreach (ValueEntry valueEntry in array)
		{
			valueDict[valueEntry.id] = valueEntry.value;
		}
		NonIdentEntry[] array2 = nonIdentRewards;
		foreach (NonIdentEntry nonIdentEntry in array2)
		{
			nonIdentRewardDict[nonIdentEntry.reward] = nonIdentEntry.icon;
		}
		ConfigureOfferGenerators();
	}

	public void Start()
	{
		if (progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_WILDS))
		{
			pediaDir.Unlock(PediaDirector.Id.WILDS_TUTORIAL);
		}
		if (progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_VALLEY))
		{
			pediaDir.Unlock(PediaDirector.Id.VALLEY_TUTORIAL);
		}
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void InitModel(WorldModel worldModel)
	{
		worldModel.currOffers.Clear();
		worldModel.lastOfferRancherIds.Clear();
		worldModel.pendingOfferRancherIds.Clear();
		int fullDays = SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().exchangeStartDay - 1;
		worldModel.nextDailyOfferCreateTime = TimeDirector.GetHourAfter(0.0, fullDays, 12.083333f);
	}

	public void SetModel(WorldModel worldModel)
	{
		this.worldModel = worldModel;
	}

	private void PrepareNextDailyOffer()
	{
		SetupPendingOfferRanchers();
		worldModel.lastOfferRancherIds.Clear();
		worldModel.lastOfferRancherIds.AddRange(worldModel.pendingOfferRancherIds);
	}

	private double GetNextDailyOfferCreateTime()
	{
		if (SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2 && timeDir.CurrDay() == 1)
		{
			return timeDir.GetHourAfter(1, 12.083333f);
		}
		return timeDir.GetNextHour(12.083333f);
	}

	public void Update()
	{
		Offer offer = (worldModel.currOffers.ContainsKey(OfferType.GENERAL) ? worldModel.currOffers[OfferType.GENERAL] : null);
		if (offer != null && timeDir.HasReached(offer.expireTime))
		{
			ClearOffer(OfferType.GENERAL);
		}
		if (offer == null && timeDir.HasReached(worldModel.nextDailyOfferCreateTime))
		{
			worldModel.nextDailyOfferCreateTime = GetNextDailyOfferCreateTime();
			PrepareNextDailyOffer();
			OfferDidChange();
		}
		if (!worldModel.currOffers.ContainsKey(OfferType.OGDEN_RECUR) && (worldModel.currOffers.ContainsKey(OfferType.OGDEN) || (float)progressDir.GetProgress(ProgressDirector.ProgressType.OGDEN_REWARDS) >= 3f))
		{
			worldModel.currOffers[OfferType.OGDEN_RECUR] = CreateOgdenRecurOffer();
			OfferDidChange();
		}
		if (!worldModel.currOffers.ContainsKey(OfferType.MOCHI_RECUR) && (worldModel.currOffers.ContainsKey(OfferType.MOCHI) || (float)progressDir.GetProgress(ProgressDirector.ProgressType.MOCHI_REWARDS) >= 3f))
		{
			worldModel.currOffers[OfferType.MOCHI_RECUR] = CreateMochiRecurOffer();
			OfferDidChange();
		}
		if (!worldModel.currOffers.ContainsKey(OfferType.VIKTOR_RECUR) && (worldModel.currOffers.ContainsKey(OfferType.VIKTOR) || (float)progressDir.GetProgress(ProgressDirector.ProgressType.VIKTOR_REWARDS) >= 3f))
		{
			worldModel.currOffers[OfferType.VIKTOR_RECUR] = CreateViktorRecurOffer();
			OfferDidChange();
		}
	}

	public bool MaybeStartNext(OfferType offerType)
	{
		if (worldModel.currOffers.ContainsKey(offerType))
		{
			return false;
		}
		ProgressOfferEntry progressEntry = GetProgressEntry(offerType);
		if (progressEntry != null && !worldModel.currOffers.ContainsKey(progressEntry.specialOfferType) && progressDir.GetProgress(progressEntry.progressType) < progressEntry.rewardLevels.Length)
		{
			worldModel.currOffers[progressEntry.specialOfferType] = CreateProgressOffer(progressEntry.specialOfferType, progressEntry.progressType, progressEntry.rewardLevels);
			OfferDidChange();
			return CreateRancherChatUI(offerType, intro: true);
		}
		return false;
	}

	private void SetupPendingOfferRanchers()
	{
		worldModel.pendingOfferRancherIds.Clear();
		List<string> list = new List<string>();
		Rancher[] array = ranchers;
		foreach (Rancher rancher in array)
		{
			ProgressDirector.ProgressType rancherProgressType = ProgressDirector.GetRancherProgressType(rancher.name);
			if (!progressDir.HasProgress(rancherProgressType))
			{
				worldModel.pendingOfferRancherIds.Add(rancher.name);
				mailDir.SendMail(MailDirector.Type.EXCHANGE, "exchangeintro_" + rancher.name);
				return;
			}
			if (!worldModel.lastOfferRancherIds.Contains(rancher.name))
			{
				list.Add(rancher.name);
			}
		}
		if (list.Count < 2)
		{
			Log.Error("Somehow do not have enough available ranchers to choose from for exchange offers.");
			return;
		}
		worldModel.pendingOfferRancherIds.Add(Randoms.SHARED.Pluck(list, null));
		worldModel.pendingOfferRancherIds.Add(Randoms.SHARED.Pluck(list, null));
	}

	public double? GetOfferExpirationTime(OfferType type)
	{
		if (worldModel.currOffers.ContainsKey(type))
		{
			return worldModel.currOffers[type].expireTime - timeDir.WorldTime();
		}
		return null;
	}

	public List<RequestedItemEntry> GetOfferRequests(OfferType type)
	{
		if (worldModel != null)
		{
			if (worldModel.currOffers.ContainsKey(type))
			{
				return worldModel.currOffers[type].requests;
			}
			return null;
		}
		return null;
	}

	public List<ItemEntry> GetOfferRewards(OfferType type)
	{
		if (worldModel != null)
		{
			if (worldModel.currOffers.ContainsKey(type))
			{
				return worldModel.currOffers[type].rewards;
			}
			return null;
		}
		return null;
	}

	public string GetOfferId(OfferType type)
	{
		if (worldModel != null)
		{
			if (worldModel.currOffers.ContainsKey(type))
			{
				return worldModel.currOffers[type].offerId;
			}
			return null;
		}
		return null;
	}

	private ProgressOfferEntry GetProgressEntry(OfferType type)
	{
		ProgressOfferEntry[] array = progressOffers;
		foreach (ProgressOfferEntry progressOfferEntry in array)
		{
			if (progressOfferEntry.specialOfferType == type)
			{
				return progressOfferEntry;
			}
		}
		return null;
	}

	public bool TryToAcceptNewOffer()
	{
		if (worldModel.pendingOfferRancherIds.Count == 0)
		{
			return false;
		}
		if (worldModel.pendingOfferRancherIds.Count == 1)
		{
			SelectDailyOffer(worldModel.pendingOfferRancherIds[0], isFirstOffer: true);
			return false;
		}
		SRSingleton<GameContext>.Instance.UITemplates.CreateRancherChoiceUI(worldModel.pendingOfferRancherIds);
		return true;
	}

	public bool SelectDailyOffer(string rancherId, bool isFirstOffer)
	{
		if (worldModel.currOffers.ContainsKey(OfferType.GENERAL))
		{
			return false;
		}
		Offer offer = CreateDailyOffer(rancherId, isFirstOffer);
		if (offer == null)
		{
			return false;
		}
		worldModel.currOffers[OfferType.GENERAL] = offer;
		ProgressDirector.ProgressType rancherProgressType = ProgressDirector.GetRancherProgressType(offer.rancherId);
		progressDir.AddProgress(rancherProgressType);
		worldModel.pendingOfferRancherIds.Clear();
		OfferDidChange();
		return true;
	}

	public Sprite GetRancherImage(string rancherId)
	{
		return GetRancher(rancherId).defaultImg;
	}

	public Sprite GetRancherIcon(string rancherId)
	{
		return GetRancher(rancherId).icon;
	}

	private Rancher GetRancher(string rancherId)
	{
		Rancher[] array = ranchers;
		foreach (Rancher rancher in array)
		{
			if (rancher.name == rancherId)
			{
				return rancher;
			}
		}
		return null;
	}

	public string GetOfferRancherId(OfferType type)
	{
		if (worldModel.currOffers.ContainsKey(type))
		{
			return worldModel.currOffers[type].rancherId;
		}
		return null;
	}

	public void RewardsDidSpawn(OfferType type)
	{
		ClearOffer(type);
	}

	public bool TryAccept(OfferType type, Identifiable.Id id, Awarder[] awarders)
	{
		if (worldModel.currOffers.ContainsKey(type) && worldModel.currOffers[type].TryAccept(id, awarders, type))
		{
			OfferDidChange();
			return true;
		}
		return false;
	}

	public int GetCountForValue(Identifiable.Id id, int value)
	{
		if (valueDict.ContainsKey(id))
		{
			return Mathf.RoundToInt((float)value / valueDict[id]);
		}
		return 0;
	}

	public Sprite GetSpecRewardIcon(NonIdentReward specReward)
	{
		return nonIdentRewardDict[specReward];
	}

	private void ClearOffer(OfferType type)
	{
		worldModel.currOffers.Remove(type);
		OfferDidChange();
	}

	private Offer CreateOgdenRecurOffer()
	{
		return new Offer("m.offer.ogden_recur", "ogden", double.PositiveInfinity, double.NegativeInfinity, new List<RequestedItemEntry>
		{
			new RequestedItemEntry(Identifiable.Id.KOOKADOBA_FRUIT, ogdenRecurAmount, 0)
		}, new List<ItemEntry>
		{
			new ItemEntry(Identifiable.Id.SPICY_TOFU, 1)
		});
	}

	private Offer CreateMochiRecurOffer()
	{
		return new Offer("m.offer.mochi_recur", "mochi", double.PositiveInfinity, double.NegativeInfinity, new List<RequestedItemEntry>
		{
			new RequestedItemEntry(Identifiable.Id.QUICKSILVER_PLORT, mochiRecurAmount, 0)
		}, new List<ItemEntry>
		{
			new ItemEntry(NonIdentReward.NEWBUCKS_MOCHI)
		});
	}

	private Offer CreateViktorRecurOffer()
	{
		return new Offer("m.offer.viktor_recur", "viktor", double.PositiveInfinity, double.NegativeInfinity, new List<RequestedItemEntry>
		{
			new RequestedItemEntry(Identifiable.Id.GLITCH_BUG_REPORT, viktorRecurAmount, 0)
		}, new List<ItemEntry>
		{
			new ItemEntry(Identifiable.Id.MANIFOLD_CUBE_CRAFT, 1)
		});
	}

	private Offer CreateProgressOffer(OfferType offerType, ProgressDirector.ProgressType progressType, RewardLevel[] rewardLevels)
	{
		int num = progressDir.GetProgress(progressType) + 1;
		RewardLevel rewardLevel = rewardLevels[num - 1];
		return new Offer("m.offer." + progressType.ToString().ToLowerInvariant() + "_level" + num, requests: new List<RequestedItemEntry>
		{
			new RequestedItemEntry(rewardLevel.requestedItem, rewardLevel.count, 0)
		}, rewards: new List<ItemEntry>
		{
			new ItemEntry(rewardLevel.reward)
		}, rancherId: offerType.ToString().ToLowerInvariant(), expireTime: double.PositiveInfinity, earlyExchangeTime: double.NegativeInfinity);
	}

	private Offer CreateDailyOffer(string rancherId, bool isFirstOffer)
	{
		int num = 10;
		if (SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2)
		{
			List<Identifiable.Id> whitelist = CreateRushModeWhiteList(timeDir.CurrDay());
			List<RequestedItemEntry> list = null;
			while (list == null && num > 0)
			{
				list = offerGenerators[rancherId].GenerateRequestList(this, whitelist);
				num--;
			}
			if (list == null)
			{
				return null;
			}
			return new Offer($"m.offer_{offerGenerators[rancherId].GetRandomBlurb()}.{rancherId}", rewards: new List<ItemEntry>
			{
				new ItemEntry(NonIdentReward.TIME_EXTENSION_12H),
				new ItemEntry(Identifiable.Id.GINGER_VEGGIE, 6)
			}, rancherId: rancherId, expireTime: timeDir.GetNextHourAtLeastHalfDay(12f), earlyExchangeTime: timeDir.HoursFromNow(2f), requests: list);
		}
		return offerGenerators[rancherId].Generate(this, CreateWhiteList(), timeDir.GetNextHourAtLeastHalfDay(12f), timeDir.HoursFromNow(2f), num, isFirstOffer, SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().exchangeRewardsGoldPlorts);
	}

	private List<Identifiable.Id> CreateRushModeWhiteList(int day)
	{
		HashSet<ProgressDirector.ProgressType> hashSet = new HashSet<ProgressDirector.ProgressType>(ProgressDirector.progressTypeComparer);
		if (day >= 2)
		{
			hashSet.Add(ProgressDirector.ProgressType.UNLOCK_QUARRY);
			hashSet.Add(ProgressDirector.ProgressType.UNLOCK_MOSS);
		}
		if (day >= 3)
		{
			hashSet.Add(ProgressDirector.ProgressType.UNLOCK_DESERT);
			hashSet.Add(ProgressDirector.ProgressType.UNLOCK_RUINS);
		}
		List<Identifiable.Id> list = new List<Identifiable.Id>(initUnlocked);
		UnlockList[] array = unlockLists;
		foreach (UnlockList unlockList in array)
		{
			if (hashSet.Contains(unlockList.unlock))
			{
				list.AddRange(unlockList.ids);
			}
		}
		return list;
	}

	private List<Identifiable.Id> CreateWhiteList()
	{
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		list.AddRange(initUnlocked);
		UnlockList[] array = unlockLists;
		foreach (UnlockList unlockList in array)
		{
			if (progressDir.HasProgress(unlockList.unlock))
			{
				list.AddRange(unlockList.ids);
			}
		}
		return list;
	}

	private void OfferDidChange()
	{
		if (onOfferChanged != null)
		{
			onOfferChanged();
		}
	}

	private void ConfigureOfferGenerators()
	{
		offerGenerators.Clear();
		Rancher[] array = ranchers;
		foreach (Rancher rancher in array)
		{
			List<Identifiable.Id> list = new List<Identifiable.Id>();
			Category[] requestCategories = rancher.requestCategories;
			foreach (Category key in requestCategories)
			{
				list.AddRange(catDict[key]);
			}
			list.AddRange(rancher.indivRequests);
			List<Identifiable.Id> list2 = new List<Identifiable.Id>();
			requestCategories = rancher.rewardCategories;
			foreach (Category key2 in requestCategories)
			{
				list2.AddRange(catDict[key2]);
			}
			list2.AddRange(rancher.indivRewards);
			List<Identifiable.Id> list3 = new List<Identifiable.Id>();
			requestCategories = rancher.rareRewardCategories;
			foreach (Category key3 in requestCategories)
			{
				list3.AddRange(catDict[key3]);
			}
			list3.AddRange(rancher.indivRareRewards);
			offerGenerators[rancher.name] = new OfferGenerator(rancher.name, rancher.numBlurbs, list.ToArray(), list2.ToArray(), list3.ToArray());
		}
	}

	public bool IsOffline(OfferType offerType)
	{
		if (GetOfferRequests(offerType) == null)
		{
			return !HasPendingOffers(offerType);
		}
		return false;
	}

	public bool CreateRancherChatUI(OfferType offerType, bool intro)
	{
		RancherChatMetadata rancherChatMetadata = GetRancherChatMetadata(offerType, intro);
		if (rancherChatMetadata == null)
		{
			return false;
		}
		RancherChatUI.Instantiate(rancherChatMetadata).onDestroy = delegate
		{
			if (SRSingleton<SceneContext>.Instance != null)
			{
				switch (offerType)
				{
				case OfferType.OGDEN:
					if (progressDir.SetUniqueProgress(ProgressDirector.ProgressType.UNLOCK_WILDS))
					{
						tutorialDir.MaybeShowPopup(TutorialDirector.Id.WILDS_SLIMEPEDIA);
						pediaDir.Unlock(PediaDirector.Id.WILDS_TUTORIAL);
					}
					break;
				case OfferType.MOCHI:
					if (progressDir.SetUniqueProgress(ProgressDirector.ProgressType.UNLOCK_VALLEY))
					{
						tutorialDir.MaybeShowPopup(TutorialDirector.Id.VALLEY_SLIMEPEDIA);
						pediaDir.Unlock(PediaDirector.Id.VALLEY_TUTORIAL);
					}
					break;
				case OfferType.VIKTOR:
					if (progressDir.SetUniqueProgress(ProgressDirector.ProgressType.UNLOCK_SLIMULATIONS))
					{
						tutorialDir.MaybeShowPopup(TutorialDirector.Id.SLIMULATIONS_SLIMEPEDIA);
						pediaDir.Unlock(PediaDirector.Id.SLIMULATIONS_TUTORIAL);
					}
					break;
				case OfferType.OGDEN_RECUR:
				case OfferType.MOCHI_RECUR:
					break;
				}
			}
		};
		return true;
	}

	private RancherChatMetadata GetRancherChatMetadata(OfferType offerType, bool intro)
	{
		switch (offerType)
		{
		case OfferType.OGDEN_RECUR:
		{
			ProgressOfferEntry progressEntry4 = GetProgressEntry(OfferType.OGDEN);
			intro = progressDir.SetUniqueProgress(ProgressDirector.ProgressType.OGDEN_SEEN_FINAL_CHAT);
			if (!intro)
			{
				return progressEntry4.rancherChatEndRepeat;
			}
			return progressEntry4.rancherChatEndIntro;
		}
		case OfferType.MOCHI_RECUR:
		{
			ProgressOfferEntry progressEntry3 = GetProgressEntry(OfferType.MOCHI);
			intro = progressDir.SetUniqueProgress(ProgressDirector.ProgressType.MOCHI_SEEN_FINAL_CHAT);
			if (!intro)
			{
				return progressEntry3.rancherChatEndRepeat;
			}
			return progressEntry3.rancherChatEndIntro;
		}
		case OfferType.VIKTOR_RECUR:
		{
			ProgressOfferEntry progressEntry2 = GetProgressEntry(OfferType.VIKTOR);
			intro = progressDir.SetUniqueProgress(ProgressDirector.ProgressType.VIKTOR_SEEN_FINAL_CHAT);
			if (!intro)
			{
				return progressEntry2.rancherChatEndRepeat;
			}
			return progressEntry2.rancherChatEndIntro;
		}
		default:
		{
			ProgressOfferEntry progressEntry = GetProgressEntry(offerType);
			if (progressEntry != null)
			{
				int progress = progressDir.GetProgress(progressEntry.progressType);
				if (progress < progressEntry.rewardLevels.Length)
				{
					RewardLevel rewardLevel = progressEntry.rewardLevels[progress];
					if (!intro)
					{
						return rewardLevel.rancherChatRepeat;
					}
					return rewardLevel.rancherChatIntro;
				}
			}
			if (worldModel.currOffers.ContainsKey(offerType))
			{
				Offer offer = worldModel.currOffers[offerType];
				return CreateRancherChatMetadata(offer.rancherId, offer.offerId);
			}
			return null;
		}
		}
	}

	public RancherChatMetadata CreateRancherChatMetadata(string rancherId, string message)
	{
		RancherChatMetadata rancherChatMetadata = ScriptableObject.CreateInstance<RancherChatMetadata>();
		rancherChatMetadata.entries = new RancherChatMetadata.Entry[1]
		{
			new RancherChatMetadata.Entry
			{
				rancherName = (RancherChatMetadata.Entry.RancherName)Enum.Parse(typeof(RancherChatMetadata.Entry.RancherName), rancherId.ToUpperInvariant()),
				rancherImage = GetRancherImage(rancherId),
				messageBackground = GetRancher(rancherId).chatBackground,
				messageText = message
			}
		};
		return rancherChatMetadata;
	}
}
