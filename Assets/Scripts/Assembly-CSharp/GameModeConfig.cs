using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GameModeConfig : MonoBehaviour, GameModel.Participant
{
	public GameModeSettings classicSettings;

	public GameModeSettings casualSettings;

	public GameModeSettings timeLimitSettings;

	public GameModeSettings timeLimitV2Settings;

	public PlayerState.GameMode initGameMode;

	private Dictionary<PlayerState.GameMode, GameModeSettings> modeSettings = new Dictionary<PlayerState.GameMode, GameModeSettings>();

	private GameModel gameModel;

	public void Awake()
	{
		modeSettings[PlayerState.GameMode.CLASSIC] = classicSettings;
		modeSettings[PlayerState.GameMode.CASUAL] = casualSettings;
		modeSettings[PlayerState.GameMode.TIME_LIMIT] = timeLimitSettings;
		modeSettings[PlayerState.GameMode.TIME_LIMIT_V2] = timeLimitV2Settings;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGameModelParticipant(this);
	}

	public void InitForLevel()
	{
	}

	public void InitModel(GameModel model)
	{
		model.currGameMode = initGameMode;
	}

	public void SetModel(GameModel model)
	{
		gameModel = model;
		gameModel.ResetPlayerForGameMode(GetModeSettings());
	}

	public GameModeSettings GetModeSettings()
	{
		return modeSettings[gameModel.currGameMode];
	}
}
