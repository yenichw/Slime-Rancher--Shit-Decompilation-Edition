using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV04 : VersionedPersistedDataSet<GameData>
	{
		public string gameName;

		public WorldV13 world = new WorldV13();

		public PlayerV07 player = new PlayerV07();

		public RanchV05 ranch = new RanchV05();

		public List<ActorDataV05> actors = new List<ActorDataV05>();

		public PediaV02 pedia = new PediaV02();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public override string Identifier => "SRGAME";

		public override uint Version => 4u;

		protected override void LoadData(BinaryReader reader)
		{
			gameName = reader.ReadString();
			world = WorldV13.Load(reader);
			player = PlayerV07.Load(reader);
			ranch = RanchV05.Load(reader);
			ReadSectionSeparator(reader);
			actors = PersistedDataSet.LoadList<ActorDataV05>(reader);
			ReadSectionSeparator(reader);
			pedia = PediaV02.Load(reader);
			achieve = GameAchieveV03.Load(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(gameName);
			world.Write(writer.BaseStream);
			player.Write(writer.BaseStream);
			ranch.Write(writer.BaseStream);
			WriteSectionSeparator(writer);
			PersistedDataSet.WriteList(writer, actors);
			WriteSectionSeparator(writer);
			pedia.Write(writer.BaseStream);
			achieve.Write(writer.BaseStream);
		}

		protected override void UpgradeFrom(GameData legacyData)
		{
			gameName = legacyData.gameName;
			world = UpgradeFrom(legacyData.world);
			achieve = UpgradeFrom(legacyData.achieve);
			player = UpgradeFrom(legacyData.player);
			ranch = UpgradeFrom(legacyData.ranch);
			actors = UpgradeFrom(legacyData.actors);
			pedia = UpgradeFrom(legacyData.pedia);
		}

		private WorldV13 UpgradeFrom(WorldData legacyData)
		{
			WorldV13 worldV = new WorldV13();
			worldV.worldTime = legacyData.worldTime;
			worldV.econSeed = legacyData.econSeed;
			if (legacyData.econSaturations != null)
			{
				worldV.econSaturations = new Dictionary<Identifiable.Id, float>(legacyData.econSaturations);
			}
			else
			{
				worldV.econSaturations = new Dictionary<Identifiable.Id, float>();
			}
			worldV.resourceSpawnerWater = new Dictionary<Vector3V02, ResourceWaterV03>();
			if (legacyData.resourceSpawnerWater != null)
			{
				foreach (KeyValuePair<Vector3, WorldData.ResourceWater> item in legacyData.resourceSpawnerWater)
				{
					worldV.resourceSpawnerWater.Add(UpgradeFrom(item.Key), UpgradeFrom(item.Value));
				}
			}
			worldV.spawnerTriggerTimes = new Dictionary<Vector3V02, double>();
			if (legacyData.spawnerTriggerTimes != null)
			{
				foreach (KeyValuePair<Vector3, float> spawnerTriggerTime in legacyData.spawnerTriggerTimes)
				{
					worldV.spawnerTriggerTimes.Add(UpgradeFrom(spawnerTriggerTime.Key), spawnerTriggerTime.Value);
				}
			}
			if (legacyData.teleportNodeActivations != null)
			{
				worldV.teleportNodeActivations = new Dictionary<string, bool>(legacyData.teleportNodeActivations);
			}
			else
			{
				worldV.teleportNodeActivations = new Dictionary<string, bool>();
			}
			worldV.animalSpawnerTimes = new Dictionary<Vector3V02, double>();
			if (legacyData.animalSpawnerTimes != null)
			{
				foreach (KeyValuePair<Vector3, float> animalSpawnerTime in legacyData.animalSpawnerTimes)
				{
					worldV.animalSpawnerTimes.Add(UpgradeFrom(animalSpawnerTime.Key), animalSpawnerTime.Value);
				}
			}
			worldV.offer = UpgradeFrom(legacyData.offer);
			worldV.dailyOfferCreateTime = legacyData.dailyOfferCreateTime;
			worldV.lastRancherOfferId = legacyData.lastRancherOfferId;
			worldV.liquidSourceUnits = new Dictionary<Vector3V02, float>();
			if (legacyData.liquidSourceUnits != null)
			{
				foreach (KeyValuePair<Vector3, float> liquidSourceUnit in legacyData.liquidSourceUnits)
				{
					worldV.liquidSourceUnits.Add(UpgradeFrom(liquidSourceUnit.Key), liquidSourceUnit.Value);
				}
			}
			worldV.weather = legacyData.weather;
			worldV.weatherUntil = legacyData.weatherUntil;
			Dictionary<Vector3V02, int> dictionary = new Dictionary<Vector3V02, int>();
			if (legacyData.gordoEatenCounts != null)
			{
				foreach (KeyValuePair<Vector3, int> gordoEatenCount in legacyData.gordoEatenCounts)
				{
					dictionary.Add(UpgradeFrom(gordoEatenCount.Key), gordoEatenCount.Value);
				}
			}
			worldV.gordos = WorldV12.UpgradeGordoEatsFrom(WorldV10.UpgradeGordoEatsFrom(dictionary));
			worldV.placedGadgets = new Dictionary<string, PlacedGadgetV02>();
			worldV.treasurePods = new Dictionary<string, TreasurePod.State>();
			worldV.switches = new Dictionary<string, SwitchHandler.State>();
			worldV.puzzleSlotsFilled = new Dictionary<string, bool>();
			return worldV;
		}

		private ExchangeOfferV03 UpgradeFrom(ExchangeDirector.Offer legacyData)
		{
			ExchangeOfferV03 exchangeOfferV = new ExchangeOfferV03();
			exchangeOfferV.requests = new List<RequestedItemEntryV03>();
			exchangeOfferV.rewards = new List<ItemEntryV03>();
			if (legacyData == null)
			{
				exchangeOfferV.hasOffer = false;
				return exchangeOfferV;
			}
			exchangeOfferV.hasOffer = true;
			exchangeOfferV.expireTime = legacyData.expireTime;
			exchangeOfferV.offerId = legacyData.offerId;
			exchangeOfferV.rancherId = legacyData.rancherId;
			foreach (ExchangeDirector.RequestedItemEntry request in legacyData.requests)
			{
				exchangeOfferV.requests.Add(UpgradeFrom(request));
			}
			foreach (ExchangeDirector.ItemEntry reward in legacyData.rewards)
			{
				exchangeOfferV.rewards.Add(UpgradeFrom(reward));
			}
			return exchangeOfferV;
		}

		private RequestedItemEntryV03 UpgradeFrom(ExchangeDirector.RequestedItemEntry legacyData)
		{
			RequestedItemEntryV02 requestedItemEntryV = new RequestedItemEntryV02();
			if (legacyData != null)
			{
				requestedItemEntryV.count = legacyData.count;
				requestedItemEntryV.id = legacyData.id;
				requestedItemEntryV.progress = legacyData.progress;
			}
			return new RequestedItemEntryV03(requestedItemEntryV);
		}

		private ItemEntryV03 UpgradeFrom(ExchangeDirector.ItemEntry legacyData)
		{
			ItemEntryV02 itemEntryV = new ItemEntryV02();
			if (legacyData != null)
			{
				itemEntryV.id = legacyData.id;
				itemEntryV.count = legacyData.count;
			}
			return new ItemEntryV03(itemEntryV);
		}

		private ResourceWaterV03 UpgradeFrom(WorldData.ResourceWater legacyData)
		{
			ResourceWaterV03 resourceWaterV = new ResourceWaterV03();
			if (legacyData == null)
			{
				return resourceWaterV;
			}
			resourceWaterV.spawn = legacyData.spawn;
			resourceWaterV.water = legacyData.water;
			return resourceWaterV;
		}

		private GameAchieveV03 UpgradeFrom(GameAchieveData legacyData)
		{
			GameAchieveV03 gameAchieveV = new GameAchieveV03();
			if (legacyData == null)
			{
				return gameAchieveV;
			}
			gameAchieveV.gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();
			if (legacyData.gameFloatStatDict != null)
			{
				foreach (KeyValuePair<AchievementsDirector.GameFloatStat, float> item in legacyData.gameFloatStatDict)
				{
					gameAchieveV.gameFloatStatDict.Add(item.Key, item.Value);
				}
			}
			gameAchieveV.gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();
			if (legacyData.gameIntStatDict != null)
			{
				foreach (KeyValuePair<AchievementsDirector.GameIntStat, int> item2 in legacyData.gameIntStatDict)
				{
					gameAchieveV.gameIntStatDict.Add(item2.Key, item2.Value);
				}
			}
			gameAchieveV.gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();
			if (legacyData.gameIdDictStatDict != null)
			{
				foreach (KeyValuePair<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> item3 in legacyData.gameIdDictStatDict)
				{
					gameAchieveV.gameIdDictStatDict.Add(item3.Key, item3.Value);
				}
			}
			gameAchieveV.gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>();
			return gameAchieveV;
		}

		private PlayerV07 UpgradeFrom(PlayerData legacyData)
		{
			PlayerV07 playerV = new PlayerV07();
			if (legacyData == null)
			{
				return playerV;
			}
			playerV.playerPos = UpgradeFrom(legacyData.playerPos);
			playerV.playerRotEuler = UpgradeFrom(legacyData.playerRotEuler);
			playerV.health = legacyData.health;
			playerV.energy = legacyData.energy;
			playerV.radiation = legacyData.rad;
			playerV.currency = legacyData.currency;
			playerV.ammo = new List<AmmoDataV02>();
			if (legacyData.ammo != null)
			{
				Ammo.AmmoData[] ammo = legacyData.ammo;
				foreach (Ammo.AmmoData legacyData2 in ammo)
				{
					playerV.ammo.Add(UpgradeFrom(legacyData2));
				}
			}
			if (legacyData.upgrades != null)
			{
				playerV.upgrades = new List<PlayerState.Upgrade>(legacyData.upgrades);
			}
			else
			{
				playerV.upgrades = new List<PlayerState.Upgrade>();
			}
			if (legacyData.upgradeLocks != null)
			{
				playerV.upgradeLocks = new Dictionary<PlayerState.Upgrade, PlayerState.UpgradeLockData>(PlayerV07.UpgradeFrom(PlayerV06.UpgradeFrom(legacyData.upgradeLocks)));
			}
			else
			{
				playerV.upgradeLocks = new Dictionary<PlayerState.Upgrade, PlayerState.UpgradeLockData>();
			}
			playerV.mail = new List<MailV02>();
			if (legacyData.mail != null)
			{
				foreach (MailDirector.Mail item in legacyData.mail)
				{
					playerV.mail.Add(UpgradeFrom(item));
				}
			}
			playerV.keys = legacyData.keys;
			if (legacyData.progress != null)
			{
				playerV.progress = new Dictionary<ProgressDirector.ProgressType, int>(legacyData.progress);
			}
			else
			{
				playerV.progress = new Dictionary<ProgressDirector.ProgressType, int>();
			}
			if (legacyData.delayedProgress != null)
			{
				playerV.delayedProgress = new Dictionary<ProgressDirector.ProgressType, List<double>>(PlayerV06.UpgradeFrom(legacyData.delayedProgress));
			}
			else
			{
				playerV.delayedProgress = new Dictionary<ProgressDirector.ProgressType, List<double>>();
			}
			playerV.currencyEverCollected = legacyData.currencyEverCollected;
			playerV.gameMode = legacyData.gameMode;
			playerV.gameIconId = legacyData.gameIconId;
			playerV.version = legacyData.version;
			playerV.blueprints = new List<Gadget.Id>();
			playerV.availBlueprints = new List<Gadget.Id>();
			playerV.blueprintLocks = new Dictionary<Gadget.Id, GadgetDirector.BlueprintLockData>();
			playerV.gadgets = new Dictionary<Gadget.Id, int>();
			playerV.craftMatCounts = new Dictionary<Identifiable.Id, int>();
			playerV.availUpgrades = new List<PlayerState.Upgrade>();
			foreach (PlayerState.Upgrade value in Enum.GetValues(typeof(PlayerState.Upgrade)))
			{
				if (!playerV.upgradeLocks.ContainsKey(value))
				{
					playerV.availUpgrades.Add(value);
				}
			}
			return playerV;
		}

		private MailV02 UpgradeFrom(MailDirector.Mail legacyData)
		{
			MailV02 mailV = new MailV02();
			if (legacyData == null)
			{
				return mailV;
			}
			mailV.isRead = legacyData.read;
			mailV.mailType = legacyData.type;
			mailV.messageKey = legacyData.key;
			return mailV;
		}

		private RanchV05 UpgradeFrom(RanchData legacyData)
		{
			RanchV05 ranchV = new RanchV05();
			if (legacyData == null)
			{
				return ranchV;
			}
			Dictionary<Vector3V02, AccessDoor.State> dictionary = new Dictionary<Vector3V02, AccessDoor.State>();
			if (legacyData.GetAccessDoorStates() != null)
			{
				foreach (KeyValuePair<Vector3, AccessDoor.State> accessDoorState in legacyData.GetAccessDoorStates())
				{
					dictionary.Add(UpgradeFrom(accessDoorState.Key), accessDoorState.Value);
				}
			}
			ranchV.accessDoorStates = RanchV05.UpgradeDoorsFrom(dictionary);
			ranchV.plots = new List<LandPlotV04>();
			if (legacyData.GetPlots() != null)
			{
				RanchData.LandPlotData[] plots = legacyData.GetPlots();
				foreach (RanchData.LandPlotData legacyData2 in plots)
				{
					ranchV.plots.Add(UpgradeFrom(legacyData2));
				}
			}
			return ranchV;
		}

		private LandPlotV04 UpgradeFrom(RanchData.LandPlotData legacyData)
		{
			LandPlotV04 landPlotV = new LandPlotV04();
			if (legacyData == null)
			{
				return landPlotV;
			}
			landPlotV.id = LandPlotV04.GetIdFromPos(UpgradeFrom(legacyData.pos));
			landPlotV.typeId = legacyData.id;
			landPlotV.upgrades = new List<LandPlot.Upgrade>();
			if (legacyData.upgrades != null)
			{
				foreach (LandPlot.Upgrade upgrade in legacyData.upgrades)
				{
					landPlotV.upgrades.Add(upgrade);
				}
			}
			landPlotV.attachedId = legacyData.attachedId;
			landPlotV.attachedDeathTime = legacyData.attachedDeathTime;
			landPlotV.siloAmmo = new Dictionary<SiloStorage.StorageType, List<AmmoDataV02>>();
			if (legacyData.siloAmmo != null)
			{
				foreach (KeyValuePair<SiloStorage.StorageType, Ammo.AmmoData[]> item in legacyData.siloAmmo)
				{
					List<AmmoDataV02> list = new List<AmmoDataV02>();
					Ammo.AmmoData[] value = item.Value;
					foreach (Ammo.AmmoData legacyData2 in value)
					{
						list.Add(UpgradeFrom(legacyData2));
					}
					landPlotV.siloAmmo.Add(item.Key, list);
				}
			}
			landPlotV.feederNextTime = legacyData.feederNextTime;
			landPlotV.feederPendingCount = legacyData.feederPendingCount;
			landPlotV.collectorNextTime = legacyData.collectorNextTime;
			landPlotV.fastforwarderDisableTime = legacyData.fastforwarderDisableTime;
			return landPlotV;
		}

		private AmmoDataV02 UpgradeFrom(Ammo.AmmoData legacyData)
		{
			AmmoDataV02 ammoDataV = new AmmoDataV02();
			if (legacyData == null)
			{
				return ammoDataV;
			}
			ammoDataV.count = legacyData.count;
			ammoDataV.emotionData = UpgradeFrom(legacyData.emotionData);
			ammoDataV.id = legacyData.id;
			return ammoDataV;
		}

		private List<ActorDataV05> UpgradeFrom(ActorsData legacyData)
		{
			List<ActorDataV05> list = new List<ActorDataV05>();
			if (legacyData == null)
			{
				return list;
			}
			if (legacyData.GetActors() != null)
			{
				ActorsData.ActorData[] array = legacyData.GetActors();
				foreach (ActorsData.ActorData legacyData2 in array)
				{
					list.Add(UpgradeFrom(legacyData2));
				}
			}
			return list;
		}

		private ActorDataV05 UpgradeFrom(ActorsData.ActorData legacyData)
		{
			ActorDataV05 actorDataV = new ActorDataV05();
			if (legacyData == null)
			{
				return actorDataV;
			}
			actorDataV.pos = UpgradeFrom(legacyData.pos);
			actorDataV.rot = UpgradeFrom(legacyData.rot);
			actorDataV.id = (int)legacyData.id;
			actorDataV.emotions = UpgradeFrom(legacyData.emotions);
			actorDataV.transformTime = legacyData.transformTime;
			actorDataV.reproduceTime = legacyData.reproduceTime;
			actorDataV.cycleData = UpgradeFrom(legacyData.cycleData);
			actorDataV.disabledAtTime = legacyData.disabledAtTime;
			actorDataV.isFeral = false;
			actorDataV.fashions = new List<Identifiable.Id>();
			return actorDataV;
		}

		private SlimeEmotionDataV02 UpgradeFrom(SlimeEmotionData legacyData)
		{
			SlimeEmotionDataV02 slimeEmotionDataV = new SlimeEmotionDataV02();
			slimeEmotionDataV.emotionData = new Dictionary<SlimeEmotions.Emotion, float>();
			if (legacyData == null)
			{
				return slimeEmotionDataV;
			}
			foreach (KeyValuePair<SlimeEmotions.Emotion, float> legacyDatum in legacyData)
			{
				slimeEmotionDataV.emotionData.Add(legacyDatum.Key, legacyDatum.Value);
			}
			return slimeEmotionDataV;
		}

		private ResourceCycleDataV03 UpgradeFrom(ResourceCycle.CycleData legacyData)
		{
			ResourceCycleDataV03 resourceCycleDataV = new ResourceCycleDataV03();
			if (legacyData == null)
			{
				return resourceCycleDataV;
			}
			resourceCycleDataV.progressTime = legacyData.progressTime;
			resourceCycleDataV.state = legacyData.state;
			return resourceCycleDataV;
		}

		private PediaV02 UpgradeFrom(PediaData legacyData)
		{
			PediaV02 pediaV = new PediaV02();
			if (legacyData == null)
			{
				return pediaV;
			}
			pediaV.progressGivenForPediaCount = legacyData.progressGivenForPediaCount;
			pediaV.unlockedIds = new List<string>();
			if (legacyData.unlockedIds != null)
			{
				foreach (string unlockedId in legacyData.unlockedIds)
				{
					pediaV.unlockedIds.Add(unlockedId);
				}
			}
			pediaV.completedTuts = new List<string>();
			if (legacyData.completedTuts != null)
			{
				foreach (string completedTut in pediaV.completedTuts)
				{
					pediaV.completedTuts.Add(completedTut);
				}
			}
			return pediaV;
		}

		private Vector3V02 UpgradeFrom(Vector3 legacyData)
		{
			return new Vector3V02
			{
				value = new Vector3(legacyData.x, legacyData.y, legacyData.z)
			};
		}

		public static void AssertAreEqual(GameV04 expected, GameV04 actual)
		{
			WorldV13.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV02.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV07.AssertAreEqual(expected.player, actual.player);
			RanchV05.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
		}
	}
}
