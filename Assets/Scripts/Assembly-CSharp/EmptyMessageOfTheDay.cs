public class EmptyMessageOfTheDay : MessageOfTheDay
{
	public static EmptyMessageOfTheDay Default = new EmptyMessageOfTheDay();

	private EmptyMessageOfTheDay()
	{
	}

	public override string GetUrl(string lang)
	{
		return "";
	}

	public override string GetAnnouncementText(string lang)
	{
		return "";
	}

	public override string GetTitleText(string lang)
	{
		return "";
	}

	public override string GetBodyText(string lang)
	{
		return "";
	}

	public override string GetButtonText(string lang)
	{
		return "";
	}
}
