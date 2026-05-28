using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizedMessageOfTheDay : MessageOfTheDay
{
	private struct LocalizedEntry
	{
		public string announcementText;

		public string titleText;

		public string bodyText;

		public string buttonText;

		public string url;
	}

	private Dictionary<string, LocalizedEntry> localizedEntries = new Dictionary<string, LocalizedEntry>();

	private string defaultLanguageCode;

	public LocalizedMessageOfTheDay(string id, Sprite sprite, string defaultLanguageCode)
	{
		base.id = id;
		base.sprite = sprite;
		this.defaultLanguageCode = defaultLanguageCode;
	}

	public void AddEntry(string languageCode, string announcementText, string titleText, string bodyText, string buttonText, string url)
	{
		localizedEntries.Add(languageCode, new LocalizedEntry
		{
			announcementText = announcementText,
			titleText = titleText,
			bodyText = bodyText,
			buttonText = buttonText,
			url = url
		});
	}

	public override string GetAnnouncementText(string languageCode)
	{
		return GetEntryText(languageCode, (LocalizedEntry entry) => entry.announcementText);
	}

	public override string GetTitleText(string languageCode)
	{
		return GetEntryText(languageCode, (LocalizedEntry entry) => entry.titleText);
	}

	public override string GetBodyText(string languageCode)
	{
		return GetEntryText(languageCode, (LocalizedEntry entry) => entry.bodyText);
	}

	public override string GetButtonText(string languageCode)
	{
		return GetEntryText(languageCode, (LocalizedEntry entry) => entry.buttonText);
	}

	private string GetEntryText(string languageCode, Func<LocalizedEntry, string> extractor)
	{
		if (!TryGetLocalizedValue(languageCode, out var entry))
		{
			return "";
		}
		return extractor(entry);
	}

	private bool TryGetLocalizedValue(string languageCode, out LocalizedEntry entry)
	{
		if (!localizedEntries.TryGetValue(languageCode, out entry))
		{
			return localizedEntries.TryGetValue(defaultLanguageCode, out entry);
		}
		return true;
	}

	public override string GetUrl(string languageCode)
	{
		if (localizedEntries.TryGetValue(languageCode, out var value))
		{
			return value.url;
		}
		if (localizedEntries.TryGetValue(defaultLanguageCode, out value))
		{
			return value.url;
		}
		return "";
	}
}
