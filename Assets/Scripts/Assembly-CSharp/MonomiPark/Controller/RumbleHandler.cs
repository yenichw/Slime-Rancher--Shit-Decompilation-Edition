namespace MonomiPark.Controller
{
	public interface RumbleHandler
	{
		void EnableRumble();

		void DisableRumble();

		bool IsRumbleEnabled();

		void AddRumble(Rumble rumble);
	}
}
