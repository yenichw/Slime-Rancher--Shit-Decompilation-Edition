namespace rail
{
	public interface IRailFloatingWindow
	{
		RailResult AsyncShowRailFloatingWindow(EnumRailWindowType window_type, string user_data);

		RailResult SetNotifyWindowPosition(EnumRailNotifyWindowType window, EnumRailNotifyWindowPosition position);

		RailResult AsyncShowStoreWindow(ulong id, RailStoreOptions options, string user_data);

		bool IsFloatingWindowAvailable();

		RailResult AsyncShowDefaultGameStoreWindow(string user_data);

		RailResult AsyncShowChatWindowWithFriend(RailID rail_id);
	}
}
