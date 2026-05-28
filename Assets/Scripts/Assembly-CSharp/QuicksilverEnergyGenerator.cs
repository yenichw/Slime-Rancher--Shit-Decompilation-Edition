using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class QuicksilverEnergyGenerator : IdHandler, QuicksilverEnergyGeneratorModel.Participant, VacDisplayTimer.TimeSource
{
	public enum State
	{
		INACTIVE = 0,
		COUNTDOWN = 1,
		ACTIVE = 2,
		COOLDOWN = 3
	}

	public class StateComparer : IEqualityComparer<State>
	{
		public static StateComparer Instance = new StateComparer();

		public bool Equals(State a, State b)
		{
			return a == b;
		}

		public int GetHashCode(State state)
		{
			return (int)state;
		}
	}

	public delegate void OnStateChanged();

	[Tooltip("Duration, in game minutes, that the game will countdown until the race begins.")]
	public float countdownMinutes;

	[Tooltip("Duration, in game hours, that the generator will stay active.")]
	public float activeHours;

	[Tooltip("Duration, in game hours, that the generator will cooldown.")]
	public float cooldownHours;

	[Tooltip("FX to display when the generator is inactive. (optional)")]
	public GameObject inactiveFX;

	[Tooltip("FX to display when the generator is active. (optional)")]
	public GameObject activeFX;

	[Tooltip("FX to display when the generator is cooling down. (optional)")]
	public GameObject cooldownFX;

	[Tooltip("CountdownUI prefab.")]
	public GameObject countdownUIPrefab;

	private GameObject countdownUI;

	[Tooltip("SFX played when the generator is ready to be activated. (optional)")]
	public SECTR_AudioCue onInactiveCue;

	[Tooltip("SFX played when the countdown timer begins. (optional)")]
	public SECTR_AudioCue onCountdownCue;

	[Tooltip("SFX played when the cooldown timer begins. (optional)")]
	public SECTR_AudioCue onCooldownCue;

	[Tooltip("SFX played when the cooldown timer begins. (2D, optional)")]
	public SECTR_AudioCue onCooldownCue2D;

	public static List<QuicksilverEnergyGenerator> allGenerators = new List<QuicksilverEnergyGenerator>();

	public OnStateChanged onStateChanged;

	private TimeDirector timeDirector;

	private PlayerDeathHandler deathHandler;

	private TutorialDirector tutDirector;

	private QuicksilverEnergyGeneratorModel model;

	public void Awake()
	{
		tutDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGenerator(base.id, this);
	}

	public void Start()
	{
		allGenerators.Add(this);
		if (SRSingleton<SceneContext>.Instance != null)
		{
			deathHandler = SRSingleton<SceneContext>.Instance.Player.GetComponent<PlayerDeathHandler>();
			PlayerDeathHandler playerDeathHandler = deathHandler;
			playerDeathHandler.onPlayerDeath = (PlayerDeathHandler.OnPlayerDeath)Delegate.Combine(playerDeathHandler.onPlayerDeath, new PlayerDeathHandler.OnPlayerDeath(OnPlayerDeath));
		}
	}

	public void OnDestroy()
	{
		allGenerators.Remove(this);
		if (deathHandler != null)
		{
			PlayerDeathHandler playerDeathHandler = deathHandler;
			playerDeathHandler.onPlayerDeath = (PlayerDeathHandler.OnPlayerDeath)Delegate.Remove(playerDeathHandler.onPlayerDeath, new PlayerDeathHandler.OnPlayerDeath(OnPlayerDeath));
		}
		Destroyer.Destroy(countdownUI, "QuicksilverEnergyGenerator.OnDestroy");
	}

	public void InitModel(QuicksilverEnergyGeneratorModel model)
	{
		model.state = State.INACTIVE;
		model.timer = null;
	}

	public void SetModel(QuicksilverEnergyGeneratorModel model)
	{
		this.model = model;
		if (this.model.state == State.ACTIVE || this.model.state == State.COUNTDOWN)
		{
			SetState(State.COOLDOWN, enableSFX: false);
			return;
		}
		double? timer = model.timer;
		SetState(this.model.state, enableSFX: false);
		this.model.timer = timer;
	}

	public bool Activate()
	{
		if (model.state == State.INACTIVE)
		{
			tutDirector.MaybeShowPopup(TutorialDirector.Id.RACE_GENERATOR);
			tutDirector.OnQuicksilverRaceActivated();
			SRSingleton<GameContext>.Instance.MusicDirector.SetValleyRaceMode(enabled: true);
			onStateChanged = (OnStateChanged)Delegate.Combine(onStateChanged, new OnStateChanged(DisableRaceMusicOnStateChanged));
			SetState(State.COUNTDOWN, enableSFX: true);
			return true;
		}
		return false;
	}

	private void DisableRaceMusicOnStateChanged()
	{
		if (model.state == State.COOLDOWN || model.state == State.INACTIVE)
		{
			SRSingleton<GameContext>.Instance.MusicDirector.SetValleyRaceMode(enabled: false);
			onStateChanged = (OnStateChanged)Delegate.Remove(onStateChanged, new OnStateChanged(DisableRaceMusicOnStateChanged));
		}
	}

	public bool ExtendActiveDuration(float hours)
	{
		if (model.state == State.ACTIVE)
		{
			tutDirector.MaybeShowPopup(TutorialDirector.Id.RACE_CHECKPOINT);
			model.timer = TimeDirector.HoursFromTime(hours, model.timer.Value);
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (model.timer.HasValue && timeDirector.HasReached(model.timer.Value))
		{
			SetState((State)((int)(model.state + 1) % Enum.GetNames(typeof(State)).Length), enableSFX: true);
		}
	}

	public State GetState()
	{
		return model.state;
	}

	private void SetState(State state, bool enableSFX)
	{
		Destroyer.Destroy(countdownUI, "QuicksilverEnergyGenerator.SetState");
		model.state = state;
		if (model.state == State.COUNTDOWN)
		{
			model.timer = timeDirector.HoursFromNow(countdownMinutes * (1f / 60f));
			if (enableSFX)
			{
				SECTR_AudioSystem.Play(onCountdownCue, base.transform.position, loop: false);
			}
			SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>().GetComponentInChildren<VacDisplayTimer>().SetQuicksilverEnergyGenerator(this);
			countdownUI = UnityEngine.Object.Instantiate(countdownUIPrefab);
			countdownUI.GetComponent<HUDCountdownUI>().SetCountdownTime(countdownMinutes);
		}
		else if (model.state == State.ACTIVE)
		{
			model.timer = timeDirector.HoursFromNow(activeHours);
		}
		else if (model.state == State.COOLDOWN)
		{
			model.timer = timeDirector.HoursFromNow(cooldownHours);
			if (enableSFX)
			{
				SECTR_AudioSystem.Play(onCooldownCue, base.transform.position, loop: false);
				SECTR_AudioSystem.Play(onCooldownCue2D, Vector3.zero, loop: false);
			}
		}
		else
		{
			model.timer = null;
			if (enableSFX)
			{
				SECTR_AudioSystem.Play(onInactiveCue, base.transform.position, loop: false);
			}
		}
		if (inactiveFX != null)
		{
			inactiveFX.SetActive(model.state == State.INACTIVE);
		}
		if (activeFX != null)
		{
			activeFX.SetActive(model.state == State.ACTIVE);
		}
		if (cooldownFX != null)
		{
			cooldownFX.SetActive(model.state == State.COOLDOWN);
		}
		if (onStateChanged != null)
		{
			onStateChanged();
		}
	}

	private void OnPlayerDeath(PlayerDeathHandler.DeathType deathType)
	{
		if (model.state == State.ACTIVE || model.state == State.COUNTDOWN)
		{
			SetState(State.COOLDOWN, enableSFX: false);
		}
	}

	public double? GetTimeRemaining()
	{
		if (model.timer.HasValue)
		{
			return model.timer.Value - timeDirector.WorldTime();
		}
		return null;
	}

	public double? GetMaxTimeRemaining()
	{
		switch (model.state)
		{
		case State.COUNTDOWN:
			return countdownMinutes * 60f;
		case State.ACTIVE:
			return activeHours * 3600f;
		case State.COOLDOWN:
			return cooldownHours * 3600f;
		default:
			return null;
		}
	}

	public double? GetWarningTimeSeconds()
	{
		return null;
	}

	protected override string IdPrefix()
	{
		return "qseg";
	}
}
