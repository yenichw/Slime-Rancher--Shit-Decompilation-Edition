using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveDirector : SRBehaviour
{
	private class LoadNewGameMetadata
	{
		public string displayName;

		public PlayerState.GameMode gameMode;

		public Identifiable.Id gameIconId;
	}

	public GameObject saveErrorPrefab;

	public LoadErrorUI loadFileErrorPrefab;

	public DLCPurgedExceptionUI prefabDLCPurgedExceptionUI;

	private LoadingUI loadingUI;

	private float nextSaveTime;

	private bool loadedGame;

	private bool loadingGame;

	private LoadNewGameMetadata newGameMetadata;

	private float lastSaveTime;

	private const float SAVE_PERIOD = 1440f;

	private const int MAX_AUTOSAVES = 5;

	private string currentGameId;

	private static bool firstLoad = true;

	public SavedGame SavedGame { get; private set; }

	public SavedProfile ProfileManager { get; private set; } = new SavedProfile();


	public StorageProvider StorageProvider { get; private set; }

	public float GetLastSaveTime()
	{
		return lastSaveTime;
	}

	public void Awake()
	{
		SavedGame = new SavedGame(new ScenePrefabInstantiator(SRSingleton<GameContext>.Instance.LookupDirector), new SceneSavedGameInfoProvider());
		nextSaveTime = Time.time + 1440f;
		base.enabled = true;
		Initialize();
	}

	public void Start()
	{
	}

	private void Initialize()
	{
		StorageProvider = new FileStorageProvider();
		StorageProvider.Initialize();
	}

	public void OnSceneLoaded()
	{
		SetupDynamicObjects();
		if (base.gameObject == SRSingleton<GameContext>.Instance.gameObject)
		{
			LoadProfile();
		}
	}

	private void SetupDynamicObjects()
	{
		if (IsNewGame())
		{
			SRSingleton<DynamicObjectContainer>.Instance.RegisterDynamicObjectActors();
		}
		else
		{
			SRSingleton<DynamicObjectContainer>.Instance.DestroyDynamicObjectActors();
		}
	}

	public void Update()
	{
		if (!Levels.isSpecialNonAlloc() && Time.time >= nextSaveTime)
		{
			SaveAllNow();
		}
	}

	public bool SaveAllNow()
	{
		Log.Warning("Saving game and profile...");
		nextSaveTime = Time.time + 1440f;
		try
		{
			SaveGame();
			SaveProfile(forceFlush: false);
			StorageProvider.Flush();
		}
		catch (Exception e)
		{
			ErrorSaveFailure(e);
			return false;
		}
		lastSaveTime = Time.time;
		return true;
	}

	public GameData.Summary LoadSummary(string saveName)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			StorageProvider.GetGameData(saveName, memoryStream);
			if (memoryStream.Length == 0L)
			{
				Log.Warning("Datastream was empty when loading save.", "saveName", saveName);
				return new GameData.Summary(saveName);
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return SavedGame.LoadSummary(saveName, memoryStream);
		}
	}

	private void ErrorSaveFailure(Exception e)
	{
		Log.Error("Error while saving.", "Exception", e.Message, "Stack Trace", e.StackTrace);
		UnityEngine.Object.Instantiate(saveErrorPrefab).GetComponent<SaveErrorUI>().SetException(e, SavedGame.GetName());
	}

	public void OnApplicationQuit()
	{
		if (!Levels.isSpecial())
		{
			SaveGame();
			SaveProfile(forceFlush: false);
			StorageProvider.Flush();
		}
	}

	public void LoadNewGame(string displayName, Identifiable.Id gameIconId, PlayerState.GameMode gameMode, Action onError)
	{
		StartCoroutine(LoadNewGame_Coroutine(new LoadNewGameMetadata
		{
			displayName = displayName,
			gameMode = gameMode,
			gameIconId = gameIconId
		}, onError));
	}

	private IEnumerator LoadNewGame_Coroutine(LoadNewGameMetadata metadata, Action onError)
	{
		loadingGame = true;
		loadedGame = false;
		newGameMetadata = metadata;
		yield return OpenLoadingUI();
		yield return SRSingleton<GameContext>.Instance.DLCDirector.RefreshPackagesAsync();
		try
		{
			currentGameId = null;
			string gameSaveFileName = GetGameSaveFileName(metadata.displayName);
			string arg = "";
			int num = 1;
			while (StorageProvider.HasGameData($"{gameSaveFileName}{arg}"))
			{
				arg = $"_{num}";
				num++;
			}
			SavedGame.CreateNew($"{gameSaveFileName}{arg}", metadata.displayName);
			SceneContext.onNextSceneAwake = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onNextSceneAwake, new SceneContext.SceneLoadDelegate(OnNextSceneAwake_NewGame));
			SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnNewGameLoaded));
			BeginSceneSwitch(onError);
		}
		catch (Exception ex)
		{
			Log.Error("Error while creating a new save file.", "Exception", ex.Message, "Stack Trace", ex.StackTrace);
			LoadErrorUI.OpenLoadErrorUI(loadFileErrorPrefab, MessageUtil.Tcompose("e.pushfile_error", SavedGame.GetName()), showContactSupport: true, "e.ok_button", delegate
			{
				RevertToMainMenu(onError);
			});
			loadingUI.OnLoadingError();
		}
	}

	private string GetGameSaveFileName(string displayName)
	{
		string text = Regex.Replace(displayName, "[^A-Za-z0-9]", "");
		text = text.Substring(0, Mathf.Min(25, text.Length));
		return $"{DateTime.Now:yyyyMMddHHmmss}_{text}";
	}

	public Dictionary<string, List<GameData.Summary>> AvailableGamesByDisplayName()
	{
		return AvailableGames((GameData.Summary summary) => summary.displayName);
	}

	public Dictionary<string, List<GameData.Summary>> AvailableGamesByGameName()
	{
		return AvailableGames((GameData.Summary summary) => summary.name);
	}

	private Dictionary<string, List<GameData.Summary>> AvailableGames(Func<GameData.Summary, string> keyFunc)
	{
		List<string> availableGames = StorageProvider.GetAvailableGames();
		Dictionary<string, List<GameData.Summary>> dictionary = new Dictionary<string, List<GameData.Summary>>();
		foreach (string item in availableGames)
		{
			try
			{
				GameData.Summary summary = LoadSummary(item);
				string key = keyFunc(summary);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new List<GameData.Summary>();
					dictionary.Add(key, value);
				}
				value.Add(summary);
			}
			catch (Exception ex)
			{
				Log.Error("Failed to load summary for saved game.", "name", item, "Exception", ex.ToString(), "Exception Stack Trace", ex.StackTrace);
			}
		}
		foreach (KeyValuePair<string, List<GameData.Summary>> item2 in dictionary)
		{
			item2.Value.Sort(CompareSummaryBySaveOrder);
		}
		return dictionary;
	}

	private int CompareSummaryBySaveOrder(GameData.Summary s1, GameData.Summary s2)
	{
		int num = s2.saveNumber.CompareTo(s1.saveNumber);
		if (num == 0)
		{
			num = s2.saveTimestamp.CompareTo(s1.saveTimestamp);
		}
		return num;
	}

	public bool DisplayNameAvailable(string displayName)
	{
		foreach (string availableGame in StorageProvider.GetAvailableGames())
		{
			Log.Debug(availableGame);
			GameData.Summary summary = LoadSummary(availableGame);
			if (string.Compare(displayName, summary.displayName, ignoreCase: false) == 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool GameExists(string gameName)
	{
		return StorageProvider.HasGameData(gameName);
	}

	public void DeleteGame(string gameName)
	{
		List<GameData.Summary> savesByGameName = GetSavesByGameName(gameName);
		for (int i = 0; i < savesByGameName.Count; i++)
		{
			string saveName = savesByGameName[i].saveName;
			if (StorageProvider.HasGameData(saveName))
			{
				DeleteSave(saveName);
			}
		}
		StorageProvider.Flush();
	}

	private List<GameData.Summary> GetSavesByGameName(string gameName)
	{
		if (!AvailableGamesByGameName().TryGetValue(gameName, out var value))
		{
			return new List<GameData.Summary>();
		}
		return value;
	}

	public void CleanupAutosaves(string gameName)
	{
		List<GameData.Summary> savesByGameName = GetSavesByGameName(gameName);
		List<string> list = new List<string>();
		for (int i = 5; i < savesByGameName.Count; i++)
		{
			string saveName = savesByGameName[i].saveName;
			if (StorageProvider.HasGameData(saveName))
			{
				Log.Warning("Cleaning up autosave file.", "name", saveName);
				list.Add(saveName);
			}
		}
		StorageProvider.DeleteGamesData(list);
	}

	public void DeleteSave(string saveName)
	{
		StorageProvider.DeleteGameData(saveName);
	}

	public bool IsNewGame()
	{
		if (loadedGame)
		{
			return Levels.isSpecial();
		}
		return true;
	}

	public bool HasContinue()
	{
		if (string.IsNullOrEmpty(ProfileManager.ContinueGameName))
		{
			return false;
		}
		GameData.Summary saveToContinue = GetSaveToContinue();
		if (saveToContinue != null)
		{
			return StorageProvider.HasGameData(saveToContinue.saveName);
		}
		return false;
	}

	public GameData.Summary GetSaveToContinue()
	{
		string continueGameName = ProfileManager.ContinueGameName;
		if (string.IsNullOrEmpty(continueGameName))
		{
			return null;
		}
		if (AvailableGamesByGameName().TryGetValue(continueGameName, out var value))
		{
			return value.FirstOrDefault();
		}
		return null;
	}

	public void SaveGameAndFlush()
	{
		SaveGame();
		StorageProvider.Flush();
	}

	private void SaveGame()
	{
		if (loadingGame)
		{
			Log.Warning("Attempted to save game while loading, skipping.");
		}
		else if (!string.IsNullOrEmpty(SavedGame.GetName()))
		{
			SavedGame.Pull(SRSingleton<SceneContext>.Instance.GameModel);
			string text = SavedGame.GetName();
			string displayName = SavedGame.GetDisplayName();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				SavedGame.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				string nextFileName = GetNextFileName(text, 0, "{0}_{1}");
				if (currentGameId == null)
				{
					currentGameId = StorageProvider.GetGameId(nextFileName);
					Log.Warning("setting initial gameid", "currentGameId", currentGameId, "nextFileName", nextFileName);
				}
				StorageProvider.StoreGameData(currentGameId, displayName, nextFileName, memoryStream);
			}
			ProfileManager.ContinueGameName = (SavedGame.GameState.summary.isGameOver ? string.Empty : text);
			CleanupAutosaves(text);
		}
		else
		{
			Log.Warning("Save game name was null or empty. Skipping save.");
		}
	}

	private string GetNextFileName(string filename, int startingNumber, string format)
	{
		string result;
		do
		{
			result = string.Format(format, filename, startingNumber++);
		}
		while (StorageProvider.HasGameData(result));
		return result;
	}

	private void LoadProfile()
	{
		Log.Debug("Storage provider initialized. Loading profile.", (StorageProvider == null).ToString());
		LoadFromStream(StorageProvider.GetProfileData, ProfileManager.LoadProfile, delegate
		{
			Log.Debug("No profile was found.");
			ProfileManager = new SavedProfile();
		});
		LoadSettings();
		ProfileManager.Push();
		if (!firstLoad)
		{
			return;
		}
		firstLoad = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs != null)
		{
			string[] array = commandLineArgs;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains("lowGraphics"))
				{
					Log.Debug("Forcing Lowest Quality Graphics");
					SRQualitySettings.ForceLowQuality();
					SaveProfile(forceFlush: false);
				}
			}
		}
		ProfileManager.Profile.RunUpgradeActions(this);
	}

	private void LoadSettings()
	{
		if (StorageProvider.HasSettings())
		{
			LoadFromStream(StorageProvider.GetSettingsData, ProfileManager.LoadSettings, delegate
			{
				Log.Debug("No settings were found.");
			});
		}
		else if (StorageProvider.HasProfile())
		{
			LoadFromStream(StorageProvider.GetProfileData, ProfileManager.LoadLegacySettings, delegate
			{
				Log.Debug("No profile data was found to load legacy settings.");
			});
		}
	}

	public bool SaveProfile()
	{
		return SaveProfile(forceFlush: true);
	}

	private bool SaveProfile(bool forceFlush)
	{
		ProfileManager.Pull();
		if (StorageProvider.IsInitialized())
		{
			SaveStream(ProfileManager.SaveProfile, StorageProvider.StoreProfileData);
			SaveStream(ProfileManager.SaveSettings, StorageProvider.StoreSettingsData);
			if (forceFlush)
			{
				StorageProvider.Flush();
			}
			return true;
		}
		Log.Warning("Storage provider not initialized. Skipping profile and settings save.");
		return false;
	}

	private void LoadFromStream(Action<MemoryStream> openStream, Action<MemoryStream> load, Action onErr)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			openStream(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			if (memoryStream.Length > 0)
			{
				load(memoryStream);
			}
			else
			{
				onErr();
			}
		}
	}

	private void SaveStream(Action<Stream> saveToStream, Action<MemoryStream> storeStream)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			saveToStream(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			storeStream(memoryStream);
		}
	}

	public void ResetProfile()
	{
		Log.Info("Resetting profile.");
		SRQualitySettings.ResetProfile();
		SRSingleton<GameContext>.Instance.OptionsDirector.ResetProfile();
		SRSingleton<GameContext>.Instance.InputDirector.ResetProfile();
		SRSingleton<SceneContext>.Instance.AchievementsDirector.ResetProfile();
		SRSingleton<GameContext>.Instance.MessageDirector.SetCulture(SRSingleton<GameContext>.Instance.MessageDirector.defaultLang);
		ProfileManager.ContinueGameName = "";
		SaveProfile(forceFlush: false);
	}

	public bool IsLoadingGame()
	{
		return loadingGame;
	}

	public void BeginLoad(string gameName, string saveName, Action onError)
	{
		if (!loadingGame)
		{
			LoadSave(gameName, saveName, promptDLCPurgedException: true, onError);
		}
	}

	private void OnNextSceneAwake_NewGame(SceneContext sceneContext)
	{
		sceneContext.GameModel.expectingPush = false;
		sceneContext.GameModeConfig.initGameMode = newGameMetadata.gameMode;
		sceneContext.GameModel.gameIconId = newGameMetadata.gameIconId;
		newGameMetadata = null;
	}

	private void OnNextSceneAwake_ExistingGame(SceneContext sceneContext)
	{
		sceneContext.GameModel.expectingPush = true;
	}

	private void LoadSave(string gameName, string saveName, bool promptDLCPurgedException, Action onError)
	{
		StartCoroutine(LoadSave_Coroutine(gameName, saveName, promptDLCPurgedException, onError));
	}

	private IEnumerator LoadSave_Coroutine(string gameName, string saveName, bool promptDLCPurgedException, Action onError)
	{
		loadingGame = true;
		loadedGame = true;
		yield return OpenLoadingUI();
		yield return SRSingleton<GameContext>.Instance.DLCDirector.RefreshPackagesAsync();
		try
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				StorageProvider.GetGameData(saveName, memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				SavedGame.Load(memoryStream);
				currentGameId = StorageProvider.GetGameId(saveName);
			}
			try
			{
				SRSingleton<GameContext>.Instance.DLCDirector.Purge(SavedGame.GameState);
			}
			catch (DLCPurgedException exception)
			{
				if (promptDLCPurgedException)
				{
					DLCPurgedExceptionUI.OnExceptionCaught(prefabDLCPurgedExceptionUI, exception, delegate
					{
						LoadSave(gameName, saveName, promptDLCPurgedException: false, onError);
					}, delegate
					{
						RevertToMainMenu(onError);
					});
					loadingUI.OnLoadingError();
					yield break;
				}
			}
			SceneContext.onNextSceneAwake = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onNextSceneAwake, new SceneContext.SceneLoadDelegate(OnNextSceneAwake_ExistingGame));
			BeginSceneSwitch(onError);
		}
		catch (Exception ex)
		{
			Log.Error("Error while loading a save file.", "save", saveName, "Exception", ex.Message, "Stack Trace", ex.StackTrace);
			LoadErrorUI.OpenLoadErrorUI(loadFileErrorPrefab, "e.file_load_failed", showContactSupport: false, "e.yes_button", delegate
			{
				LoadFallbackSave(gameName, saveName, promptDLCPurgedException: true, onError);
			}, "e.no_button", delegate
			{
				RevertToMainMenu(onError);
			});
			loadingUI.OnLoadingError();
		}
	}

	private void LoadFallbackSave(string gameName, string saveName, bool promptDLCPurgedException, Action onError)
	{
		StartCoroutine(LoadFallbackSave_Coroutine(gameName, saveName, promptDLCPurgedException, onError));
	}

	private IEnumerator LoadFallbackSave_Coroutine(string gameName, string saveName, bool promptDLCPurgedException, Action onError)
	{
		IEnumerable<GameData.Summary> summaries = GetSavesByGameName(gameName).SkipWhile((GameData.Summary s) => saveName.CompareTo(s.saveName) != 0).Skip(1);
		yield return OpenLoadingUI();
		int count = 0;
		foreach (GameData.Summary summary in summaries)
		{
			count++;
			yield return new WaitForSeconds(0.1f);
			try
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					StorageProvider.GetGameData(summary.saveName, memoryStream);
					memoryStream.Seek(0L, SeekOrigin.Begin);
					SavedGame.Load(memoryStream);
					currentGameId = StorageProvider.GetGameId(summary.saveName);
				}
				try
				{
					SRSingleton<GameContext>.Instance.DLCDirector.Purge(SavedGame.GameState);
				}
				catch (DLCPurgedException exception)
				{
					if (promptDLCPurgedException)
					{
						DLCPurgedExceptionUI.OnExceptionCaught(prefabDLCPurgedExceptionUI, exception, delegate
						{
							LoadFallbackSave(gameName, saveName, promptDLCPurgedException: false, onError);
						}, delegate
						{
							RevertToMainMenu(onError);
						});
						loadingUI.OnLoadingError();
						yield break;
					}
				}
				SceneContext.onNextSceneAwake = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onNextSceneAwake, new SceneContext.SceneLoadDelegate(OnNextSceneAwake_ExistingGame));
				BeginSceneSwitch(onError);
				yield break;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to fallback to prior save.", "save", summary.saveName, "Exception", ex.Message, "Stack Trace", ex.StackTrace);
			}
		}
		Log.Error($"Failed all fallback attempts. Attempted to load {count} files.");
		LoadErrorUI.OpenLoadErrorUI(loadFileErrorPrefab, "e.fallback_failed", showContactSupport: true, "e.ok_button", delegate
		{
			RevertToMainMenu(onError);
		});
		loadingUI.OnLoadingError();
	}

	private void BeginSceneSwitch(Action onErr)
	{
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnGameLoaded));
		SceneManager.LoadSceneAsync("worldGenerated", LoadSceneMode.Single);
	}

	private void OnNewGameLoaded(SceneContext sceneContext)
	{
		sceneContext.GameModel.OnNewGameLoaded();
	}

	private void OnGameLoaded(SceneContext ctx)
	{
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnGameLoaded));
		StartCoroutine(OnGameLoadedCoroutine(ctx));
	}

	private IEnumerator OnGameLoadedCoroutine(SceneContext ctx)
	{
		if (ctx.GameModel.expectingPush)
		{
			Exception ex = null;
			try
			{
				SavedGame.Push(ctx.GameModel);
			}
			catch (Exception ex2)
			{
				Log.Error("Error while populating scene from save game.", "save", SavedGame.GetName(), "Exception", ex2.Message, "Stack Trace", ex2.StackTrace);
				ex = ex2;
				LoadErrorUI.OpenLoadErrorUI(loadFileErrorPrefab, MessageUtil.Tcompose("e.pushfile_error", SavedGame.GetName()), showContactSupport: true, "e.ok_button", delegate
				{
					RevertToMainMenu(delegate
					{
						Log.Debug("Falling back to main menu from worldGenerated.");
					});
				});
				loadingUI.OnLoadingError();
			}
			finally
			{
				ctx.GameModel.expectingPush = false;
			}
			if (ex != null)
			{
				yield break;
			}
		}
		ctx.TutorialDirector.SuppressTutorials();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		CloseLoadingUI();
		if (IsNewGame() && ctx.GameModeConfig.GetModeSettings().newGamePrefab != null)
		{
			Destroyer.Monitor(UnityEngine.Object.Instantiate(ctx.GameModeConfig.GetModeSettings().newGamePrefab), delegate
			{
				ctx.TutorialDirector.UnsuppressTutorials();
			});
		}
		else
		{
			ctx.TutorialDirector.UnsuppressTutorials();
		}
		ctx.NoteGameFullyLoaded();
		loadingGame = false;
	}

	private void RevertToMainMenu(Action onError)
	{
		SceneContext.onNextSceneAwake = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onNextSceneAwake, new SceneContext.SceneLoadDelegate(OnNextSceneAwake_NewGame));
		SceneContext.onNextSceneAwake = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onNextSceneAwake, new SceneContext.SceneLoadDelegate(OnNextSceneAwake_ExistingGame));
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnNewGameLoaded));
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnGameLoaded));
		loadingUI.isReturningToMenu = true;
		if (Levels.isMainMenu())
		{
			RevertToMainMenu_OnRevertComplete();
			onError();
		}
		else
		{
			SceneManager.sceneLoaded += RevertToMainMenu_OnSceneLoaded;
			SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
		}
	}

	private void RevertToMainMenu_OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= RevertToMainMenu_OnSceneLoaded;
		RevertToMainMenu_OnRevertComplete();
	}

	private void RevertToMainMenu_OnRevertComplete()
	{
		loadingGame = false;
		CloseLoadingUI();
	}

	private IEnumerator OpenLoadingUI()
	{
		if (loadingUI == null)
		{
			GameObject target = UnityEngine.Object.Instantiate(SRSingleton<GameContext>.Instance.UITemplates.loadingUI);
			loadingUI = target.GetRequiredComponent<LoadingUI>();
			UnityEngine.Object.DontDestroyOnLoad(target);
		}
		loadingUI.OnLoadingStart();
		yield return new WaitForEndOfFrame();
	}

	private void CloseLoadingUI()
	{
		if (loadingUI != null)
		{
			Destroyer.Destroy(loadingUI.gameObject, "AutoSaveDirector.CloseLoadingUI");
			loadingUI = null;
		}
	}
}
