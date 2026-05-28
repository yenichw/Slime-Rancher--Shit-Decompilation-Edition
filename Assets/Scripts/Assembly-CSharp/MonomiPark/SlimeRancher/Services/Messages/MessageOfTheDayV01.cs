using System.Xml.Serialization;

namespace MonomiPark.SlimeRancher.Services.Messages
{
	public class MessageOfTheDayV01
	{
		public class LocalizedMessage
		{
			[XmlAttribute(AttributeName = "languageCode")]
			public string LanguageCode;

			public string AnnouncementText;

			public string TitleText;

			public string BodyText;

			public string ButtonText;

			public string Url;
		}

		[XmlAttribute(AttributeName = "messageId")]
		public string MessageId;

		public string ImageUrl;

		public string MessageTranslationId;

		public LocalizedMessage[] LocalizedMessages;
	}
}
