namespace rail
{
	public class ShowNotifyFloatingWindowResult : EventBase
	{
		public EnumRailNotifyWindowType notify_window_type;

		public bool is_show;

		public new RailResult result;
	}
}
