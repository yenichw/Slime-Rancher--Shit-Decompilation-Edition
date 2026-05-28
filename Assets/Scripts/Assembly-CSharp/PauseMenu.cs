using System;
using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : SRSingleton<PauseMenu>
{
	public GameObject pauseUI;

	public GameObject optionsUI;

	public GameObject bugReportUI;

	public Button resumeButton;

	public Button pediaButton;

	public Button achievementsButton;

	public Button optionsButton;

	public Button bugReportButton;

	public Button screenshotButton;

	public Button emergencyResetButton;

	public Button quitButton;

	public Toggle invertViewYAxisToggle;

	public GameObject invertViewYAxisPanel;

	private TimeDirector timeDir;

	private bool suppressUnpause;

	private SRInput.InputMode? previousInputMode;

	public override void Awake()
	{
		base.Awake();
		bugReportButton.gameObject.SetActive(value: true);
		screenshotButton.gameObject.SetActive(value: true);
		achievementsButton.gameObject.SetActive(value: true);
		XlateText[] componentsInChildren = quitButton.GetComponentsInChildren<XlateText>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			componentsInChildren[0].key = "b.save_and_quit";
		}
		invertViewYAxisPanel.SetActive(value: false);
		InputManager.OnDeviceDetached += PauseOnDeviceDetach;
	}

	private void Start()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		pauseUI.SetActive(value: false);
	}

	public void Update()
	{
		if ((SRInput.Actions.menu.WasPressed || SRInput.PauseActions.unmenu.WasPressed) && !timeDir.IsFastForwarding())
		{
			if (pauseUI.activeSelf)
			{
				if (timeDir.ExactlyOnePauser() && !suppressUnpause)
				{
					UnPauseGame();
				}
			}
			else if (Time.timeScale > 0f)
			{
				PauseGame();
			}
		}
		else if (SRInput.PauseActions.cancel.WasPressed && !suppressUnpause && pauseUI.activeSelf && timeDir.ExactlyOnePauser())
		{
			UnPauseGame();
		}
	}

	public bool IsOnlyPauser()
	{
		if (pauseUI.activeSelf)
		{
			return timeDir.ExactlyOnePauser();
		}
		return false;
	}

	public void PauseGame()
	{
		SRInput.Instance.SetInputMode(SRInput.InputMode.PAUSE, base.gameObject.GetInstanceID());
		pauseUI.SetActive(value: true);
	}

	public void UnPauseGame()
	{
		pauseUI.SetActive(value: false);
		SRInput.Instance.ClearInputMode(base.gameObject.GetInstanceID());
	}

	public void Resume()
	{
		UnPauseGame();
	}

	public void Pedia()
	{
		InstantiateAndWait(SRSingleton<SceneContext>.Instance.PediaDirector.pediaPanelPrefab);
	}

	public void Achievements()
	{
		InstantiateAndWait(SRSingleton<SceneContext>.Instance.AchievementsDirector.achievementsPanelPrefab);
	}

	public void EmergencyReturn()
	{
		WaitFor(SRSingleton<GameContext>.Instance.UITemplates.CreateConfirmDialog("m.emergency_return", delegate
		{
			DeathHandler.Kill(SRSingleton<SceneContext>.Instance.Player, DeathHandler.Source.EMERGENCY_RETURN, null, "PauseGame.EmergencyReturn");
			UnPauseGame();
		}));
	}

	public void Quit()
	{
		if (SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow())
		{
			SRSingleton<SceneContext>.Instance.OnSessionEnded();
			SceneManager.LoadScene("MainMenu");
		}
	}

	public void Options()
	{
		InstantiateAndWait(optionsUI);
	}

	public void ReportIssue()
	{
		InstantiateAndWait(bugReportUI);
	}

	public GameObject InstantiateAndWait(GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		WaitFor(gameObject);
		return gameObject;
	}

	public void WaitFor(GameObject uiObj)
	{
		BaseUI component = uiObj.GetComponent<BaseUI>();
		base.gameObject.SetActive(value: false);
		component.onDestroy = (BaseUI.OnDestroyDelegate)Delegate.Combine(component.onDestroy, (BaseUI.OnDestroyDelegate)delegate
		{
			if (this != null && base.gameObject != null)
			{
				base.gameObject.SetActive(value: true);
			}
		});
	}

	public void Screenshot()
	{
		SRSingleton<GameContext>.Instance.TakeScreenshot();
	}

	public void OnEnable()
	{
		invertViewYAxisToggle.isOn = SRSingleton<GameContext>.Instance.InputDirector.GetInvertGamepadLookY();
	}

	public void OnDisable()
	{
	}

	public void OnToggleYAxis()
	{
		SRSingleton<GameContext>.Instance.InputDirector.SetInvertGamepadLookY(invertViewYAxisToggle.isOn);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SRInput.Instance.ClearInputMode(base.gameObject.GetInstanceID());
		InputManager.OnDeviceDetached -= PauseOnDeviceDetach;
	}

	private void PauseOnDeviceDetach(InputDevice device)
	{
		if (Time.timeScale > 0f && !timeDir.IsFastForwarding() && SRInput.Instance.GetInputMode() == SRInput.InputMode.DEFAULT)
		{
			PauseGame();
		}
	}
}
