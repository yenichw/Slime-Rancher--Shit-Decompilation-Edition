using System.Collections;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageOfTheDayUI : MonoBehaviour
{
	[Header("UI Elements")]
	public GameObject messageWindow;

	public Image iconImage;

	public TMP_Text announcementLabel;

	public TMP_Text titleLabel;

	public TMP_Text bodyLabel;

	public MessageOfTheDayButton button;

	[Header("Button Animation")]
	public Ease easeType;

	public float moveTime;

	public float moveDistance;

	private MessageOfTheDay message;

	private string currentLanguage = "";

	private bool isMessageSet;

	private MessageDirector messageDirector;

	private void Awake()
	{
		messageDirector = SRSingleton<GameContext>.Instance.MessageDirector;
	}

	private void Start()
	{
		messageWindow.gameObject.SetActive(value: false);
		RequestAndSetupMessage();
	}

	public void Update()
	{
		if (isMessageSet)
		{
			string currentLanguageCode = messageDirector.GetCurrentLanguageCode();
			if (currentLanguage.CompareTo(currentLanguageCode) != 0)
			{
				UpdateLocalizedMessage(currentLanguageCode);
				currentLanguage = currentLanguageCode;
			}
		}
	}

	private void UpdateLocalizedMessage(string lang)
	{
		announcementLabel.text = message.GetAnnouncementText(lang);
		titleLabel.text = message.GetTitleText(lang);
		bodyLabel.text = message.GetBodyText(lang);
		button.UpdateButton(message.GetId(), message.GetButtonText(lang), message.GetUrl(lang));
		iconImage.sprite = message.GetSprite();
	}

	private void RequestAndSetupMessage()
	{
		SRSingleton<GameContext>.Instance.MessageOfTheDayDirector.GetProvider().Get(delegate(MessageOfTheDay m)
		{
			OnMessageSuccess(this, m);
		}, delegate
		{
			OnMessageFailure(this);
		});
	}

	private static void OnMessageSuccess(MessageOfTheDayUI instance, MessageOfTheDay retrievedMessage)
	{
		if (instance != null && instance.gameObject.activeInHierarchy)
		{
			instance.message = retrievedMessage;
			instance.isMessageSet = true;
			instance.gameObject.StartCoroutine(instance.AnimateIn());
		}
	}

	private static void OnMessageFailure(MessageOfTheDayUI instance)
	{
		if (instance != null && instance.gameObject.activeInHierarchy)
		{
			instance.isMessageSet = false;
		}
	}

	public IEnumerator AnimateIn()
	{
		messageWindow.gameObject.SetActive(value: true);
		RectTransform component = messageWindow.gameObject.GetComponent<RectTransform>();
		Transform obj = messageWindow.transform;
		float num = obj.position.x + component.rect.width;
		obj.position += Vector3.left * num;
		messageWindow.transform.DOBlendableMoveBy(Vector3.right * num, moveTime).SetEase(easeType).SetUpdate(isIndependentUpdate: true);
		yield return new WaitForSecondsRealtime(moveTime);
	}
}
