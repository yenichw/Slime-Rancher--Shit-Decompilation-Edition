using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : SRSingleton<HudUI>
{
	[Tooltip("UIContainer parent GameObject.")]
	public GameObject uiContainer;

	[Tooltip("Reference to the EnergyMeter child.")]
	public EnergyMeter energyMeter;

	public TMP_Text currencyText;

	public TMP_Text keysText;

	public TMP_Text dayText;

	public TMP_Text timeText;

	public TMP_Text debugText;

	public Image timeIcon;

	public Image mailIcon;

	public Image keysIcon;

	public GameObject partnerArea;

	public TMP_Text partnerLevelText;

	public Image autosaveImg;

	public GameObject keyGainFX;

	private PlayerState player;

	private TimeDirector timeDir;

	private MailDirector mailDir;

	private ProgressDirector progressDir;

	private AutoSaveDirector autosaveDir;

	private OptionsDirector optionsDir;

	private Hashtable scaleToTweenArgs;

	private Hashtable scaleBackTweenArgs;

	private float mailInitY;

	private const float OFFSET_SPACING = 50f;

	private const float GAME_SAVED_NOTIFICATION_DURATION = 5f;

	private bool priorShowMinimalHud;

	[Tooltip("Game objects in this list will be hidden when Minimal HUD is enabled.")]
	[SerializeField]
	private GameObject[] disabledOnMinimalHud;

	private int lastTime = -1;

	private int lastCurrency = -1;

	private int lastKeys = -1;

	private int lastPartnerLevel = -1;

	public override void Awake()
	{
		base.Awake();
		optionsDir = SRSingleton<GameContext>.Instance.OptionsDirector;
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		mailDir = SRSingleton<SceneContext>.Instance.MailDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		autosaveDir = SRSingleton<GameContext>.Instance.AutoSaveDirector;
		mailInitY = mailIcon.rectTransform.anchoredPosition.y;
	}

	public void Start()
	{
		Update();
		debugText.gameObject.SetActive(value: false);
		debugText.text = string.Empty;
	}

	public void Update()
	{
		bool showMinimalHUD = optionsDir.GetShowMinimalHUD();
		if (priorShowMinimalHud != showMinimalHUD)
		{
			bool active = !showMinimalHUD;
			for (int i = 0; i < disabledOnMinimalHud.Length; i++)
			{
				GameObject gameObject = disabledOnMinimalHud[i];
				if (gameObject != null)
				{
					gameObject.SetActive(active);
				}
			}
			priorShowMinimalHud = showMinimalHUD;
		}
		if (!showMinimalHUD)
		{
			int num = timeDir.CurrTime();
			if (num != lastTime)
			{
				dayText.text = timeDir.CurrDayString();
				timeText.text = timeDir.CurrTimeString();
				timeIcon.sprite = timeDir.CurrTimeIcon();
				lastTime = num;
			}
			int displayedCurrency = player.GetDisplayedCurrency();
			if (displayedCurrency != lastCurrency)
			{
				currencyText.text = displayedCurrency.ToString();
				lastCurrency = displayedCurrency;
			}
			int keys = player.GetKeys();
			if (keys != lastKeys)
			{
				if (keys > 0)
				{
					string text = keys.ToString();
					ScaleUpAndBack(keysIcon.gameObject);
					ScaleUpAndBack(keysText.gameObject);
					keysText.text = text;
				}
				keysIcon.enabled = keys > 0;
				keysText.enabled = keys > 0;
				lastKeys = keys;
			}
			bool flag = false;
			int progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
			if (progress != lastPartnerLevel)
			{
				partnerArea.SetActive(progress > 0);
				partnerLevelText.text = progress.ToString();
				lastPartnerLevel = progress;
				flag = true;
			}
			bool flag2 = mailDir.HasNewMail();
			if (flag2 != mailIcon.enabled)
			{
				mailIcon.enabled = flag2;
				flag = true;
			}
			if (flag)
			{
				Vector3 vector = mailIcon.rectTransform.anchoredPosition;
				vector.y = mailInitY + ((progress > 0) ? 0f : 50f);
				mailIcon.rectTransform.anchoredPosition = vector;
			}
		}
		autosaveImg.enabled = Time.time - autosaveDir.GetLastSaveTime() < 5f;
	}

	private void ScaleUpAndBack(GameObject gameObject)
	{
		DOTween.Sequence().Append(gameObject.transform.DOScale(2f, 0.25f).SetEase(Ease.Linear)).Append(gameObject.transform.DOScale(1f, 0.25f).SetEase(Ease.Linear));
	}
}
