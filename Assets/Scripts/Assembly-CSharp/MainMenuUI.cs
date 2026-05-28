using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : SRBehaviour
{
	public GameObject loadGameUI;

	public GameObject expoSelectGameUI;

	public GameObject newGameUI;

	public GameObject optionsUI;

	public GameObject creditsUI;

	[Tooltip("DLCManageUI prefab.")]
	public GameObject manageDLCUI;

	public GameObject expoModePanel;

	public GameObject standardModePanel;

	public Button continueBtn;

	public Button quitBtn;

	public Button loadBtn;

	public TMP_Text statusText;

	public Button expoQuitBtn;

	[Tooltip("DLC button.")]
	public Button DLCButton;

	public GameObject gdkUserPanel;

	public TMP_Dropdown languageDropdown;

	private MessageBundle uiBundle;

	private const string FORUMS_URL = "http://forums.monomipark.com";

	private const string SUPPORT_URL = "https://support.slimerancher.com/hc";

	public void Awake()
	{
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
	}

	public void Start()
	{
		Log.Debug("MainMenuUI.Start");
		DLCButton.gameObject.SetActive(DLCManageUI.IsEnabled());
		if (quitBtn != null)
		{
			quitBtn.gameObject.SetActive(value: true);
			expoQuitBtn.gameObject.SetActive(value: true);
			gdkUserPanel.gameObject.SetActive(value: false);
		}
		else
		{
			Log.Debug("quit button was null");
		}
		standardModePanel.SetActive(value: true);
		expoModePanel.SetActive(value: false);
		MaybeShowContinue();
	}

	public void OnEnable()
	{
		MaybeShowContinue();
	}

	private void MaybeShowContinue()
	{
		if (SRSingleton<GameContext>.Instance.AutoSaveDirector.HasContinue())
		{
			continueBtn.gameObject.SetActive(value: true);
			InitSelected component = loadBtn.gameObject.GetComponent<InitSelected>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			continueBtn.gameObject.AddComponent<InitSelected>();
		}
		else
		{
			continueBtn.gameObject.SetActive(value: false);
			InitSelected component2 = continueBtn.gameObject.GetComponent<InitSelected>();
			if (component2 != null)
			{
				UnityEngine.Object.Destroy(component2);
			}
			loadBtn.gameObject.AddComponent<InitSelected>();
		}
	}

	public virtual void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	public virtual void OnBundlesAvailable(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
		SetupLanguages();
	}

	public void OnButtonDLC()
	{
		InstantiateAndWaitForDestroy(manageDLCUI);
	}

	public void ContinueGame()
	{
		SetInteractable(interactable: false);
		GameData.Summary saveToContinue = SRSingleton<GameContext>.Instance.AutoSaveDirector.GetSaveToContinue();
		SRSingleton<GameContext>.Instance.AutoSaveDirector.BeginLoad(saveToContinue.name, saveToContinue.saveName, delegate
		{
			SetInteractable(interactable: true);
			base.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		});
	}

	public void LoadGame()
	{
		InstantiateAndWaitForDestroy(loadGameUI);
	}

	public void SelectGame()
	{
		UnityEngine.Object.Instantiate(expoSelectGameUI);
		Destroyer.Destroy(base.gameObject, "MainMenuUI.SelectGame");
	}

	public void NewGame()
	{
		InstantiateAndWaitForDestroy(newGameUI);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void Options()
	{
		InstantiateAndWaitForDestroy(optionsUI);
	}

	public void Credits()
	{
		InstantiateAndWaitForDestroy(creditsUI);
	}

	public void Forums()
	{
		Application.OpenURL("http://forums.monomipark.com");
	}

	public void SupportEmail()
	{
		Application.OpenURL("https://support.slimerancher.com/hc");
	}

	public GameObject InstantiateAndWaitForDestroy(GameObject prefab)
	{
		GameObject obj = UnityEngine.Object.Instantiate(prefab);
		BaseUI component = obj.GetComponent<BaseUI>();
		base.gameObject.SetActive(value: false);
		component.onDestroy = (BaseUI.OnDestroyDelegate)Delegate.Combine(component.onDestroy, (BaseUI.OnDestroyDelegate)delegate
		{
			if (this != null && base.gameObject != null)
			{
				base.gameObject.SetActive(value: true);
			}
		});
		return obj;
	}

	private void SetInteractable(bool interactable)
	{
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].interactable = interactable;
		}
	}

	private void SetupLanguages()
	{
		SetupDropdown(languageDropdown, "l.lang_", delegate(MessageDirector.Lang lang)
		{
			CultureInfo culture = SRSingleton<GameContext>.Instance.MessageDirector.GetCulture();
			return Enum.GetName(typeof(MessageDirector.Lang), lang).ToLowerInvariant() == culture.TwoLetterISOLanguageName;
		}, delegate(MessageDirector.Lang lang)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.SetCulture(lang);
			SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveProfile();
		});
	}

	private void SetupDropdown<T>(TMP_Dropdown dropdown, string msgPrefix, Predicate<T> isLevel, UnityAction<T> assignLevel)
	{
		int num = 0;
		dropdown.options.Clear();
		dropdown.onValueChanged.RemoveAllListeners();
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			string text = Enum.GetName(typeof(T), value);
			string text2 = uiBundle.Xlate(msgPrefix + text.ToLowerInvariant());
			dropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			T fLevel = value;
			int fIdx = num;
			if (isLevel(fLevel))
			{
				dropdown.value = fIdx;
				dropdown.captionText.text = text2;
			}
			dropdown.onValueChanged.AddListener(delegate(int val)
			{
				if (val == fIdx)
				{
					assignLevel(fLevel);
				}
			});
			num++;
		}
		UnityAction<int> call = delegate
		{
			Transform transform = dropdown.transform.Find("Dropdown List");
			if (transform != null)
			{
				Destroyer.Destroy(transform.gameObject, "MainMenuUI.SetupDropdown");
			}
		};
		dropdown.onValueChanged.AddListener(call);
	}
}
