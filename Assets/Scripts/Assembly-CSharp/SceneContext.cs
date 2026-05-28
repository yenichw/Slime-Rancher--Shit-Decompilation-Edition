using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using RichPresence;
using UnityEngine;

public class SceneContext : SRSingleton<SceneContext>
{
	public delegate void SceneLoadDelegate(SceneContext ctx);

	public delegate void OnSessionEnded_Delegate();

	public static SceneLoadDelegate beforeSceneLoaded;

	public static SceneLoadDelegate onSceneLoaded;

	public static SceneLoadDelegate onNextSceneAwake;

	private GameObject player;

	private Dictionary<RegionRegistry.RegionSetId, WakeUpDestination> wakeUpDestinations = new Dictionary<RegionRegistry.RegionSetId, WakeUpDestination>(RegionRegistry.RegionSetIdComparer.Instance);

	public ObjectPool fxPool;

	public ObjectPool appearanceObjectPool;

	public SlimeAppearanceDirector SlimeAppearanceDirector;

	public PlayerState PlayerState { get; private set; }

	public EconomyDirector EconomyDirector { get; private set; }

	public ExchangeDirector ExchangeDirector { get; private set; }

	public PediaDirector PediaDirector { get; private set; }

	public TutorialDirector TutorialDirector { get; private set; }

	public MailDirector MailDirector { get; private set; }

	public ModDirector ModDirector { get; private set; }

	public ProgressDirector ProgressDirector { get; private set; }

	public AchievementsDirector AchievementsDirector { get; private set; }

	public TimeDirector TimeDirector { get; private set; }

	public AmbianceDirector AmbianceDirector { get; private set; }

	public GadgetDirector GadgetDirector { get; private set; }

	public PopupDirector PopupDirector { get; private set; }

	public TeleportNetwork TeleportNetwork { get; set; }

	public SceneParticleDirector SceneParticleDirector { get; set; }

	public MetadataDirector MetadataDirector { get; private set; }

	public ActorRegistry ActorRegistry { get; private set; }

	public RegionRegistry RegionRegistry { get; private set; }

	public GameModel GameModel { get; private set; }

	public SECTRDirector SECTRDirector => null;

	public RanchDirector RanchDirector { get; private set; }

	public HolidayDirector HolidayDirector { get; private set; }

	public InstrumentDirector InstrumentDirector { get; private set; }

	public GameModeConfig GameModeConfig { get; private set; }

	public GameObject Player
	{
		get
		{
			return player;
		}
		set
		{
			player = value;
			SRSingleton<GameContext>.Instance.InputDirector.NoteNewPlayer(player);
			PlayerZoneTracker = ((player != null) ? player.GetComponent<PlayerZoneTracker>() : null);
		}
	}

	public PlayerZoneTracker PlayerZoneTracker { get; private set; }

	public event OnSessionEnded_Delegate onSessionEnded;

	public override void Awake()
	{
		base.Awake();
		PlayerState = GetComponent<PlayerState>();
		EconomyDirector = GetComponent<EconomyDirector>();
		ExchangeDirector = GetComponent<ExchangeDirector>();
		PediaDirector = GetComponent<PediaDirector>();
		TutorialDirector = GetComponent<TutorialDirector>();
		MailDirector = GetComponent<MailDirector>();
		ModDirector = GetComponent<ModDirector>();
		ProgressDirector = GetComponent<ProgressDirector>();
		AchievementsDirector = GetComponent<AchievementsDirector>();
		TimeDirector = GetComponent<TimeDirector>();
		AmbianceDirector = GetComponent<AmbianceDirector>();
		GadgetDirector = GetComponent<GadgetDirector>();
		PopupDirector = GetComponent<PopupDirector>();
		ActorRegistry = GetComponent<ActorRegistry>();
		RanchDirector = GetComponent<RanchDirector>();
		HolidayDirector = GetComponent<HolidayDirector>();
		InstrumentDirector = GetComponent<InstrumentDirector>();
		SceneParticleDirector = GetComponent<SceneParticleDirector>();
		MetadataDirector = GetComponent<MetadataDirector>();
		RegionRegistry = GetComponent<RegionRegistry>();
		TeleportNetwork = GetComponent<TeleportNetwork>();
		GameModel = GetComponent<GameModel>();
		GameModeConfig = GetComponent<GameModeConfig>();
		PreloadObjectPoolOnState(fxPool, "fxPool", ObjectPoolConfig.StartupPoolMode.Awake);
		PreloadObjectPoolOnState(appearanceObjectPool, "appearanceObjectPool", ObjectPoolConfig.StartupPoolMode.Awake);
		if (onNextSceneAwake != null)
		{
			onNextSceneAwake(this);
			onNextSceneAwake = null;
		}
		onSessionEnded += delegate
		{
			AnalyticsUtil.CustomEvent("SessionEnded");
		};
	}

