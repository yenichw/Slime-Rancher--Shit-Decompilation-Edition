using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

namespace MonomiPark.SlimeRancher
{
	public class SavedGame
	{
		private const float MAX_DIST_MATCH = 5f;

		private const float MAX_DIST_MATCH_SQR = 25f;

		private const float MAX_DIST_CLOSE_MATCH = 0.1f;

		public const float MAX_DIST_CLOSE_MATCH_SQR = 0.010000001f;

		private const float MAX_FAR_DIST_MATCH = 10f;

		public const float MAX_FAR_DIST_MATCH_SQR = 100f;

		private const float MAX_ACTOR_POSITION_COORD = 10000000f;

		private GameV12 gameState;

		private PrefabInstantiator prefabInstantiator;

		private SavedGameInfoProvider savedGameInfoProvider;

		private const string EXTENSION = ".sav";

		private const string TEMP_EXTENSION = ".tmp";

		public GameV12 GameState => gameState;

		public SavedGame(PrefabInstantiator prefabInstantiator, SavedGameInfoProvider gameModeEndTimeProvider)
		{
			this.prefabInstantiator = prefabInstantiator;
			savedGameInfoProvider = gameModeEndTimeProvider;
		}

		public string GetDisplayName()
		{
			if (gameState != null)
			{
				return gameState.displayName;
			}
			return "";
		}

		public string GetName()
		{
			if (gameState != null)
			{
				return gameState.gameName;
			}
			return "";
		}

		public void ClearName()
		{
			if (gameState != null)
			{
				gameState.gameName = null;
			}
		}

		public GameData.Summary LoadSummary(string saveName, Stream gameData)
		{
			try
			{
				GameV12 gameV = new GameV12();
				gameV.LoadSummary(gameData);
				if (string.IsNullOrEmpty(gameV.displayName))
				{
					gameV.displayName = saveName;
				}
				return new GameData.Summary(gameV.gameName, gameV.displayName, gameV.summary.iconId, gameV.summary.gameMode, gameV.summary.version, (int)Math.Floor(gameV.summary.worldTime * 1.1574074051168282E-05), gameV.summary.currency, gameV.summary.pediaCount, gameV.summary.isGameOver, gameV.summary.saveTimestamp, saveName, gameV.summary.saveNumber);
			}
			catch (Exception ex)
			{
				Log.Warning("Error while loading saved game summary.", "name", saveName, "Exception", ex.Message, "Stack Trace", ex.StackTrace);
				return new GameData.Summary(saveName);
			}
		}

		public void CreateNew(string name, string displayName)
		{
			gameState = new GameV12();
			gameState.gameName = name;
			gameState.displayName = displayName;
		}

		public void Load(Stream stream)
		{
			GameV12 gameV = new GameV12();
			gameV.Load(stream);
			gameState = gameV;
		}

		public void Save(Stream stream)
		{
			gameState.Write(stream);
		}

		public void Push(GameModel gameModel)
		{
			if (gameState == null)
			{
				throw new InvalidOperationException("There is no game state to restore.");
			}
			PushBase(gameModel, gameState.player);
			PushWorldGlobal(gameModel, gameState.world);
			Push(gameModel, gameState.achieve);
			Push(gameModel, gameState.player);
			Push(gameModel, gameState.pedia);
			Push(gameModel, gameState.ranch);
			PushWorldItems(gameModel, gameState.world);
			Push(gameModel, gameState.actors, gameState.world);
			Push(gameModel, gameState.holiday);
			Push(gameModel, gameState.appearances);
			Push(gameModel, gameState.instrument);
		}

		public void Push(GameModel gameModel, RanchV07 ranch)
		{
			Dictionary<string, LandPlotModel> dictionary = new Dictionary<string, LandPlotModel>(gameModel.AllLandPlots());
			foreach (LandPlotV08 plot in ranch.plots)
			{
				LandPlotToGameObject(plot, dictionary);
			}
			if (dictionary.Count > 0)
			{
				Log.Warning("Remaining unreplaced plots: " + dictionary.Count);
				foreach (LandPlotModel value2 in dictionary.Values)
				{
					value2.Init();
					value2.NotifyParticipants();
				}
			}
			Dictionary<string, AccessDoorModel> dictionary2 = new Dictionary<string, AccessDoorModel>(gameModel.AllDoors());
			foreach (KeyValuePair<string, AccessDoor.State> accessDoorState in ranch.accessDoorStates)
			{
				dictionary2.TryGetValue(accessDoorState.Key, out var value);
				if (value != null)
				{
					dictionary2.Remove(accessDoorState.Key);
					value.Init();
					value.Push(accessDoorState.Value);
				}
				else
				{
					Log.Debug("Skipping deserializing door, as it's missing.", "id", accessDoorState.Key);
				}
			}
			if (dictionary2.Count > 0)
			{
				Log.Warning("Remaining unreplaced doors: " + dictionary2.Count);
				foreach (AccessDoorModel value3 in dictionary2.Values)
				{
					value3.Init();
				}
			}
			foreach (AccessDoorModel value4 in gameModel.AllDoors().Values)
			{
				value4.NotifyParticipants();
			}
			RanchModel ranchModel = gameModel.GetRanchModel();
			ranchModel.Init();
			ranchModel.Push(ranch.palettes, ranch.ranchFastForward);
			ranchModel.NotifyParticipants();
		}

