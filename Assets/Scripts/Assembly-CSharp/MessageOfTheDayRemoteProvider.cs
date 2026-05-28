using MonomiPark.SlimeRancher.Services;
using MonomiPark.SlimeRancher.Services.Messages;
using UnityEngine;

[CreateAssetMenu(menuName = "Message Of The Day/Remote Provider")]
public class MessageOfTheDayRemoteProvider : MessageOfTheDayProvider
{
	public string url;

	public int timeout;

	protected override void RetrieveMessage(SuccessHandler onSuccess, ErrorHandler onError)
	{
		MessageOfTheDayServiceRequest messageOfTheDayServiceRequest = new MessageOfTheDayServiceRequest(url, timeout);
		messageOfTheDayServiceRequest.OnError += delegate
		{
			onError();
		};
		messageOfTheDayServiceRequest.OnSuccess += delegate(MessageOfTheDayV01 message, Texture2D image)
		{
			onSuccess(CreateLocalizedMessage(message, image));
		};
		messageOfTheDayServiceRequest.Begin();
	}

	private MessageOfTheDay CreateLocalizedMessage(MessageOfTheDayV01 serviceMessage, Texture2D image)
	{
		Texture2D texture2D = new Texture2D(image.width, image.height, TextureFormat.RGBA32, mipChain: true);
		texture2D.SetPixels(image.GetPixels());
		texture2D.Apply();
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, image.width, image.height), new Vector2(0.5f, 0.5f));
		LocalizedMessageOfTheDay localizedMessageOfTheDay = new LocalizedMessageOfTheDay(serviceMessage.MessageId, sprite, "en");
		MessageOfTheDayV01.LocalizedMessage[] localizedMessages = serviceMessage.LocalizedMessages;
		foreach (MessageOfTheDayV01.LocalizedMessage localizedMessage in localizedMessages)
		{
			localizedMessageOfTheDay.AddEntry(localizedMessage.LanguageCode, localizedMessage.AnnouncementText, localizedMessage.TitleText, localizedMessage.BodyText, localizedMessage.ButtonText, localizedMessage.Url);
		}
		return localizedMessageOfTheDay;
	}
}
