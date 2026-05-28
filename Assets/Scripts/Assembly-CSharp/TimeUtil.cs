public static class TimeUtil
{
	public static bool HasReached(double currentWorldTime, double targetWorldTime)
	{
		return currentWorldTime >= targetWorldTime;
	}
}
