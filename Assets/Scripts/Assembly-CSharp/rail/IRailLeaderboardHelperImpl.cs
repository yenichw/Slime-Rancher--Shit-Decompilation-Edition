using System;

namespace rail
{
	public class IRailLeaderboardHelperImpl : RailObject, IRailLeaderboardHelper
	{
		internal IRailLeaderboardHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailLeaderboardHelperImpl()
		{
		}

		public virtual IRailLeaderboard OpenLeaderboard(string leaderboard_name)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailLeaderboardHelper_OpenLeaderboard(swigCPtr_, leaderboard_name);
			if (!(intPtr == IntPtr.Zero))
			{
				return new IRailLeaderboardImpl(intPtr);
			}
			return null;
		}
	}
}
