using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageOfTheDayButton : MonoBehaviour
{
	public const string UNITY_ANALYTICS_CLICK_EVENT = "MotDClicked";

	public const string UNITY_ANALYTICS_MESSAGE_ID_KEY = "MessageId";

	public TMP_Text buttonLabel;

	private MessageOfTheDayDirector motdDirector;

	private string messageId;

	private string url;

	public void Awake()
	{
		motdDirector = SRSingleton<GameContext>.Instance.MessageOfTheDayDirector;
	}

	public void UpdateButton(string messageId, string buttonText, string url)
	{
		this.messageId = messageId;
		buttonLabel.text = buttonText;
		this.url = url;
	}

	public void OnClick()
	{
		AnalyticsUtil.CustomEvent("MotDClicked", new Dictionary<string, object> { { "MessageId", messageId } }, includeDefaultEventData: false);
		motdDirector.ActivateLink(url);
	}
}
