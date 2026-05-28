using System;
using System.Collections.Generic;
using DLCPackage;

[Serializable]
public class BundledMessageOfTheDay : MessageOfTheDay
{
	public string url;

	public string announcementTranslationId;

	public string titleTranslationId;

	public string bodyTranslationId;

	public string buttonTranslationId;

	public List<Id> showForAvailableDLCPackages;

	public override string GetAnnouncementText(string lang)
	{
		return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get(announcementTranslationId);
	}

	public override string GetTitleText(string lang)
	{
		return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get(titleTranslationId);
	}

	public override string GetBodyText(string lang)
	{
		return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get(bodyTranslationId);
	}

	public override string GetButtonText(string lang)
	{
		return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get(buttonTranslationId);
	}

	public override string GetUrl(string lang)
	{
		return url;
	}
}
