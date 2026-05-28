namespace MonomiPark.Controller
{
	public class EmptyRumbleHandler : RumbleHandler
	{
		private bool enabled;

		public void AddRumble(Rumble rumble)
		{
		}

		public void DisableRumble()
		{
			enabled = false;
		}

		public void EnableRumble()
		{
			enabled = true;
		}

		public bool IsRumbleEnabled()
		{
			return enabled;
		}
	}
}
