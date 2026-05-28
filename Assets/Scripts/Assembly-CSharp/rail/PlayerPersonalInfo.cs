namespace rail
{
	public class PlayerPersonalInfo
	{
		public EnumRailPlayerOnLineState state;

		public string avatar_url;

		public uint rail_level;

		public RailID rail_id = new RailID();

		public string rail_name;

		public RailResult err_code;
	}
}
