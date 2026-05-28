using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameUI : BaseUI
{
	public GameObject loadGameButtonPrefab;

	public GameObject loadButtonPanel;

	public GameObject mainMenuUIPrefab;

	public GameObject deleteUIPrefab;

	public ScrollRect scroller;

	public TMP_Text status;

	public Button playButton;

	public Button deleteButton;

	public Button backButton;

	public GameObject loadingPanel;

	public GameSummaryPanel summaryPanel;

	public GameObject noSavesPanel;

	private List<Toggle> gameToggles = new List<Toggle>();

	private int selectedIdx;

	private bool settingToggleStates;

	private List<GameData.Summary> availGames = new List<GameData.Summary>();

	private AutoSaveDirector autoSaveDirector;

	public override void Awake()
	{
		base.Awake();
		autoSaveDirector = SRSingleton<GameContext>.Instance.AutoSaveDirector;
		UpdateAvailGames();
	}

	private void UpdateAvailGames()
	{
		foreach (Toggle gameToggle in gameToggles)
		{
			Destroyer.Destroy(gameToggle.gameObject, "LoadGameUI.UpdateAvailGames");
		}
		gameToggles.Clear();
		noSavesPanel.gameObject.SetActive(value: true);
		loadingPanel.SetActive(value: true);
		StartCoroutine(FinishUpdateAvailGames());
	}

	private IEnumerator FinishUpdateAvailGames()
	{
		yield return new WaitForSeconds(0f);
		availGames.Clear();
		foreach (KeyValuePair<string, List<GameData.Summary>> item in autoSaveDirector.AvailableGamesByDisplayName())
		{
			availGames.Add(item.Value[0]);
		}
		loadingPanel.SetActive(value: false);
		summaryPanel.gameObject.SetActive(availGames.Count > 0);
		noSavesPanel.gameObject.SetActive(availGames.Count <= 0);
		foreach (GameData.Summary availGame in availGames)
		{
			GameObject gameObject = CreateLoadGameButton(availGame);
			gameObject.transform.SetParent(loadButtonPanel.transform, worldPositionStays: false);
			gameToggles.Add(gameObject.GetComponent<Toggle>());
		}
		if (gameToggles.Count > 0)
		{
			gameToggles[0].gameObject.AddComponent<InitSelected>();
		}
		for (int i = 0; i < gameToggles.Count; i++)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (i > 0)
			{
				navigation.selectOnUp = gameToggles[i - 1];
			}
			if (i < gameToggles.Count - 1)
			{
				navigation.selectOnDown = gameToggles[i + 1];
			}
			gameToggles[i].navigation = navigation;
			AddToggleListener(i);
		}
		if (availGames.Count > 0)
		{
			SetSelectedIdx(0);
		}
		StartCoroutine(ScrollToTop());
	}

	private void AddToggleListener(int idx)
	{
		gameToggles[idx].onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn && !settingToggleStates)
			{
				SetSelectedIdx(idx);
			}
		});
	}

	private IEnumerator ScrollToTop()
	{
		yield return new WaitForEndOfFrame();
		scroller.verticalNormalizedPosition = 1f;
	}

	public void LoadSelectedGame()
	{
		GameSummaryEntry gameSummaryEntry = SelectedGame();
		if (gameSummaryEntry != null)
		{
			LoadGame(gameSummaryEntry.gameName, gameSummaryEntry.saveName);
		}
	}

	public void DeleteSelectedGame()
	{
		GameSummaryEntry gameSummaryEntry = SelectedGame();
		if (gameSummaryEntry != null)
		{
			DeleteGame(gameSummaryEntry.saveName);
		}
	}

	public override void Close()
	{
		base.Close();
	}

	private void SetSelectedIdx(int idx)
	{
		selectedIdx = idx;
		try
		{
			settingToggleStates = true;
			gameToggles[idx].Select();
			summaryPanel.Init(availGames[idx]);
			playButton.interactable = !availGames[idx].isInvalid && !availGames[idx].gameOver && !autoSaveDirector.IsLoadingGame();
		}
		finally
		{
			settingToggleStates = false;
		}
	}

	public void SelectNextGame()
	{
		SetSelectedIdx(Math.Min(gameToggles.Count - 1, selectedIdx + 1));
	}

	public void SelectPrevGame()
	{
		SetSelectedIdx(Math.Max(0, selectedIdx - 1));
	}

	private void DeleteGame(string saveName)
	{
		GameData.Summary gameToDelete = autoSaveDirector.LoadSummary(saveName);
		base.gameObject.SetActive(value: false);
		CreateDeleteGameDialog(gameToDelete, delegate
		{
			base.gameObject.SetActive(value: true);
			autoSaveDirector.DeleteGame(gameToDelete.name);
		}, delegate
		{
			base.gameObject.SetActive(value: true);
			UpdateAvailGames();
		});
	}

	private GameSummaryEntry SelectedGame()
	{
		ToggleGroup component = loadButtonPanel.GetComponent<ToggleGroup>();
		if (component.GetActive() != null)
		{
			return component.GetActive().GetComponent<GameSummaryEntry>();
		}
		return null;
	}

	private void LoadGame(string gameName, string saveName)
	{
		base.gameObject.SetActive(value: false);
		autoSaveDirector.BeginLoad(gameName, saveName, delegate
		{
			base.gameObject.SetActive(value: true);
		});
	}

	private GameObject CreateLoadGameButton(GameData.Summary gameSummary)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(loadGameButtonPrefab);
		Toggle toggle = gameObject.GetComponent<Toggle>();
		toggle.group = loadButtonPanel.GetComponent<ToggleGroup>();
		gameObject.GetComponent<GameSummaryEntry>().Init(gameSummary);
		OnSelectDelegator.Create(gameObject, delegate
		{
			toggle.isOn = true;
		});
		return gameObject;
	}

	private GameObject CreateDeleteGameDialog(GameData.Summary gameSummary, ConfirmUI.OnConfirm onConfirm, OnDestroyDelegate onDestroy)
	{
		GameObject obj = UnityEngine.Object.Instantiate(deleteUIPrefab);
		obj.GetComponent<ConfirmUI>().onConfirm = onConfirm;
		obj.GetComponent<ConfirmUI>().onDestroy = onDestroy;
		obj.transform.Find("MainPanel/GameSummaryPanel").GetComponent<GameSummaryPanel>().Init(gameSummary);
		return obj;
	}
}