		private void LandPlotToGameObject(LandPlotV08 plotData, Dictionary<string, LandPlotModel> scenePlots)
		{
			scenePlots.TryGetValue(plotData.id, out var value);
			if (value == null)
			{
				Log.Warning("Did not find plot: " + plotData.id);
				return;
			}
			scenePlots.Remove(plotData.id);
			prefabInstantiator.InstantiatePlot(plotData.typeId, value, expectingPush: true);
			Dictionary<SiloStorage.StorageType, Ammo.Slot[]> dictionary = new Dictionary<SiloStorage.StorageType, Ammo.Slot[]>();
			foreach (KeyValuePair<SiloStorage.StorageType, List<AmmoDataV02>> item in plotData.siloAmmo)
			{
				dictionary[item.Key] = AmmoDataToSlots(item.Value);
			}
			value.Init();
			value.Push(plotData.feederNextTime, plotData.feederPendingCount, plotData.feederCycleSpeed, plotData.collectorNextTime, plotData.typeId, plotData.attachedId, plotData.upgrades, dictionary, plotData.siloActivatorIndices, plotData.ashUnits);
			value.NotifyParticipants();
		}

		private void PushWorldGlobal(GameModel gameModel, WorldV22 world)
		{
			Dictionary<ExchangeDirector.OfferType, ExchangeDirector.Offer> dictionary = new Dictionary<ExchangeDirector.OfferType, ExchangeDirector.Offer>();
			foreach (KeyValuePair<ExchangeDirector.OfferType, ExchangeOfferV04> offer in world.offers)
			{
				dictionary[offer.Key] = FromOfferData(offer.Value);
			}
			WorldModel worldModel = gameModel.GetWorldModel();
			worldModel.Init();
			worldModel.Push(world.econSeed, world.econSaturations, world.worldTime, dictionary, world.dailyOfferCreateTime, world.lastOfferRancherIds, world.pendingOfferRancherIds, world.weather, world.weatherUntil, world.firestorm.endStormTime, world.firestorm.nextStormTime, world.firestorm.stormPreparing, world.activeGingerPatches, world.occupiedPhaseSites);
			worldModel.NotifyParticipants();
		}

		private void PushWorldItems(GameModel gameModel, WorldV22 world)
		{
			SetSpawnTimes(gameModel, world.resourceSpawnerWater);
			SetKookadobaNodes(gameModel);
			SetTriggerTimes(gameModel, world.spawnerTriggerTimes);
			SetAnimalSpawnTimes(gameModel, world.animalSpawnerTimes);
			SetLiquidUnits(gameModel, world.liquidSourceUnits);
			SetGordos(gameModel, world.gordos);
			SetEchoNoteGordos(gameModel, world.echoNoteGordos);
			SetPlacedGadgets(gameModel, world.placedGadgets);
			SetTreasurePods(gameModel, world.treasurePods);
			SetSwitches(gameModel, world.switches);
			SetPuzzleSlotsFilled(gameModel, world.puzzleSlotsFilled);
			SetOasisStates(gameModel, world.oasisStates);
			SetQuicksilverEnergyGenerators(gameModel, world.quicksilverEnergyGenerators);
			GlitchSlimulationModel glitch = gameModel.Glitch;
			glitch.Init();
			glitch.Push(world.glitch);
			glitch.NotifyParticipants();
		}

		private void SetQuicksilverEnergyGenerators(GameModel gameModel, Dictionary<string, QuicksilverEnergyGeneratorV02> generators)
		{
			foreach (KeyValuePair<string, QuicksilverEnergyGeneratorModel> item in gameModel.AllGenerators())
			{
				item.Value.Init();
				if (generators.ContainsKey(item.Key))
				{
					QuicksilverEnergyGeneratorV02 quicksilverEnergyGeneratorV = generators[item.Key];
					item.Value.Push(quicksilverEnergyGeneratorV.state, quicksilverEnergyGeneratorV.timer);
				}
				item.Value.NotifyParticipants();
			}
		}

