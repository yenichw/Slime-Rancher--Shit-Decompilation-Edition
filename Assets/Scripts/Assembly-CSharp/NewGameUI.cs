using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewGameUI : BaseUI
{
	public GameObject mainMenuUIPrefab;

	public InputField gameNameField;

	public Button playButton;

	public Toggle classicToggle;

	public Toggle casualToggle;

	public Toggle timeLimitToggle;

	public ToggleGroup iconGroup;

	public TMP_Text gameModeText;

	public GameObject gameIconPrefab;

	public Button leftIconButton;

	public Button rightIconButton;

	public Identifiable.Id[] availIconIds;

	[Tooltip("TabByMenuKeys attached to the icon selection scrollview.")]
	public TabByMenuKeys iconTabByMenuKeys;

	private PlayerState.GameMode selGameMode;

	private int selIconIdIdx;

	private Toggle[] iconToggles;

	private bool settingToggleStates;

	private const string ERR_EXISTS = "e.game_name_exists";

	private const string ERR_LETTERS_NUMS_ONLY = "e.letters_nums_only";

	private const string ERR_MAX_LENGTH = "e.max_length";

	private const int GAME_NAME_MAX_LENGTH = 24;

	private AutoSaveDirector autoSaveDirector;

	private GameObject waitForErrorDialog;

	public override void Awake()
	{
		base.Awake();
		autoSaveDirector = SRSingleton<GameContext>.Instance.AutoSaveDirector;
	}

	public void Start()
	{
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		HashSet<string> availableGameDisplayNames = GetAvailableGameDisplayNames();
		for (int i = 1; i < 1000; i++)
		{
			string text = bundle.Get("m.default_game_name", i);
			if (!availableGameDisplayNames.Contains(text))
			{
				gameNameField.text = text;
				break;
			}
		}
		classicToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				SetGameMode(PlayerState.GameMode.CLASSIC);
			}
		});
		casualToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				SetGameMode(PlayerState.GameMode.CASUAL);
			}
		});
		timeLimitToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				SetGameMode(PlayerState.GameMode.TIME_LIMIT_V2);
			}
		});
		SetGameMode(PlayerState.GameMode.CLASSIC);
		iconToggles = new Toggle[availIconIds.Length];
		bool flag = true;
		LookupDirector lookupDirector = SRSingleton<GameContext>.Instance.LookupDirector;
		for (int j = 0; j < availIconIds.Length; j++)
		{
			Identifiable.Id id = availIconIds[j];
			GameObject gameObject = UnityEngine.Object.Instantiate(gameIconPrefab);
			gameObject.transform.SetParent(iconGroup.transform, worldPositionStays: false);
			Toggle toggle = gameObject.GetComponent<Toggle>();
			gameObject.transform.Find("GameIcon").GetComponent<Image>().sprite = lookupDirector.GetIcon(id);
			toggle.group = iconGroup;
			iconToggles[j] = toggle;
			int idxToSet = j;
			toggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn && !settingToggleStates)
				{
					SetIconIdIdx(idxToSet);
				}
			});
			OnSelectDelegator.Create(toggle.gameObject, delegate
			{
				toggle.isOn = true;
			});
			if (flag)
			{
				flag = false;
				toggle.isOn = true;
			}
		}
	}

	private HashSet<string> GetAvailableGameDisplayNames()
	{
		return new HashSet<string>(autoSaveDirector.AvailableGamesByDisplayName().Keys);
	}

	public void PlayNewGame()
	{
		string text = gameNameField.text;
		if (!autoSaveDirector.DisplayNameAvailable(text))
		{
			waitForErrorDialog = SRSingleton<GameContext>.Instance.UITemplates.CreateErrorDialog("e.game_name_exists");
			return;
		}
		if (text.Length > 24 || text.Length < 1)
		{
			waitForErrorDialog = SRSingleton<GameContext>.Instance.UITemplates.CreateErrorDialogWithArgs("e.max_length", 24);
			return;
		}
		playButton.interactable = false;
		base.gameObject.SetActive(value: false);
		SRSingleton<GameContext>.Instance.AutoSaveDirector.LoadNewGame(text, GetGameIconId(), GetGameMode(), delegate
		{
			playButton.interactable = true;
			base.gameObject.SetActive(value: true);
		});
	}

	protected override bool Closeable()
	{
		return waitForErrorDialog == null;
	}

	private PlayerState.GameMode GetGameMode()
	{
		return selGameMode;
	}

	private void SetGameMode(PlayerState.GameMode mode)
	{
		selGameMode = mode;
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		if (mode == PlayerState.GameMode.TIME_LIMIT_V2)
		{
			int? stat = SRSingleton<SceneContext>.Instance.AchievementsDirector.GetStat(AchievementsDirector.IntStat.TIME_LIMIT_V2_CURRENCY);
			string key = string.Format("m.desc.gamemode_{0}{1}", mode.ToString().ToLowerInvariant(), (!stat.HasValue) ? string.Empty : "_high_score");
			gameModeText.text = bundle.Get(key, stat);
		}
		else
		{
			string key2 = $"m.desc.gamemode_{mode.ToString().ToLowerInvariant()}";
			gameModeText.text = bundle.Get(key2);
		}
	}

	private void SetIconIdIdx(int idx)
	{
		selIconIdIdx = idx;
		try
		{
			settingToggleStates = true;
			iconToggles[idx].isOn = true;
			leftIconButton.interactable = idx > 0;
			rightIconButton.interactable = idx < iconToggles.Length - 1;
			iconTabByMenuKeys.RecalcSelected();
		}
		finally
		{
			settingToggleStates = false;
		}
	}

	private Identifiable.Id GetGameIconId()
	{
		return availIconIds[selIconIdIdx];
	}

	public override void Close()
	{
		base.Close();
	}

	public void SelectNextIcon()
	{
		SetIconIdIdx(Math.Min(availIconIds.Length - 1, selIconIdIdx + 1));
	}

	public void SelectPrevIcon()
	{
		SetIconIdIdx(Math.Max(0, selIconIdIdx - 1));
	}

	public override void Update()
	{
		base.Update();
		iconTabByMenuKeys.enabled = !gameNameField.isFocused || InputDirector.UsingGamepad();
	}
}
