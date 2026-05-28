using System;

namespace rail
{
	public class IRailAppsImpl : RailObject, IRailApps
	{
		internal IRailAppsImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailAppsImpl()
		{
		}

		public virtual RailResult MarkGameContentDamaged(EnumRailGameContentDamageFlag flag)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailApps_MarkGameContentDamaged(swigCPtr_, (int)flag);
		}

		public virtual RailResult GetGameInstallPath(out string app_path)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.new_RailString__SWIG_0();
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailApps_GetGameInstallPath(swigCPtr_, intPtr);
			}
			finally
			{
				app_path = RAIL_API_PINVOKE.RailString_c_str(intPtr);
				RAIL_API_PINVOKE.delete_RailString(intPtr);
			}
		}

		public virtual RailResult GetGameLanguageCode(out string language_code)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.new_RailString__SWIG_0();
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailApps_GetGameLanguageCode(swigCPtr_, intPtr);
			}
			finally
			{
				language_code = RAIL_API_PINVOKE.RailString_c_str(intPtr);
				RAIL_API_PINVOKE.delete_RailString(intPtr);
			}
		}

		public virtual RailResult SetGameState(EnumRailGamePlayingState game_state_flag)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailApps_SetGameState(swigCPtr_, (int)game_state_flag);
		}

		public virtual RailResult GetGameState(out EnumRailGamePlayingState game_state_flag)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.NewInt();
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailApps_GetGameState(swigCPtr_, intPtr);
			}
			finally
			{
				game_state_flag = (EnumRailGamePlayingState)RAIL_API_PINVOKE.GetInt(intPtr);
				RAIL_API_PINVOKE.DeleteInt(intPtr);
			}
		}

		public virtual uint GetGameEarliestPurchaseTime()
		{
			return RAIL_API_PINVOKE.IRailApps_GetGameEarliestPurchaseTime(swigCPtr_);
		}

		public virtual RailResult GetCurrentBranchInfo(RailBranchInfo branch_info)
		{
			IntPtr intPtr = ((branch_info == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailBranchInfo__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailApps_GetCurrentBranchInfo(swigCPtr_, intPtr);
			}
			finally
			{
				if (branch_info != null)
				{
					RailConverter.Cpp2Csharp(intPtr, branch_info);
				}
				RAIL_API_PINVOKE.delete_RailBranchInfo(intPtr);
			}
		}

		public virtual RailResult AsyncQuerySubscribeWishPlayState(string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailApps_AsyncQuerySubscribeWishPlayState(swigCPtr_, user_data);
		}
	}
}
