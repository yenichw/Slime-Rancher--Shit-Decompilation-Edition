namespace rail
{
	public interface IRailSystemHelper
	{
		float GetRateOfGameRevenue();

		RailResult SetTerminationTimeoutOwnershipExpired(int timeout_seconds);

		RailSystemState GetPlatformSystemState();
	}
}
