namespace RichPresence
{
	public interface Handler
	{
		void SetRichPresence(MainMenuData data);

		void SetRichPresence(InZoneData data);
	}
}
