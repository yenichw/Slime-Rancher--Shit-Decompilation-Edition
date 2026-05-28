using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RanchHouseUI : BaseUI, LocationalUI
{
	private enum FadeMode
	{
		NONE = 0,
		IN_FADE_IN = 1,
		IN_FADE_OUT = 2,
		OUT_FADE_IN = 3,
		OUT_FADE_OUT = 4
	}

	public GameObject mailUI;

	public GameObject partnerUI;

	public GameObject manageDLCPrefab;

	public GameObject appearancePrefab;

	public TMP_Text dayText;

	public TMP_Text timeText;

	public Image timeIcon;

	public Image mailIcon;

	public GameObject mailHighlight;

	public TMP_Text mailHighlightText;

	public GameObject partnerArea;

	public TMP_Text partnerLevelText;

	public GameObject mainUI;

	public Image obscurer;

	public GameObject buttonPanel;

	public GameObject mailButton;

	public Button sleepButton;

	public GameObject partnerButton;

	public GameObject DLCButton;

	public GameObject appearanceButton;

	public Image backgroundImg;

	public Sprite dayBg;

	public Sprite nightBg;

	public Sprite dawnBg;

	public Sprite duskBg;

	public RawImage beatrixImg;

	public SECTR_AudioCue openCue;

	public SECTR_AudioCue closeCue;

	public GameObject beatrixPrefab;

	public bool isClosing;

	private FadeMode fadeMode;

	private TimeDirector timeDir;

	private MailDirector mailDir;

	private ProgressDirector progressDir;

	private MailUI currMailUI;

	private CorporatePartnerUI currPartnerUI;

	private DLCManageUI currDLCManageUI;

	private SlimeAppearanceUI currAppearanceUI;

	private bool sleeping;

	private Material bgMat;

	private Vector3 worldPos;

	private MusicDirector musicDir;

	private float mailInitY;

	private GameObject beatrixObj;

	private CameraDisabler camDisabler;

	private const float OFFSET_SPACING = 50f;

	private const float FADE_RATE = 2f;

	private const float BEATRIX_FADE_TIME = 0.5f;

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		camDisabler = SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<CameraDisabler>();
		mainUI.SetActive(value: false);
		fadeMode = FadeMode.IN_FADE_IN;
		mailButton.SetActive(SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().AllowMail());
		mailInitY = mailIcon.rectTransform.anchoredPosition.y;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		SRSingleton<SceneContext>.Instance.PlayerState.onEndGame += OnEndGame;
		SRSingleton<SceneContext>.Instance.PopupDirector.RegisterSuppressor();
		bgMat = new Material(backgroundImg.material);
		backgroundImg.material = bgMat;
		OnButtonsEnabled();
		SECTR_AudioSystem.PauseNonUISFX(pause: true);
		DLCButton.SetActive(DLCManageUI.IsEnabled());
		appearanceButton.SetActive(SlimeAppearanceUI.IsEnabled());
	}

	public void OnEnable()
	{
		musicDir.SetHouseMode(enabled: true);
		SRSingleton<PopupElementsUI>.Instance.RegisterBlocker(base.gameObject);
		beatrixObj = UnityEngine.Object.Instantiate(beatrixPrefab, Vector3.zero, Quaternion.identity);
		SECTR_AudioSystem.PauseNonUISFX(pause: true);
	}

	public void OnDisable()
	{
		musicDir.SetHouseMode(enabled: false);
		if (SRSingleton<PopupElementsUI>.Instance != null)
		{
			SRSingleton<PopupElementsUI>.Instance.DeregisterBlocker(base.gameObject);
		}
		Destroyer.Destroy(beatrixObj, "RanchHouseUI.OnDisable");
		if (camDisabler != null)
		{
			camDisabler.RemoveBlocker(this);
			camDisabler = null;
		}
		SECTR_AudioSystem.PauseNonUISFX(pause: false);
	}

	public void OnButtonsEnabled()
	{
		partnerButton.SetActive(SRSingleton<SceneContext>.Instance.RanchDirector.IsPartnerUnlocked());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SECTR_AudioSystem.PauseNonUISFX(pause: false);
		Destroyer.Destroy(bgMat, "RanchHouseUI.onDestroy");
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.PlayerState.onEndGame -= OnEndGame;
			SRSingleton<SceneContext>.Instance.PopupDirector.UnregisterSuppressor();
		}
	}

	private void OnEndGame()
	{
		Close();
		sleepButton.interactable = false;
	}

	public override void Update()
	{
		base.Update();
		timeText.text = timeDir.CurrTimeString();
		dayText.text = timeDir.CurrDayString();
		timeIcon.sprite = timeDir.CurrTimeIcon();
		mailIcon.enabled = mailDir.HasNewMail();
		mailHighlightText.text = mailDir.GetNewMailCount().ToString();
		mailHighlight.SetActive(mailDir.HasNewMail());
		int progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
		partnerArea.SetActive(progress > 0);
		partnerLevelText.text = progress.ToString();
		Vector3 vector = mailIcon.rectTransform.anchoredPosition;
		vector.y = mailInitY + ((progress > 0) ? 0f : 50f);
		mailIcon.rectTransform.anchoredPosition = vector;
		switch (fadeMode)
		{
		case FadeMode.IN_FADE_IN:
		case FadeMode.OUT_FADE_IN:
			obscurer.color = new Color(obscurer.color.r, obscurer.color.g, obscurer.color.b, Math.Min(1f, obscurer.color.a + Time.unscaledDeltaTime * 2f));
			break;
		case FadeMode.IN_FADE_OUT:
		case FadeMode.OUT_FADE_OUT:
			obscurer.color = new Color(obscurer.color.r, obscurer.color.g, obscurer.color.b, Math.Max(0f, obscurer.color.a - Time.unscaledDeltaTime * 2f));
			break;
		}
		if (fadeMode == FadeMode.IN_FADE_IN && obscurer.color.a == 1f)
		{
			mainUI.SetActive(value: true);
			fadeMode = FadeMode.IN_FADE_OUT;
			camDisabler.AddBlocker(this);
		}
		else if (fadeMode == FadeMode.IN_FADE_OUT && obscurer.color.a == 0f)
		{
			fadeMode = FadeMode.NONE;
		}
		else if (fadeMode == FadeMode.OUT_FADE_IN && obscurer.color.a == 1f)
		{
			mainUI.SetActive(value: false);
			fadeMode = FadeMode.OUT_FADE_OUT;
			camDisabler.RemoveBlocker(this);
		}
		else if (fadeMode == FadeMode.OUT_FADE_OUT && obscurer.color.a == 0f)
		{
			fadeMode = FadeMode.NONE;
			base.Close();
		}
		obscurer.gameObject.SetActive(obscurer.color.a > 0f);
		UpdateBackgroundMaterial();
	}

	public void Mail()
	{
		currMailUI = UnityEngine.Object.Instantiate(mailUI).GetComponent<MailUI>();
		buttonPanel.SetActive(value: false);
		MailUI obj = currMailUI;
		obj.onDestroy = (OnDestroyDelegate)Delegate.Combine(obj.onDestroy, (OnDestroyDelegate)delegate
		{
			if (buttonPanel != null)
			{
				buttonPanel.SetActive(value: true);
				OnButtonsEnabled();
			}
		});
	}

	public void CorporatePartner()
	{
		currPartnerUI = UnityEngine.Object.Instantiate(partnerUI).GetComponent<CorporatePartnerUI>();
		buttonPanel.SetActive(value: false);
		CorporatePartnerUI corporatePartnerUI = currPartnerUI;
		corporatePartnerUI.onDestroy = (OnDestroyDelegate)Delegate.Combine(corporatePartnerUI.onDestroy, (OnDestroyDelegate)delegate
		{
			if (buttonPanel != null)
			{
				buttonPanel.SetActive(value: true);
				OnButtonsEnabled();
			}
		});
	}

	public void SleepUntilMorning()
	{
		if (sleeping)
		{
			Debug.Log("Attempted to sleep while sleeping. Ignore.");
			return;
		}
		AnalyticsUtil.CustomEvent("PlayerSlept");
		sleeping = true;
		timeDir.Unpause(unpauseSFX: false);
		beatrixImg.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
		SRSingleton<LockOnDeath>.Instance.LockUntil(timeDir.GetNextDawn(), 0f, delegate
		{
			beatrixImg.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
			timeDir.Pause(pauseSFX: false);
			sleeping = false;
		});
	}

	public void OnButtonDLC()
	{
		currDLCManageUI = UnityEngine.Object.Instantiate(manageDLCPrefab).GetComponent<DLCManageUI>();
		buttonPanel.SetActive(value: false);
		DLCManageUI dLCManageUI = currDLCManageUI;
		dLCManageUI.onDestroy = (OnDestroyDelegate)Delegate.Combine(dLCManageUI.onDestroy, (OnDestroyDelegate)delegate
		{
			if (buttonPanel != null)
			{
				buttonPanel.SetActive(value: true);
				OnButtonsEnabled();
			}
		});
	}

	public void OnButtonAppearances()
	{
		currAppearanceUI = UnityEngine.Object.Instantiate(appearancePrefab).GetComponent<SlimeAppearanceUI>();
		buttonPanel.SetActive(value: false);
		SlimeAppearanceUI slimeAppearanceUI = currAppearanceUI;
		slimeAppearanceUI.onDestroy = (OnDestroyDelegate)Delegate.Combine(slimeAppearanceUI.onDestroy, (OnDestroyDelegate)delegate
		{
			if (buttonPanel != null)
			{
				buttonPanel.SetActive(value: true);
				OnButtonsEnabled();
			}
		});
	}

	protected override bool Closeable()
	{
		if (sleeping)
		{
			return false;
		}
		if (currMailUI != null || currPartnerUI != null || currDLCManageUI != null || currAppearanceUI != null)
		{
			return false;
		}
		if (isClosing)
		{
			return false;
		}
		return base.Closeable();
	}

	public override void Close()
	{
		progressDir.NoteReturnedToRanch();
		SECTR_AudioSystem.Play(closeCue, worldPos, loop: false);
		SRSingleton<SceneContext>.Instance.Player.GetComponent<PlayerDeathHandler>().ResetPlayerLocation(0f, delegate
		{
			fadeMode = FadeMode.OUT_FADE_IN;
		});
		isClosing = true;
	}

	private void UpdateBackgroundMaterial()
	{
		float value = timeDir.CurrDayFraction();
		bgMat.SetFloat("_DayFraction", value);
	}

	public void SetPosition(Vector3 pos)
	{
		worldPos = pos;
		SECTR_AudioSystem.Play(openCue, worldPos, loop: false);
	}
}