	private void PreloadObjectPoolOnState(ObjectPool pool, string poolName, ObjectPoolConfig.StartupPoolMode startupPoolMode)
	{
		if (pool.config.startupPoolMode == startupPoolMode)
		{
			pool.CreateStartupPools();
		}
	}

	public void Start()
	{
		if (beforeSceneLoaded != null)
		{
			try
			{
				beforeSceneLoaded(this);
			}
			finally
			{
				beforeSceneLoaded = null;
			}
		}
		GameModeConfig.InitForLevel();
		TimeDirector.InitForLevel();
		PediaDirector.InitForLevel();
		AchievementsDirector.InitForLevel();
		PlayerState.InitForLevel();
		EconomyDirector.InitForLevel();
		ExchangeDirector.InitForLevel();
		TutorialDirector.InitForLevel();
		MailDirector.InitForLevel();
		ModDirector.InitForLevel();
		ProgressDirector.InitForLevel();
		AmbianceDirector.InitForLevel();
		GadgetDirector.InitForLevel();
		PopupDirector.InitForLevel();
		SceneParticleDirector.InitForLevel();
		HolidayDirector.InitForLevel();
		SlimeAppearanceDirector.InitForLevel();
		InstrumentDirector.InitForLevel();
		RanchDirector.InitForLevel();
		if (Player != null)
		{
			Player.GetComponent<FirestormActivator>().InitForLevel();
		}
		if (Levels.isMainMenu())
		{
			SRSingleton<GameContext>.Instance.RichPresenceDirector.SetRichPresence(default(MainMenuData));
		}
		PreloadObjectPoolOnState(fxPool, "fxPool", ObjectPoolConfig.StartupPoolMode.Start);
		PreloadObjectPoolOnState(appearanceObjectPool, "appearanceObjectPool", ObjectPoolConfig.StartupPoolMode.Start);
		SRSingleton<GameContext>.Instance.DLCDirector.InitForLevel();
		if (!GameModel.expectingPush)
		{
			NoteGameFullyLoaded();
		}
		SRSingleton<GameContext>.Instance.AutoSaveDirector.OnSceneLoaded();
		if (onSceneLoaded != null)
		{
			try
			{
				onSceneLoaded(this);
			}
			finally
			{
				onSceneLoaded = null;
			}
		}
	}

	public void LateUpdate()
	{
		SlimeEat.ClearClaimedFood();
	}

	public void NoteGameFullyLoaded()
	{
		ProgressDirector.GameFullyLoaded();
	}

	public void OnApplicationQuit()
	{
		OnSessionEnded();
	}

	public void OnSessionEnded()
	{
		if (!Levels.isSpecial() && this.onSessionEnded != null)
		{
			this.onSessionEnded();
			this.onSessionEnded = null;
		}
	}

	public void Register(WakeUpDestination destination)
	{
		wakeUpDestinations[destination.deathRegionSetId] = destination;
	}

	public void Deregister(WakeUpDestination destination)
	{
		wakeUpDestinations.Remove(destination.deathRegionSetId);
	}

	public WakeUpDestination GetWakeUpDestination(RegionMember member)
	{
		if (wakeUpDestinations.ContainsKey(member.setId))
		{
			return wakeUpDestinations[member.setId];
		}
		return GetWakeUpDestination();
	}

	public WakeUpDestination GetWakeUpDestination()
	{
		return wakeUpDestinations[RegionRegistry.RegionSetId.HOME];
	}
}