		private void SetSpawnTimes(GameModel gameModel, Dictionary<Vector3V02, ResourceWaterV03> resourceSpawnerWater)
		{
			float num = 0.010000001f;
			foreach (SpawnResourceModel item in gameModel.AllResourceSpawners())
			{
				item.Init();
				Vector3 pos = item.pos;
				bool flag = false;
				foreach (KeyValuePair<Vector3V02, ResourceWaterV03> item2 in resourceSpawnerWater)
				{
					if ((item2.Key.value - pos).sqrMagnitude < num)
					{
						item.Push(item2.Value.water, item2.Value.spawn);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.Log("Skipping deserializing spawn time, as it's missing.");
				}
				item.NotifyParticipants();
			}
		}

		private void SetKookadobaNodes(GameModel gameModel)
		{
			foreach (KookadobaNodeModel item in gameModel.AllKookadobaNodes())
			{
				item.Init();
				item.NotifyParticipants();
			}
		}

		private void SetTriggerTimes(GameModel gameModel, Dictionary<Vector3V02, double> spawnerTriggerTimes)
		{
			float num = 0.010000001f;
			foreach (SpawnerTriggerModel item in gameModel.AllSpawnerTriggers())
			{
				item.Init();
				Vector3 pos = item.pos;
				bool flag = false;
				foreach (KeyValuePair<Vector3V02, double> spawnerTriggerTime in spawnerTriggerTimes)
				{
					if ((spawnerTriggerTime.Key.value - pos).sqrMagnitude < num)
					{
						item.Push(spawnerTriggerTime.Value);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.Log("Skipping deserializing spawn time, as it's missing.");
				}
				item.NotifyParticipants();
			}
		}

		private void SetAnimalSpawnTimes(GameModel gameModel, Dictionary<Vector3V02, double> animalSpawnerTimes)
		{
			float num = 0.010000001f;
			foreach (DirectedAnimalSpawnerModel item in gameModel.AllAnimalSpawners())
			{
				item.Init();
				Vector3 pos = item.pos;
				bool flag = false;
				foreach (KeyValuePair<Vector3V02, double> animalSpawnerTime in animalSpawnerTimes)
				{
					if ((animalSpawnerTime.Key.value - pos).sqrMagnitude < num)
					{
						item.Push(animalSpawnerTime.Value);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.Log("Skipping deserializing spawn time, as it's missing.");
				}
				item.NotifyParticipants();
			}
		}

		private void SetLiquidUnits(GameModel gameModel, Dictionary<string, float> data)
		{
			Dictionary<string, LiquidSourceModel> dictionary = gameModel.LiquidSources.StaticInstances.ToDictionary((KeyValuePair<string, LiquidSourceModel> kv) => kv.Key, (KeyValuePair<string, LiquidSourceModel> kv) => kv.Value);
			foreach (KeyValuePair<string, float> datum in data)
			{
				dictionary.TryGetValue(datum.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(datum.Key);
					value.Init();
					value.Push(datum.Value);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing LiquidSource, as it's missing.", "id", datum.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced LiquidSources: " + dictionary.Count);
			foreach (LiquidSourceModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetGordos(GameModel gameModel, Dictionary<string, GordoV01> gordos)
		{
			Dictionary<string, GordoModel> dictionary = new Dictionary<string, GordoModel>(gameModel.AllGordos());
			foreach (KeyValuePair<string, GordoV01> gordo in gordos)
			{
				dictionary.TryGetValue(gordo.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(gordo.Key);
					value.Init();
					value.Push(gordo.Value.eatenCount, gordo.Value.fashions);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing gordo, as it's missing.", "id", gordo.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced gordos: " + dictionary.Count);
			foreach (GordoModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetEchoNoteGordos(GameModel gameModel, Dictionary<string, EchoNoteGordoV01> gordos)
		{
			Dictionary<string, EchoNoteGordoModel> dictionary = new Dictionary<string, EchoNoteGordoModel>(gameModel.AllEchoNoteGordos());
			foreach (KeyValuePair<string, EchoNoteGordoV01> gordo in gordos)
			{
				dictionary.TryGetValue(gordo.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(gordo.Key);
					value.Init();
					value.Push(gordo.Value);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing EchoNoteGordo, as it's missing.", "id", gordo.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced EchoNoteGordo: " + dictionary.Count);
			foreach (EchoNoteGordoModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetPlacedGadgets(GameModel gameModel, Dictionary<string, PlacedGadgetV08> placedGadgets)
		{
			Dictionary<string, PlacedGadgetV08> dictionary = new Dictionary<string, PlacedGadgetV08>(placedGadgets);
			foreach (KeyValuePair<string, GadgetSiteModel> item in gameModel.AllGadgetSites())
			{
				item.Value.Init();
				if (dictionary.ContainsKey(item.Key))
				{
					PlacedGadgetV08 gadget = dictionary[item.Key];
					Push(gameModel, gadget, item.Value);
					dictionary.Remove(item.Key);
				}
				item.Value.NotifyParticipants();
			}
		}

		private void SetTreasurePods(GameModel gameModel, Dictionary<string, TreasurePodV01> treasurePods)
		{
			Dictionary<string, TreasurePodModel> dictionary = new Dictionary<string, TreasurePodModel>(gameModel.AllPods());
			foreach (KeyValuePair<string, TreasurePodV01> treasurePod in treasurePods)
			{
				dictionary.TryGetValue(treasurePod.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(treasurePod.Key);
					value.Init();
					value.Push(treasurePod.Value.state, treasurePod.Value.spawnQueue);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing treasure pod, as it's missing.", "id", treasurePod.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced pods: " + dictionary.Count);
			foreach (TreasurePodModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetSwitches(GameModel gameModel, Dictionary<string, SwitchHandler.State> switches)
		{
			Dictionary<string, MasterSwitchModel> dictionary = new Dictionary<string, MasterSwitchModel>(gameModel.AllSwitches());
			foreach (KeyValuePair<string, SwitchHandler.State> @switch in switches)
			{
				dictionary.TryGetValue(@switch.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(@switch.Key);
					value.Init();
					value.Push(@switch.Value);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing master switch, as it's missing.", "id", @switch.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced switches: " + dictionary.Count);
			foreach (MasterSwitchModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetPuzzleSlotsFilled(GameModel gameModel, Dictionary<string, bool> puzzleSlotsFilled)
		{
			Dictionary<string, PuzzleSlotModel> dictionary = new Dictionary<string, PuzzleSlotModel>(gameModel.AllSlots());
			foreach (KeyValuePair<string, bool> item in puzzleSlotsFilled)
			{
				dictionary.TryGetValue(item.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(item.Key);
					value.Init();
					value.Push(item.Value);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing puzzle slot, as it's missing.", "id", item.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced pods: " + dictionary.Count);
			foreach (PuzzleSlotModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void SetOasisStates(GameModel gameModel, Dictionary<string, bool> oases)
		{
			Oasis.oasisSpheres.Clear();
			Dictionary<string, OasisModel> dictionary = new Dictionary<string, OasisModel>(gameModel.AllOases());
			foreach (KeyValuePair<string, bool> oasis in oases)
			{
				dictionary.TryGetValue(oasis.Key, out var value);
				if (value != null)
				{
					dictionary.Remove(oasis.Key);
					value.Init();
					value.Push(oasis.Value);
					value.NotifyParticipants();
				}
				else
				{
					Log.Debug("Skipping deserializing master switch, as it's missing.", "id", oasis.Key);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Log.Warning("Remaining unreplaced switches: " + dictionary.Count);
			foreach (OasisModel value2 in dictionary.Values)
			{
				value2.Init();
				value2.NotifyParticipants();
			}
		}

		private void Push(GameModel gameModel, PlacedGadgetV08 gadget, GadgetSiteModel siteModel)
		{
			GameObject gameObj = prefabInstantiator.InstantiateGadget(gadget.gadgetId, siteModel, gameModel);
			GadgetModel attached = siteModel.attached;
			attached.PushBase(gadget.waitForChargeupTime, gadget.yRotation);
			if (attached is ExtractorModel)
			{
				((ExtractorModel)attached).Push(gadget.extractorCyclesRemaining, gadget.extractorQueuedToProduce, gadget.extractorCycleEndTime, gadget.extractorNextProduceTime);
			}
			else if (attached is WarpDepotModel)
			{
				((WarpDepotModel)attached).Push(gadget.isPrimaryInLink, AmmoDataToSlots(gadget.ammo));
			}
			else if (attached is SnareModel)
			{
				((SnareModel)attached).Push(gadget.baitTypeId, gadget.gordoTypeId, gadget.gordoEatenCount, gadget.fashions);
			}
			else if (attached is EchoNetModel)
			{
				((EchoNetModel)attached).Push(gadget.lastSpawnTime);
			}
			else if (attached is DroneModel)
			{
				List<AmmoDataV02> list = new List<AmmoDataV02>();
				list.Add(gadget.drone.drone.ammo);
				((DroneModel)attached).Push(gadget.drone.drone.position.value, gadget.drone.drone.rotation.value, AmmoDataToSlots(list), gadget.drone.drone.fashions, gadget.drone.drone.noClip, gadget.drone.station.battery.time, gadget.drone.programs);
			}
			else
			{
				_ = attached is BasicGadgetModel;
			}
			attached.NotifyParticipants(gameObj);
		}

		private Dictionary<PlayerState.AmmoMode, Ammo.Slot[]> AmmoDataToSlots(Dictionary<PlayerState.AmmoMode, List<AmmoDataV02>> ammo)
		{
			if (ammo == null)
			{
				return null;
			}
			Dictionary<PlayerState.AmmoMode, Ammo.Slot[]> dictionary = new Dictionary<PlayerState.AmmoMode, Ammo.Slot[]>();
			foreach (KeyValuePair<PlayerState.AmmoMode, List<AmmoDataV02>> item in ammo)
			{
				dictionary[item.Key] = AmmoDataToSlots(item.Value);
			}
			return dictionary;
		}

		private Ammo.Slot[] AmmoDataToSlots(List<AmmoDataV02> ammo)
		{
			Ammo.Slot[] array = new Ammo.Slot[ammo.Count];
			for (int i = 0; i < ammo.Count; i++)
			{
				if (ammo[i] == null)
				{
					array[i] = new Ammo.Slot(Identifiable.Id.NONE, 0);
					continue;
				}
				array[i] = new Ammo.Slot(ammo[i].id, ammo[i].count);
				array[i].emotions = new SlimeEmotionData();
				foreach (KeyValuePair<SlimeEmotions.Emotion, float> emotionDatum in ammo[i].emotionData.emotionData)
				{
					array[i].emotions[emotionDatum.Key] = emotionDatum.Value;
				}
			}
			return array;
		}

		private void PushBase(GameModel gameModel, PlayerV14 player)
		{
			gameModel.Init();
			gameModel.Push(player.gameMode, player.gameIconId);
			gameModel.NotifyParticipants();
		}

		private void Push(GameModel gameModel, PlayerV14 player)
		{
			PlayerModel playerModel = gameModel.GetPlayerModel();
			playerModel.Init();
			playerModel.Push(player.health, player.energy, player.radiation, player.currency, player.currencyEverCollected, player.keys, AmmoDataToSlots(player.ammo), player.upgrades, player.availUpgrades, player.upgradeLocks, player.unlockedZoneMaps, player.regionSetId, player.playerPos.value, player.playerRotEuler.value, player.endGameTime);
			playerModel.NotifyParticipants();
			List<MailDirector.Mail> list = new List<MailDirector.Mail>();
			foreach (MailV02 item in player.mail)
			{
				if (item.mailType != MailDirector.Type.UPGRADE)
				{
					list.Add(new MailDirector.Mail
					{
						key = item.messageKey,
						read = item.isRead,
						type = item.mailType
					});
				}
			}
			MailModel mailModel = gameModel.GetMailModel();
			mailModel.Init();
			mailModel.Push(list);
			mailModel.NotifyParticipants();
			ProgressModel progressModel = gameModel.GetProgressModel();
			progressModel.Init();
			progressModel.Push(player.progress, player.delayedProgress);
			progressModel.NotifyParticipants();
			GadgetsModel gadgetsModel = gameModel.GetGadgetsModel();
			gadgetsModel.Init();
			gadgetsModel.Push(player.blueprints, player.availBlueprints, player.blueprintLocks, player.gadgets, player.craftMatCounts);
			gadgetsModel.NotifyParticipants();
			DecorizerModel decorizerModel = gameModel.GetDecorizerModel();
			decorizerModel.Init();
			decorizerModel.Push(player.decorizer);
			decorizerModel.NotifyParticipants();
		}

		private void Push(GameModel gameModel, List<ActorDataV09> actors, WorldV22 world)
		{
			foreach (ActorDataV09 actor in actors)
			{
				PushActorData(gameModel, actor, world);
			}
		}

		private bool IsInvalidActorPosition(Vector3 pos)
		{
			if (!float.IsNaN(pos.x) && !(pos.x > 10000000f) && !float.IsNaN(pos.y) && !(pos.y > 10000000f) && !float.IsNaN(pos.z))
			{
				return pos.z > 10000000f;
			}
			return true;
		}

		private void PushActorData(GameModel gameModel, ActorDataV09 actorData, WorldV22 world)
		{
			if (IsInvalidActorPosition(actorData.pos.value))
			{
				Log.Warning("Actor has invalid position during load. Skipping. id: " + (Identifiable.Id)actorData.typeId);
				return;
			}
			GameObject gameObj = prefabInstantiator.InstantiateActor(actorData.actorId, (Identifiable.Id)actorData.typeId, actorData.regionSetId, actorData.pos.value, actorData.rot.value, gameModel);
			ActorModel actorModel = gameModel.GetActorModel(actorData.actorId);
			if (actorModel is SlimeModel)
			{
				((SlimeModel)actorModel).Push(actorData);
				if (actorModel is GlitchSlimeModel)
				{
					((GlitchSlimeModel)actorModel).Push(world.glitch.slimes[actorData.actorId]);
				}
			}
			else if (actorModel is PlortModel)
			{
				((PlortModel)actorModel).Push(actorData.destroyTime);
			}
			else if (actorModel is AnimalModel)
			{
				((AnimalModel)actorModel).Push(actorData);
			}
			else if (actorModel is ProduceModel)
			{
				((ProduceModel)actorModel).Push(actorData.cycleData.state, actorData.cycleData.progressTime);
			}
			else
			{
				_ = actorModel is ScienceMatModel;
			}
			actorModel.NotifyParticipants(gameObj);
		}

		public void Push(GameModel gameModel, PediaV03 pedia)
		{
			List<PediaDirector.Id> unlocked = StringsToEnums<PediaDirector.Id>(pedia.unlockedIds);
			PediaModel pediaModel = gameModel.GetPediaModel();
			pediaModel.Init();
			pediaModel.Push(pedia.progressGivenForPediaCount, unlocked);
			pediaModel.NotifyParticipants();
			if (pedia.completedTuts == null)
			{
				Debug.Log("Had to create non-null completed tuts ids list.");
				pedia.completedTuts = new List<string>();
			}
			List<TutorialDirector.Id> completedIds = StringsToEnums<TutorialDirector.Id>(pedia.completedTuts);
			List<TutorialDirector.Id> popupQueue = StringsToEnums<TutorialDirector.Id>(pedia.popupQueue);
			TutorialsModel tutorialsModel = gameModel.GetTutorialsModel();
			tutorialsModel.Init();
			tutorialsModel.Push(completedIds, popupQueue);
			tutorialsModel.NotifyParticipants();
		}

		private static List<T> StringsToEnums<T>(IEnumerable<string> strings)
		{
			List<T> list = new List<T>();
			if (strings != null)
			{
				foreach (string @string in strings)
				{
					try
					{
						list.Add((T)Enum.Parse(typeof(T), @string));
					}
					catch (Exception)
					{
					}
				}
			}
			return list;
		}

		public void Push(GameModel gameModel, GameAchieveV03 achieve)
		{
			GameAchievesModel gameAchievesModel = gameModel.GetGameAchievesModel();
			gameAchievesModel.Init();
			gameAchievesModel.Push(achieve.gameFloatStatDict, achieve.gameDoubleStatDict, achieve.gameIntStatDict, achieve.gameIdDictStatDict);
			gameAchievesModel.NotifyParticipants();
		}

		private void Push(GameModel game, HolidayDirectorV02 persistence)
		{
			HolidayModel holidayModel = game.GetHolidayModel();
			holidayModel.Init();
			holidayModel.Push(persistence);
			holidayModel.NotifyParticipants();
		}

		private void Push(GameModel game, AppearancesV01 persistence)
		{
			AppearancesModel appearancesModel = game.GetAppearancesModel();
			appearancesModel.Init();
			appearancesModel.Push(persistence);
			appearancesModel.NotifyParticipants();
		}

		private void Push(GameModel game, InstrumentV01 persistence)
		{
			InstrumentModel instrumentModel = game.GetInstrumentModel();
			instrumentModel.Init();
			instrumentModel.Push(persistence);
			instrumentModel.NotifyParticipants();
		}

		public void Pull(GameModel gameModel)
		{
			GameV12 gameV = new GameV12();
			if (gameState != null)
			{
				gameV.gameName = gameState.gameName;
				gameV.displayName = gameState.displayName;
			}
			Pull(gameModel, gameV.world);
			Pull(gameModel, gameV.player);
			Pull(gameModel, gameV.ranch);
			Pull(gameModel, gameV.actors, gameV.world);
			Pull(gameModel, gameV.pedia);
			Pull(gameModel, gameV.achieve);
			gameV.holiday = gameModel.GetHolidayModel().Pull();
			gameV.appearances = gameModel.GetAppearancesModel().Pull();
			gameV.instrument = gameModel.GetInstrumentModel().Pull();
			gameV.summary = new GameSummaryV04();
			gameV.summary.version = gameV.player.version;
			gameV.summary.gameMode = gameV.player.gameMode;
			gameV.summary.iconId = gameV.player.gameIconId;
			gameV.summary.worldTime = gameV.world.worldTime;
			gameV.summary.currency = gameV.player.currency;
			gameV.summary.pediaCount = gameV.pedia.unlockedIds.Count;
			gameV.summary.saveTimestamp = DateTimeOffset.UtcNow;
			gameV.summary.isGameOver = gameModel.IsGameOver();
			if (gameState == null || gameState.summary == null)
			{
				gameV.summary.saveNumber = 0uL;
			}
			else
			{
				gameV.summary.saveNumber = gameState.summary.saveNumber + 1;
			}
			gameState = gameV;
		}

		private void Pull(GameModel gameModel, WorldV22 world)
		{
			gameModel.GetWorldModel().Pull(out world.econSeed, out world.econSaturations, out world.worldTime, out var offers, out world.dailyOfferCreateTime, out world.lastOfferRancherIds, out world.pendingOfferRancherIds, out world.weather, out world.weatherUntil, out world.firestorm.endStormTime, out world.firestorm.nextStormTime, out world.firestorm.stormPreparing, out world.activeGingerPatches, out world.occupiedPhaseSites);
			gameModel.Glitch.Pull(out world.glitch);
			world.offers = new Dictionary<ExchangeDirector.OfferType, ExchangeOfferV04>();
			foreach (KeyValuePair<ExchangeDirector.OfferType, ExchangeDirector.Offer> item in offers)
			{
				world.offers[item.Key] = ToOfferData(item.Value);
			}
			world.resourceSpawnerWater = new Dictionary<Vector3V02, ResourceWaterV03>();
			foreach (SpawnResourceModel item2 in gameModel.AllResourceSpawners())
			{
				Vector3V02 key = new Vector3V02
				{
					value = item2.pos
				};
				ResourceWaterV03 resourceWaterV = new ResourceWaterV03();
				item2.Pull(out resourceWaterV.water, out resourceWaterV.spawn);
				world.resourceSpawnerWater[key] = resourceWaterV;
			}
			world.spawnerTriggerTimes = new Dictionary<Vector3V02, double>();
			foreach (SpawnerTriggerModel item3 in gameModel.AllSpawnerTriggers())
			{
				Vector3V02 key2 = new Vector3V02
				{
					value = item3.pos
				};
				item3.Pull(out var nextTriggerTime);
				world.spawnerTriggerTimes[key2] = nextTriggerTime;
			}
			world.animalSpawnerTimes = new Dictionary<Vector3V02, double>();
			foreach (DirectedAnimalSpawnerModel item4 in gameModel.AllAnimalSpawners())
			{
				Vector3V02 key3 = new Vector3V02
				{
					value = item4.pos
				};
				item4.Pull(out var nextSpawnTime);
				world.animalSpawnerTimes[key3] = nextSpawnTime;
			}
			world.liquidSourceUnits = new Dictionary<string, float>();
			foreach (KeyValuePair<string, LiquidSourceModel> staticInstance in gameModel.LiquidSources.StaticInstances)
			{
				staticInstance.Value.Pull(out var unitsFilled);
				world.liquidSourceUnits[staticInstance.Key] = unitsFilled;
			}
			world.gordos = new Dictionary<string, GordoV01>();
			foreach (KeyValuePair<string, GordoModel> item5 in gameModel.AllGordos())
			{
				GordoV01 gordoV = new GordoV01();
				item5.Value.Pull(out gordoV.eatenCount, out gordoV.fashions);
				world.gordos[item5.Key] = gordoV;
			}
			world.echoNoteGordos = gameModel.AllEchoNoteGordos().ToDictionary((KeyValuePair<string, EchoNoteGordoModel> p) => p.Key, (KeyValuePair<string, EchoNoteGordoModel> p) => p.Value.Pull());
			world.placedGadgets = new Dictionary<string, PlacedGadgetV08>();
			foreach (KeyValuePair<string, GadgetSiteModel> item6 in gameModel.AllGadgetSites())
			{
				if (item6.Value.HasAttached())
				{
					PlacedGadgetV08 placedGadgetV = new PlacedGadgetV08();
					Pull(gameModel, placedGadgetV, item6.Value.attached);
					world.placedGadgets[item6.Key] = placedGadgetV;
				}
			}
			world.treasurePods = new Dictionary<string, TreasurePodV01>();
			foreach (KeyValuePair<string, TreasurePodModel> item7 in gameModel.AllPods())
			{
				TreasurePodV01 treasurePodV = new TreasurePodV01();
				item7.Value.Pull(out treasurePodV.state, out treasurePodV.spawnQueue);
				world.treasurePods[item7.Key] = treasurePodV;
			}
			world.switches = new Dictionary<string, SwitchHandler.State>();
			foreach (KeyValuePair<string, MasterSwitchModel> item8 in gameModel.AllSwitches())
			{
				item8.Value.Pull(out var state);
				world.switches[item8.Key] = state;
			}
			world.puzzleSlotsFilled = new Dictionary<string, bool>();
			foreach (KeyValuePair<string, PuzzleSlotModel> item9 in gameModel.AllSlots())
			{
				item9.Value.Pull(out var filled);
				world.puzzleSlotsFilled[item9.Key] = filled;
			}
			world.oasisStates = new Dictionary<string, bool>();
			foreach (KeyValuePair<string, OasisModel> item10 in gameModel.AllOases())
			{
				item10.Value.Pull(out var isLive);
				world.oasisStates[item10.Key] = isLive;
			}
			world.quicksilverEnergyGenerators = new Dictionary<string, QuicksilverEnergyGeneratorV02>();
			foreach (KeyValuePair<string, QuicksilverEnergyGeneratorModel> item11 in gameModel.AllGenerators())
			{
				QuicksilverEnergyGeneratorV02 quicksilverEnergyGeneratorV = new QuicksilverEnergyGeneratorV02();
				item11.Value.Pull(out quicksilverEnergyGeneratorV.state, out quicksilverEnergyGeneratorV.timer);
				world.quicksilverEnergyGenerators[item11.Key] = quicksilverEnergyGeneratorV;
			}
			world.teleportNodeActivations = new Dictionary<string, bool>();
		}

		public ExchangeDirector.Offer FromOfferData(ExchangeOfferV04 offer)
		{
			if (offer.hasOffer)
			{
				List<ExchangeDirector.ItemEntry> list = new List<ExchangeDirector.ItemEntry>();
				if (offer.rewards != null)
				{
					foreach (ItemEntryV03 reward in offer.rewards)
					{
						list.Add(new ExchangeDirector.ItemEntry(reward.id, reward.count, reward.nonIdentReward));
					}
				}
				List<ExchangeDirector.RequestedItemEntry> list2 = new List<ExchangeDirector.RequestedItemEntry>();
				if (offer.requests != null)
				{
					foreach (RequestedItemEntryV03 request in offer.requests)
					{
						list2.Add(new ExchangeDirector.RequestedItemEntry(request.id, request.count, request.progress, request.nonIdentReward));
					}
				}
				return new ExchangeDirector.Offer(offer.offerId, offer.rancherId, offer.expireTime, offer.earlyExchangeTime, list2, list);
			}
			return null;
		}

		public ExchangeOfferV04 ToOfferData(ExchangeDirector.Offer o)
		{
			ExchangeOfferV04 exchangeOfferV = new ExchangeOfferV04();
			exchangeOfferV.rewards = new List<ItemEntryV03>();
			exchangeOfferV.requests = new List<RequestedItemEntryV03>();
			if (o == null)
			{
				exchangeOfferV.hasOffer = false;
				return exchangeOfferV;
			}
			exchangeOfferV.hasOffer = true;
			exchangeOfferV.expireTime = o.expireTime;
			exchangeOfferV.earlyExchangeTime = o.earlyExchangeTime;
			exchangeOfferV.offerId = o.offerId;
			exchangeOfferV.rancherId = o.rancherId;
			foreach (ExchangeDirector.ItemEntry reward in o.rewards)
			{
				exchangeOfferV.rewards.Add(new ItemEntryV03
				{
					id = reward.id,
					count = reward.count,
					nonIdentReward = reward.specReward
				});
			}
			foreach (ExchangeDirector.RequestedItemEntry request in o.requests)
			{
				exchangeOfferV.requests.Add(new RequestedItemEntryV03
				{
					id = request.id,
					count = request.count,
					progress = request.progress,
					nonIdentReward = request.specReward
				});
			}
			return exchangeOfferV;
		}

		private Dictionary<PlayerState.AmmoMode, List<AmmoDataV02>> AmmoDataFromSlots(Dictionary<PlayerState.AmmoMode, Ammo.Slot[]> slots)
		{
			if (slots == null)
			{
				return null;
			}
			Dictionary<PlayerState.AmmoMode, List<AmmoDataV02>> dictionary = new Dictionary<PlayerState.AmmoMode, List<AmmoDataV02>>();
			foreach (KeyValuePair<PlayerState.AmmoMode, Ammo.Slot[]> slot in slots)
			{
				dictionary[slot.Key] = AmmoDataFromSlots(slot.Value);
			}
			return dictionary;
		}

		private List<AmmoDataV02> AmmoDataFromSlots(Ammo.Slot[] slots)
		{
			List<AmmoDataV02> list = new List<AmmoDataV02>(slots.Length);
			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i] != null)
				{
					list.Add(new AmmoDataV02
					{
						id = slots[i].id,
						count = slots[i].count,
						emotionData = new SlimeEmotionDataV02
						{
							emotionData = slots[i].emotions
						}
					});
				}
				else
				{
					list.Add(new AmmoDataV02
					{
						id = Identifiable.Id.NONE,
						count = 0,
						emotionData = new SlimeEmotionDataV02
						{
							emotionData = new SlimeEmotionData()
						}
					});
				}
			}
			return list;
		}

		private void Pull(GameModel gameModel, PlacedGadgetV08 gadget, GadgetModel model)
		{
			gadget.gadgetId = model.ident;
			model.PullBase(out gadget.waitForChargeupTime, out gadget.yRotation);
			if (model is ExtractorModel)
			{
				((ExtractorModel)model).Pull(out gadget.extractorCyclesRemaining, out gadget.extractorQueuedToProduce, out gadget.extractorCycleEndTime, out gadget.extractorNextProduceTime);
			}
			else if (model is WarpDepotModel)
			{
				((WarpDepotModel)model).Pull(out gadget.isPrimaryInLink, out var slots);
				gadget.ammo = AmmoDataFromSlots(slots);
			}
			else if (model is SnareModel)
			{
				((SnareModel)model).Pull(out gadget.baitTypeId, out gadget.gordoTypeId, out gadget.gordoEatenCount, out gadget.fashions);
			}
			else if (model is EchoNetModel)
			{
				((EchoNetModel)model).Pull(out gadget.lastSpawnTime);
			}
			else if (model is DroneModel)
			{
				gadget.drone = new DroneGadgetV01();
				((DroneModel)model).Pull(out gadget.drone.drone.position.value, out gadget.drone.drone.rotation.value, out var ammoSlots, out gadget.drone.drone.fashions, out gadget.drone.drone.noClip, out gadget.drone.station.battery.time, out gadget.drone.programs);
				List<AmmoDataV02> list = AmmoDataFromSlots(ammoSlots);
				gadget.drone.drone.ammo = ((list.Count >= 1) ? list[0] : null);
			}
			else
			{
				_ = model is BasicGadgetModel;
			}
		}

		private void Pull(GameModel gameModel, PlayerV14 player)
		{
			gameModel.GetPlayerModel().Pull(out player.health, out player.energy, out player.radiation, out player.currency, out player.currencyEverCollected, out player.keys, out var ammoSlots, out player.upgrades, out player.availUpgrades, out player.upgradeLocks, out player.unlockedZoneMaps, out player.regionSetId, out var position, out var rotation, out player.endGameTime);
			if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
			{
				Debug.Log("Player position was set to NaN on serialization! ZOMG!");
				position = savedGameInfoProvider.GetWakeUpDestination();
			}
			player.playerPos = new Vector3V02
			{
				value = position
			};
			player.playerRotEuler = new Vector3V02
			{
				value = rotation
			};
			player.ammo = AmmoDataFromSlots(ammoSlots);
			gameModel.Pull(out player.gameMode, out player.gameIconId);
			player.mail = new List<MailV02>();
			gameModel.GetMailModel().Pull(out var mail);
			foreach (MailDirector.Mail item in mail)
			{
				player.mail.Add(new MailV02
				{
					isRead = item.read,
					mailType = item.type,
					messageKey = item.key
				});
			}
			gameModel.GetProgressModel().Pull(out player.progress, out player.delayedProgress);
			gameModel.GetGadgetsModel().Pull(out player.blueprints, out player.availBlueprints, out player.blueprintLocks, out player.gadgets, out player.craftMatCounts);
			gameModel.GetDecorizerModel().Pull(out player.decorizer);
			player.version = savedGameInfoProvider.GetVersion();
		}

		private void Pull(GameModel gameModel, List<ActorDataV09> actors, WorldV22 world)
		{
			foreach (KeyValuePair<long, ActorModel> item in gameModel.AllActors())
			{
				Identifiable.Id ident = item.Value.ident;
				if (!Identifiable.SCENE_OBJECTS.Contains(ident) && ident != Identifiable.Id.QUICKSILVER_SLIME)
				{
					actors.Add(BuildActorData(gameModel, (int)ident, item.Value.actorId, item.Value, world));
				}
			}
		}

		private ActorDataV09 BuildActorData(GameModel gameModel, int typeId, long actorId, ActorModel actorModel, WorldV22 world)
		{
			ActorDataV09 persistence = new ActorDataV09();
			persistence.typeId = typeId;
			persistence.actorId = actorId;
			persistence.pos = new Vector3V02
			{
				value = actorModel.GetPos()
			};
			persistence.rot = new Vector3V02
			{
				value = actorModel.GetRot().eulerAngles
			};
			persistence.regionSetId = actorModel.currRegionSetId;
			persistence.cycleData = new ResourceCycleDataV03();
			persistence.emotions = new SlimeEmotionDataV02();
			if (actorModel is SlimeModel)
			{
				((SlimeModel)actorModel).Pull(ref persistence);
				if (actorModel is GlitchSlimeModel)
				{
					world.glitch.slimes[persistence.actorId] = ((GlitchSlimeModel)actorModel).Pull();
				}
			}
			else if (actorModel is PlortModel)
			{
				((PlortModel)actorModel).Pull(out persistence.destroyTime);
			}
			else if (actorModel is AnimalModel)
			{
				((AnimalModel)actorModel).Pull(ref persistence);
			}
			else if (actorModel is ProduceModel)
			{
				((ProduceModel)actorModel).Pull(out persistence.cycleData.state, out persistence.cycleData.progressTime);
			}
			else
			{
				_ = actorModel is ScienceMatModel;
			}
			return persistence;
		}

		private void Pull(GameModel gameModel, RanchV07 ranch)
		{
			ranch.accessDoorStates = new Dictionary<string, AccessDoor.State>();
			foreach (KeyValuePair<string, AccessDoorModel> item in gameModel.AllDoors())
			{
				item.Value.Pull(out var state);
				ranch.accessDoorStates[item.Key] = state;
			}
			ranch.plots = new List<LandPlotV08>();
			foreach (KeyValuePair<string, LandPlotModel> item2 in gameModel.AllLandPlots())
			{
				LandPlotV08 landPlotV = new LandPlotV08();
				landPlotV.id = item2.Key;
				item2.Value.Pull(out landPlotV.feederNextTime, out landPlotV.feederPendingCount, out landPlotV.feederCycleSpeed, out landPlotV.collectorNextTime, out landPlotV.typeId, out landPlotV.attachedId, out landPlotV.upgrades, out var siloAmmo, out landPlotV.siloActivatorIndices, out landPlotV.ashUnits);
				landPlotV.siloAmmo = new Dictionary<SiloStorage.StorageType, List<AmmoDataV02>>();
				foreach (KeyValuePair<SiloStorage.StorageType, Ammo.Slot[]> item3 in siloAmmo)
				{
					landPlotV.siloAmmo[item3.Key] = AmmoDataFromSlots(item3.Value);
				}
				ranch.plots.Add(landPlotV);
			}
			gameModel.GetRanchModel().Pull(out ranch.palettes, out ranch.ranchFastForward);
		}

		private void Pull(GameModel gameModel, PediaV03 pedia)
		{
			gameModel.GetPediaModel().Pull(out pedia.progressGivenForPediaCount, out var unlocked);
			pedia.unlockedIds = new List<string>();
			foreach (PediaDirector.Id item in unlocked)
			{
				pedia.unlockedIds.Add(Enum.GetName(typeof(PediaDirector.Id), item));
			}
			pedia.completedTuts = new List<string>();
			gameModel.GetTutorialsModel().Pull(out var completedIds, out var popupQueue);
			foreach (TutorialDirector.Id item2 in completedIds)
			{
				pedia.completedTuts.Add(Enum.GetName(typeof(TutorialDirector.Id), item2));
			}
			foreach (TutorialDirector.Id item3 in popupQueue)
			{
				pedia.popupQueue.Add(Enum.GetName(typeof(TutorialDirector.Id), item3));
			}
		}

		private void Pull(GameModel gameModel, GameAchieveV03 achieve)
		{
			gameModel.GetGameAchievesModel().Pull(out achieve.gameFloatStatDict, out achieve.gameDoubleStatDict, out achieve.gameIntStatDict, out achieve.gameIdDictStatDict);
		}
	}
}
