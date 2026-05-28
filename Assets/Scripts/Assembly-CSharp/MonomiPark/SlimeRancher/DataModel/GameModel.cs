using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GameModel : MonoBehaviour
	{
		public interface Participant
		{
			void InitModel(GameModel model);

			void SetModel(GameModel model);
		}

		public delegate void Unregistrant();

		public class IdContainer<M> where M : IdHandlerModel, new()
		{
			private readonly GameModel parent;

			private Dictionary<string, M> staticInstances = new Dictionary<string, M>();

			private List<M> dynamicInstances = new List<M>();

			public IEnumerable<M> Instances => staticInstances.Values.Concat(dynamicInstances);

			public IEnumerable<KeyValuePair<string, M>> StaticInstances => staticInstances;

			public IEnumerable<M> DynamicInstances => dynamicInstances;

			public M this[string key] => staticInstances[key];

			public IdContainer(GameModel parent)
			{
				this.parent = parent;
			}

			public bool ContainsKey(string key)
			{
				return staticInstances.ContainsKey(key);
			}

			public Unregistrant Register(IdHandlerModel.Participant participant)
			{
				M instance = new M();
				instance.SetParticipant(participant);
				if (!parent.expectingPush)
				{
					instance.Init();
					instance.NotifyParticipants();
				}
				string id = participant.GetId();
				if (!string.IsNullOrEmpty(id))
				{
					staticInstances[id] = instance;
					return delegate
					{
						Unregister(id);
					};
				}
				dynamicInstances.Add(instance);
				return delegate
				{
					Unregister(instance);
				};
			}

			public void Init()
			{
				foreach (M instance in Instances)
				{
					instance.Init();
				}
			}

			public void NotifyParticipants()
			{
				foreach (M instance in Instances)
				{
					instance.NotifyParticipants();
				}
			}

			private void Unregister(string id)
			{
				staticInstances.Remove(id);
			}

			private void Unregister(M instance)
			{
				dynamicInstances.Remove(instance);
			}
		}

		public bool expectingPush;

		public PlayerState.GameMode currGameMode;

		public Identifiable.Id gameIconId = Identifiable.Id.PINK_SLIME;

		private Dictionary<long, ActorModel> actors = new Dictionary<long, ActorModel>();

		private Dictionary<Identifiable.Id, List<ActorModel>> actorsByIdent = new Dictionary<Identifiable.Id, List<ActorModel>>();

		private long nextActorId = 100L;

		private Dictionary<string, GadgetSiteModel> gadgetSites = new Dictionary<string, GadgetSiteModel>();

		private Dictionary<string, GordoModel> gordos = new Dictionary<string, GordoModel>();

		private Dictionary<string, EchoNoteGordoModel> echoNoteGordos = new Dictionary<string, EchoNoteGordoModel>();

		private Dictionary<string, LandPlotModel> landPlots = new Dictionary<string, LandPlotModel>();

		private Dictionary<string, AccessDoorModel> doors = new Dictionary<string, AccessDoorModel>();

		private Dictionary<string, TreasurePodModel> pods = new Dictionary<string, TreasurePodModel>();

		private Dictionary<string, PuzzleSlotModel> slots = new Dictionary<string, PuzzleSlotModel>();

		private Dictionary<string, MasterSwitchModel> switches = new Dictionary<string, MasterSwitchModel>();

		private Dictionary<string, OasisModel> oases = new Dictionary<string, OasisModel>();

		private Dictionary<string, QuicksilverEnergyGeneratorModel> generators = new Dictionary<string, QuicksilverEnergyGeneratorModel>();

		private Dictionary<Vector3, SpawnResourceModel> resourceSpawners = new Dictionary<Vector3, SpawnResourceModel>();

		private List<DirectedAnimalSpawnerModel> animalSpawners = new List<DirectedAnimalSpawnerModel>();

		private List<SpawnerTriggerModel> spawnerTriggers = new List<SpawnerTriggerModel>();

		private List<KookadobaNodeModel> kookadobaNodes = new List<KookadobaNodeModel>();

		private PlayerModel player = new PlayerModel();

		private RanchModel ranch = new RanchModel();

		private HolidayModel holiday = new HolidayModel();

		private PediaModel pedia = new PediaModel();

		private TutorialsModel tutorial = new TutorialsModel();

		private GameAchievesModel gameAchieves = new GameAchievesModel();

		private ProfileAchievesModel profileAchieves = new ProfileAchievesModel();

		private WorldModel world = new WorldModel();

		private GadgetsModel gadgets = new GadgetsModel();

		private MailModel mail = new MailModel();

		private ProgressModel progress = new ProgressModel();

		private DecorizerModel decorizer = new DecorizerModel();

		private AppearancesModel appearances = new AppearancesModel();

		private InstrumentModel instrument = new InstrumentModel();

		public IdContainer<LiquidSourceModel> LiquidSources;

		private List<Participant> participants = new List<Participant>();

		public GlitchSlimulationModel Glitch { get; private set; }

		public GameModel()
		{
			LiquidSources = new IdContainer<LiquidSourceModel>(this);
			Glitch = new GlitchSlimulationModel(this);
			player.SetWorldModel(world);
		}

		public PlayerModel GetPlayerModel()
		{
			return player;
		}

		public RanchModel GetRanchModel()
		{
			return ranch;
		}

		public HolidayModel GetHolidayModel()
		{
			return holiday;
		}

		public PediaModel GetPediaModel()
		{
			return pedia;
		}

		public GameAchievesModel GetGameAchievesModel()
		{
			return gameAchieves;
		}

		public ProfileAchievesModel GetProfileAchievesModel()
		{
			return profileAchieves;
		}

		public TutorialsModel GetTutorialsModel()
		{
			return tutorial;
		}

		public GadgetsModel GetGadgetsModel()
		{
			return gadgets;
		}

		public ProgressModel GetProgressModel()
		{
			return progress;
		}

		public MailModel GetMailModel()
		{
			return mail;
		}

		public WorldModel GetWorldModel()
		{
			return world;
		}

		public DecorizerModel GetDecorizerModel()
		{
			return decorizer;
		}

		public AppearancesModel GetAppearancesModel()
		{
			return appearances;
		}

		public InstrumentModel GetInstrumentModel()
		{
			return instrument;
		}

		public ActorModel GetActorModel(long actorId)
		{
			return actors[actorId];
		}

		public GordoModel GetGordoModel(string gordoId)
		{
			GordoModel value = null;
			gordos.TryGetValue(gordoId, out value);
			return value;
		}

		public EchoNoteGordoModel GetEchoNoteGordoModel(string id)
		{
			echoNoteGordos.TryGetValue(id, out var value);
			return value;
		}

		public Dictionary<string, GordoModel> AllGordos()
		{
			return gordos;
		}

		public Dictionary<long, ActorModel> AllActors()
		{
			return actors;
		}

		public LandPlotModel GetLandPlotModel(string plotId)
		{
			LandPlotModel value = null;
			landPlots.TryGetValue(plotId, out value);
			return value;
		}

		public Dictionary<string, LandPlotModel> AllLandPlots()
		{
			return landPlots;
		}

		public Dictionary<string, AccessDoorModel> AllDoors()
		{
			return doors;
		}

		public Dictionary<string, TreasurePodModel> AllPods()
		{
			return pods;
		}

		public Dictionary<string, PuzzleSlotModel> AllSlots()
		{
			return slots;
		}

		public Dictionary<string, MasterSwitchModel> AllSwitches()
		{
			return switches;
		}

		public Dictionary<string, OasisModel> AllOases()
		{
			return oases;
		}

		public Dictionary<string, QuicksilverEnergyGeneratorModel> AllGenerators()
		{
			return generators;
		}

		public Dictionary<string, GadgetSiteModel> AllGadgetSites()
		{
			return gadgetSites;
		}

		public IEnumerable<SpawnerTriggerModel> AllSpawnerTriggers()
		{
			return spawnerTriggers;
		}

		public IEnumerable<DirectedAnimalSpawnerModel> AllAnimalSpawners()
		{
			return animalSpawners;
		}

		public IEnumerable<SpawnResourceModel> AllResourceSpawners()
		{
			return resourceSpawners.Values;
		}

		public IEnumerable<KookadobaNodeModel> AllKookadobaNodes()
		{
			return kookadobaNodes;
		}

		public Dictionary<string, EchoNoteGordoModel> AllEchoNoteGordos()
		{
			return echoNoteGordos;
		}

		public void InstantiateTestActor(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, GameObject testObj)
		{
			ActorModel actorModel = CreateActorModel(actorId, ident, regionSetId, testObj);
			if (actorModel is SlimeModel)
			{
				SlimeEmotions.EmotionState initAgitation = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.AGITATION, 0f, 0f, 0f, 0f);
				SlimeEmotions.EmotionState initFear = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.AGITATION, 0f, 0f, 0f, 0f);
				SlimeEmotions.EmotionState initHunger = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.AGITATION, 0f, 0f, 0f, 0f);
				((SlimeModel)actorModel).MaybeSetInitEmotions(initAgitation, initFear, initHunger);
			}
			actors[actorId] = actorModel;
			if (!actorsByIdent.ContainsKey(ident))
			{
				actorsByIdent[ident] = new List<ActorModel>();
			}
			actorsByIdent[ident].Add(actorModel);
		}

		public GameObject InstantiateActor(GameObject prefab, RegionRegistry.RegionSetId regionSetId, Vector3 pos, Quaternion rot, bool nonActorOk = false)
		{
			return InstantiateActor(nextActorId++, prefab, regionSetId, pos, rot, nonActorOk, skipNotify: false);
		}

		public GameObject InstantiateActorWithoutNotify(long actorId, GameObject prefab, RegionRegistry.RegionSetId regionSetId, Vector3 pos, Quaternion rot)
		{
			nextActorId = Math.Max(nextActorId, actorId + 1);
			return InstantiateActor(actorId, prefab, regionSetId, pos, rot, nonActorOk: false, skipNotify: true);
		}

		public void RegisterStartingActor(GameObject actorObj, RegionRegistry.RegionSetId regionSetId)
		{
			RegisterActor(nextActorId++, actorObj, regionSetId, nonActorOk: false, skipNotify: false);
		}

		private GameObject InstantiateActor(long actorId, GameObject prefab, RegionRegistry.RegionSetId regionSetId, Vector3 pos, Quaternion rot, bool nonActorOk, bool skipNotify)
		{
			GameObject gameObject = SRBehaviour.InstantiateDynamic(prefab, pos, rot, asActor: true);
			RegisterActor(actorId, gameObject, regionSetId, nonActorOk, skipNotify);
			return gameObject;
		}

		private void RegisterActor(long actorId, GameObject gameObj, RegionRegistry.RegionSetId regionSetId, bool nonActorOk, bool skipNotify)
		{
			Identifiable.Id id = Identifiable.GetId(gameObj);
			if (id != 0)
			{
				ActorModel actorModel = ((id != Identifiable.Id.PLAYER) ? CreateActorModel(actorId, id, regionSetId, gameObj) : player);
				actors[actorId] = actorModel;
				if (!actorsByIdent.ContainsKey(id))
				{
					actorsByIdent[id] = new List<ActorModel>();
				}
				actorsByIdent[id].Add(actorModel);
				actorModel.Init(gameObj);
				if (!skipNotify)
				{
					actorModel.NotifyParticipants(gameObj);
				}
			}
			else if (!nonActorOk)
			{
				Log.Warning("Instantiating actor which does not have an ident", "name", gameObj.name);
			}
		}

		public void DestroyActorModel(GameObject gameObj)
		{
			long actorId = Identifiable.GetActorId(gameObj);
			if (!actors.ContainsKey(actorId))
			{
				return;
			}
			ActorModel item = actors.Get(actorId);
			actors.Remove(actorId);
			Identifiable.Id id = Identifiable.GetId(gameObj);
			if (id != 0)
			{
				List<ActorModel> list = actorsByIdent[id];
				list.Remove(item);
				if (list.Count == 0)
				{
					actorsByIdent.Remove(id);
				}
			}
		}

		private ActorModel CreateActorModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, GameObject gameObj)
		{
			switch (ident)
			{
			case Identifiable.Id.PLAYER:
				throw new ArgumentException("Attempting to create ActorModel for Id.PLAYER.");
			case Identifiable.Id.GLITCH_SLIME:
				return new GlitchSlimeModel(actorId, ident, regionSetId, gameObj.transform);
			default:
				if (Identifiable.IsSlime(ident))
				{
					return new SlimeModel(actorId, ident, regionSetId, gameObj.transform);
				}
				if (Identifiable.IsAnimal(ident))
				{
					return new AnimalModel(actorId, ident, regionSetId, gameObj.transform);
				}
				if (Identifiable.IsFood(ident) || Identifiable.Id.LEMON_PHASE == ident)
				{
					return new ProduceModel(actorId, ident, regionSetId, gameObj.transform);
				}
				if (Identifiable.IsPlort(ident))
				{
					return new PlortModel(actorId, ident, regionSetId, gameObj);
				}
				if (Identifiable.IsCraft(ident))
				{
					return new ScienceMatModel(actorId, ident, regionSetId, gameObj.transform);
				}
				if (Identifiable.IsEcho(ident))
				{
					return new EchoModel(actorId, ident, regionSetId, gameObj.transform);
				}
				return new BasicActorModel(actorId, ident, regionSetId, gameObj.transform);
			}
		}

		public void OnNewGameLoaded()
		{
			player.OnNewGameLoaded(currGameMode);
			progress.OnNewGameLoaded(currGameMode);
			gadgets.OnNewGameLoaded(currGameMode);
			OnNewGameLoadedInitWorldLists(currGameMode);
		}

		private void OnNewGameLoadedInitWorldLists(PlayerState.GameMode currGameMode)
		{
			if (currGameMode != PlayerState.GameMode.TIME_LIMIT_V2)
			{
				return;
			}
			foreach (AccessDoorModel item in AllDoors().Values.Where(delegate(AccessDoorModel d)
			{
				if (d.IsUnlockedForGameMode(currGameMode))
				{
					return true;
				}
				ProgressDirector.ProgressType[] array = d.progress;
				foreach (ProgressDirector.ProgressType type in array)
				{
					if (!progress.HasProgress(type))
					{
						return false;
					}
				}
				return true;
			}))
			{
				item.state = AccessDoor.State.OPEN;
			}
			foreach (GordoModel value in AllGordos().Values)
			{
				value.OnNewGameLoaded(currGameMode);
			}
			foreach (PuzzleSlotModel value2 in AllSlots().Values)
			{
				value2.OnNewGameLoaded(currGameMode);
			}
		}

		public GameObject InstantiateGadget(GameObject prefab, GadgetSiteModel site)
		{
			return InstantiateGadget(prefab, site, skipNotify: false);
		}

		public GameObject InstantiateGadgetWithoutNotify(GameObject prefab, GadgetSiteModel site)
		{
			return InstantiateGadget(prefab, site, skipNotify: true);
		}

		private GameObject InstantiateGadget(GameObject prefab, GadgetSiteModel site, bool skipNotify)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, site.transform, worldPositionStays: false);
			GadgetModel gadgetModel = CreateGadgetModel(site, gameObject);
			site.Attach(gameObject, gadgetModel);
			gadgetModel.Init(gameObject);
			if (!skipNotify)
			{
				gadgetModel.NotifyParticipants(gameObject);
			}
			return gameObject;
		}

		public GadgetModel CreateGadgetModel(GadgetSiteModel site, GameObject gameObj)
		{
			Gadget.Id id = gameObj.GetComponent<Gadget>().id;
			if (Gadget.IsExtractor(id))
			{
				return new ExtractorModel(id, site.id, gameObj.transform);
			}
			if (Gadget.IsWarpDepot(id))
			{
				return new WarpDepotModel(id, site.id, gameObj.transform);
			}
			if (Gadget.IsSnare(id))
			{
				return new SnareModel(id, site.id, gameObj.transform);
			}
			if (Gadget.IsEchoNet(id))
			{
				return new EchoNetModel(id, site.id, gameObj.transform);
			}
			if (Gadget.IsDrone(id))
			{
				return new DroneModel(id, site.id, gameObj.transform);
			}
			return new BasicGadgetModel(id, site.id, gameObj.transform);
		}

		public void DestroyGadgetModel(string siteId, GameObject gameObj)
		{
			if (gadgetSites.ContainsKey(siteId))
			{
				gadgetSites[siteId].Detach();
			}
		}

		public void RegisterGameModelParticipant(Participant participant)
		{
			if (!expectingPush)
			{
				participant.InitModel(this);
				participant.SetModel(this);
			}
			participants.Add(participant);
		}

		public void Init()
		{
			foreach (Participant participant in participants)
			{
				participant.InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			foreach (Participant participant in participants)
			{
				participant.SetModel(this);
			}
		}

		public void RegisterGordo(string gordoId, GameObject gordoObj)
		{
			GordoModel gordoModel = new GordoModel();
			gordoModel.SetGameObject(gordoObj);
			if (!expectingPush)
			{
				gordoModel.Init();
				gordoModel.NotifyParticipants();
			}
			gordos[gordoId] = gordoModel;
		}

		public void UnregisterGordo(string gordoId)
		{
			gordos.Remove(gordoId);
		}

		public void RegisterEchoNoteGordo(string id, GameObject gameObject)
		{
			EchoNoteGordoModel echoNoteGordoModel = new EchoNoteGordoModel(gameObject);
			if (!expectingPush)
			{
				echoNoteGordoModel.Init();
				echoNoteGordoModel.NotifyParticipants();
			}
			echoNoteGordos[id] = echoNoteGordoModel;
		}

		public void UnregisterEchoNoteGordo(string id)
		{
			echoNoteGordos.Remove(id);
		}

		public void RegisterLandPlot(string plotId, GameObject plotLocObj)
		{
			LandPlotModel landPlotModel = new LandPlotModel();
			landPlotModel.SetGameObject(plotLocObj);
			if (!expectingPush)
			{
				landPlotModel.Init();
				landPlotModel.NotifyParticipants();
			}
			landPlots[plotId] = landPlotModel;
		}

		public void UnregisterLandPlot(string plotId)
		{
			landPlots.Remove(plotId);
		}

		public bool IncludesFullyUpgradedCorralCoopAndSilo()
		{
			return LandPlotModel.IncludesFullyUpgradedCorralCoopAndSilo(landPlots.Values);
		}

		public HashSet<Identifiable.Id> GetRanchResourceTypes(HashSet<Identifiable.Id> resourceClass)
		{
			return LandPlotModel.GetRanchResourceTypes(landPlots.Values, resourceClass);
		}

		public void RegisterRanchCell(string cellId, RanchCellModel.Participant participant)
		{
			ranch.RegisterRanchCell(cellId, participant, expectingPush);
		}

		public void UnregisterRanchCell(string cellId)
		{
			ranch.UnregisterRanchCell(cellId);
		}

		public void RegisterDoor(string doorId, GameObject doorObj)
		{
			AccessDoorModel accessDoorModel = new AccessDoorModel();
			accessDoorModel.SetGameObject(doorObj);
			if (!expectingPush)
			{
				accessDoorModel.Init();
				accessDoorModel.NotifyParticipants();
			}
			doors[doorId] = accessDoorModel;
		}

		public void UnregisterDoor(string doorId)
		{
			doors.Remove(doorId);
		}

		public void RegisterPod(string podId, GameObject podObj)
		{
			TreasurePodModel treasurePodModel = new TreasurePodModel();
			treasurePodModel.SetGameObject(podObj);
			if (!expectingPush)
			{
				treasurePodModel.Init();
				treasurePodModel.NotifyParticipants();
			}
			pods[podId] = treasurePodModel;
		}

		public void UnregisterPod(string podId)
		{
			pods.Remove(podId);
		}

		public void RegisterSlot(string slotId, GameObject slotObj)
		{
			PuzzleSlotModel puzzleSlotModel = new PuzzleSlotModel();
			puzzleSlotModel.SetGameObject(slotObj);
			if (!expectingPush)
			{
				puzzleSlotModel.Init();
				puzzleSlotModel.NotifyParticipants();
			}
			slots[slotId] = puzzleSlotModel;
		}

		public void UnregisterSlot(string slotId)
		{
			slots.Remove(slotId);
		}

		public void RegisterSwitch(string switchId, GameObject switchObj)
		{
			MasterSwitchModel masterSwitchModel = new MasterSwitchModel();
			masterSwitchModel.SetGameObject(switchObj);
			if (!expectingPush)
			{
				masterSwitchModel.Init();
				masterSwitchModel.NotifyParticipants();
			}
			switches[switchId] = masterSwitchModel;
		}

		public void UnregisterSwitch(string switchId)
		{
			switches.Remove(switchId);
		}

		public void RegisterOasis(string oasisId, GameObject oasisObj)
		{
			OasisModel oasisModel = new OasisModel();
			oasisModel.SetGameObject(oasisObj);
			if (!expectingPush)
			{
				oasisModel.Init();
				oasisModel.NotifyParticipants();
			}
			oases[oasisId] = oasisModel;
		}

		public void UnregisterOasis(string oasisId)
		{
			oases.Remove(oasisId);
		}

		public void RegisterGenerator(string genId, QuicksilverEnergyGeneratorModel.Participant part)
		{
			QuicksilverEnergyGeneratorModel quicksilverEnergyGeneratorModel = new QuicksilverEnergyGeneratorModel();
			quicksilverEnergyGeneratorModel.SetParticipant(part);
			if (!expectingPush)
			{
				quicksilverEnergyGeneratorModel.Init();
				quicksilverEnergyGeneratorModel.NotifyParticipants();
			}
			generators[genId] = quicksilverEnergyGeneratorModel;
		}

		public void RegisterGadgetSite(string siteId, GameObject gameObject, GadgetSiteModel.Participant part)
		{
			GadgetSiteModel gadgetSiteModel = new GadgetSiteModel(siteId, gameObject.transform);
			gadgetSiteModel.SetParticipant(part);
			if (!expectingPush)
			{
				gadgetSiteModel.Init();
				gadgetSiteModel.NotifyParticipants();
			}
			gadgetSites[siteId] = gadgetSiteModel;
		}

		public void UnregisterGadgetSite(string siteId)
		{
			gadgetSites.Remove(siteId);
		}

		public void RegisterKookadobaNode(KookadobaNodeModel.Participant part)
		{
			KookadobaNodeModel kookadobaNodeModel = new KookadobaNodeModel();
			kookadobaNodeModel.SetParticipant(part);
			if (!expectingPush)
			{
				kookadobaNodeModel.Init();
				kookadobaNodeModel.NotifyParticipants();
			}
			kookadobaNodes.Add(kookadobaNodeModel);
		}

		public void RegisterResourceSpawner(Vector3 pos, SpawnResourceModel.Participant part)
		{
			SpawnResourceModel spawnResourceModel = new SpawnResourceModel();
			spawnResourceModel.SetParticipant(part);
			if (!expectingPush)
			{
				spawnResourceModel.Init();
				spawnResourceModel.NotifyParticipants();
			}
			resourceSpawners[pos] = spawnResourceModel;
		}

		public void UnregisterResourceSpawner(Vector3 pos)
		{
			resourceSpawners.Remove(pos);
		}

		public void RegisterAnimalSpawner(DirectedAnimalSpawnerModel.Participant part)
		{
			DirectedAnimalSpawnerModel directedAnimalSpawnerModel = new DirectedAnimalSpawnerModel();
			directedAnimalSpawnerModel.SetParticipant(part);
			if (!expectingPush)
			{
				directedAnimalSpawnerModel.Init();
				directedAnimalSpawnerModel.NotifyParticipants();
			}
			animalSpawners.Add(directedAnimalSpawnerModel);
		}

		public void RegisterSpawnerTrigger(SpawnerTriggerModel.Participant part)
		{
			SpawnerTriggerModel spawnerTriggerModel = new SpawnerTriggerModel();
			spawnerTriggerModel.SetParticipant(part);
			if (!expectingPush)
			{
				spawnerTriggerModel.Init();
				spawnerTriggerModel.NotifyParticipants();
			}
			spawnerTriggers.Add(spawnerTriggerModel);
		}

		public void RegisterPlayerParticipant(PlayerModel.Participant participant)
		{
			player.AddParticipant(participant);
			if (!expectingPush)
			{
				participant.InitModel(player);
				participant.SetModel(player);
			}
		}

		public void RegisterRanch(RanchModel.Participant participant)
		{
			ranch.SetParticipant(participant);
			if (!expectingPush)
			{
				ranch.Init();
				ranch.NotifyParticipants();
			}
		}

		public void RegisterHoliday(HolidayModel.Participant participant)
		{
			holiday.SetParticipant(participant);
			if (!expectingPush)
			{
				holiday.Init();
				holiday.NotifyParticipants();
			}
		}

		public void RegisterPedia(PediaModel.Participant participant)
		{
			pedia.SetParticipant(participant);
			if (!expectingPush)
			{
				pedia.Init();
				pedia.NotifyParticipants();
			}
		}

		public void RegisterAppearances(AppearancesModel.Participant participant)
		{
			appearances.SetParticipant(participant);
			if (!expectingPush)
			{
				appearances.Init();
				appearances.NotifyParticipants();
			}
		}

		public void RegisterInstrument(InstrumentModel.Participant participant)
		{
			instrument.SetParticipant(participant);
			if (!expectingPush)
			{
				instrument.Init();
				instrument.NotifyParticipants();
			}
		}

		public void RegisterTutorials(TutorialsModel.Participant participant)
		{
			tutorial.SetParticipant(participant);
			if (!expectingPush)
			{
				tutorial.Init();
				tutorial.NotifyParticipants();
			}
		}

		public void RegisterMail(MailModel.Participant participant)
		{
			mail.SetParticipant(participant);
			if (!expectingPush)
			{
				mail.Init();
				mail.NotifyParticipants();
			}
		}

		public void RegisterProgress(ProgressModel.Participant participant)
		{
			progress.SetParticipant(participant);
			if (!expectingPush)
			{
				progress.Init();
				progress.NotifyParticipants();
			}
		}

		public void RegisterGadgets(GadgetsModel.Participant participant)
		{
			gadgets.SetParticipant(participant);
			if (!expectingPush)
			{
				gadgets.Init();
				gadgets.NotifyParticipants();
			}
		}

		public void RegisterDecorizer(DecorizerModel.Participant participant)
		{
			decorizer.AddParticipant(participant);
			if (!expectingPush)
			{
				decorizer.Init();
				decorizer.NotifyParticipants();
			}
		}

		public void RegisterProfileAchievements(ProfileAchievesModel.Participant participant)
		{
			profileAchieves.Init();
			profileAchieves.SetParticipant(participant);
			if (!expectingPush)
			{
				participant.InitModel(profileAchieves);
				participant.SetModel(profileAchieves);
			}
		}

		public void RegisterGameAchievements(GameAchievesModel.Participant participant)
		{
			gameAchieves.Init();
			gameAchieves.SetParticipant(participant);
			if (!expectingPush)
			{
				participant.InitModel(gameAchieves);
				participant.SetModel(gameAchieves);
			}
		}

		public void RegisterWorldParticipant(WorldModel.Participant part)
		{
			world.RegisterParticipant(part);
			if (!expectingPush)
			{
				part.InitModel(world);
				part.SetModel(world);
			}
		}

		public void ResetPlayerForGameMode(GameModeSettings modeSettings)
		{
			player.ResetForGameMode(modeSettings);
		}

		public bool IsGameOver()
		{
			double? endGameTime = GetPlayerModel().endGameTime;
			return GetWorldModel().worldTime >= endGameTime;
		}

		public void Push(PlayerState.GameMode gameMode, Identifiable.Id gameIconId)
		{
			currGameMode = gameMode;
			this.gameIconId = gameIconId;
		}

		public void Pull(out PlayerState.GameMode gameMode, out Identifiable.Id gameIconId)
		{
			gameMode = currGameMode;
			gameIconId = this.gameIconId;
		}
	}
}
