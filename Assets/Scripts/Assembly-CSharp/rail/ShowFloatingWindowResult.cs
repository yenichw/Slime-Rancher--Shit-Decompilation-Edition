namespace rail
{
	public class ShowFloatingWindowResult : EventBase
	{
		public EnumRailWindowType window_type;

		public bool is_show;

		public new RailResult result;
	}
}
