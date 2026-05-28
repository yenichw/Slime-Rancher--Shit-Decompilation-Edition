public class RushModeDirector : SRBehaviour
{
	private class TimeSourceWrapper : VacDisplayTimer.TimeSource
	{
		private PlayerState state;

		public TimeSourceWrapper(PlayerState state)
		{
			this.state = state;
		}

		public double? GetTimeRemaining()
		{
			return state.GetEndGameTimeRemaining();
		}

		public double? GetMaxTimeRemaining()
		{
			double num = 32400.0;
			return state.GetEndGameTime().Value - num;
		}

		public double? GetWarningTimeSeconds()
		{
			return 3600.0;
		}
	}

	private const float WARNING_THRESHOLD_1 = 10800f;

	private const float WARNING_THRESHOLD_2 = 3600f;

	private MusicDirector musicDirector;

	private TimeDirector timeDirector;

	private PlayerState playerState;

	public void Awake()
	{
		musicDirector = SRSingleton<GameContext>.Instance.MusicDirector;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void Start()
	{
		if (SRSingleton<SceneContext>.Instance.GameModel.currGameMode != PlayerState.GameMode.TIME_LIMIT_V2)
		{
			base.enabled = false;
			return;
		}
		WeaponVacuum componentInChildren = SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>();
		componentInChildren.GetComponentInChildren<VacDisplayChanger>(includeInactive: true).SetDisplayMode(PlayerState.AmmoMode.NIMBLE_VALLEY);
		componentInChildren.GetComponentInChildren<VacDisplayTimer>(includeInactive: true).SetTimeSource(new TimeSourceWrapper(playerState));
		playerState.onEndGameTimeChanged += OnEndGameTimeChanged;
		OnEndGameTimeChanged();
	}

	private void OnEndGameTimeChanged()
	{
		timeDirector.RemovePassedTimeDelegate(SetWarningMode_0);
		timeDirector.RemovePassedTimeDelegate(SetWarningMode_1);
		timeDirector.RemovePassedTimeDelegate(SetWarningMode_2);
		SetWarningMode_0();
		double value = playerState.GetEndGameTime().Value;
		timeDirector.OnPassedTime(value - 10800.0, SetWarningMode_1);
		timeDirector.OnPassedTime(value - 3600.0, SetWarningMode_2);
		timeDirector.OnPassedTime(value, SetWarningMode_0);
	}

	private void SetWarningMode_2()
	{
		musicDirector.SetRushWarningMode(musicDirector.rushModeWarning2Music);
	}

	private void SetWarningMode_1()
	{
		musicDirector.SetRushWarningMode(musicDirector.rushModeWarning1Music);
	}

	private void SetWarningMode_0()
	{
		musicDirector.SetRushWarningMode(null);
	}
}
