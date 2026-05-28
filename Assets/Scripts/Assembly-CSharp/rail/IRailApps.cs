namespace rail
{
	public interface IRailApps
	{
		RailResult MarkGameContentDamaged(EnumRailGameContentDamageFlag flag);

		RailResult GetGameInstallPath(out string app_path);

		RailResult GetGameLanguageCode(out string language_code);

		RailResult SetGameState(EnumRailGamePlayingState game_state_flag);

		RailResult GetGameState(out EnumRailGamePlayingState game_state_flag);

		uint GetGameEarliestPurchaseTime();

		RailResult GetCurrentBranchInfo(RailBranchInfo branch_info);

		RailResult AsyncQuerySubscribeWishPlayState(string user_data);
	}
}
