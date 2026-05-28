namespace RichPresence
{
	public class NoopHandler : Handler
	{
		public static NoopHandler Instance = new NoopHandler();

		public void SetRichPresence(MainMenuData data)
		{
		}

		public void SetRichPresence(InZoneData data)
		{
		}
	}
}
