using System;

namespace rail
{
	public class IRailFloatingWindowImpl : RailObject, IRailFloatingWindow
	{
		internal IRailFloatingWindowImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailFloatingWindowImpl()
		{
		}

		public virtual RailResult AsyncShowRailFloatingWindow(EnumRailWindowType window_type, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailFloatingWindow_AsyncShowRailFloatingWindow(swigCPtr_, (int)window_type, user_data);
		}

		public virtual RailResult SetNotifyWindowPosition(EnumRailNotifyWindowType window, EnumRailNotifyWindowPosition position)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailFloatingWindow_SetNotifyWindowPosition(swigCPtr_, (int)window, (int)position);
		}

		public virtual RailResult AsyncShowStoreWindow(ulong id, RailStoreOptions options, string user_data)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailStoreOptions__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailFloatingWindow_AsyncShowStoreWindow(swigCPtr_, id, intPtr, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailStoreOptions(intPtr);
			}
		}

		public virtual bool IsFloatingWindowAvailable()
		{
			return RAIL_API_PINVOKE.IRailFloatingWindow_IsFloatingWindowAvailable(swigCPtr_);
		}

		public virtual RailResult AsyncShowDefaultGameStoreWindow(string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailFloatingWindow_AsyncShowDefaultGameStoreWindow(swigCPtr_, user_data);
		}

		public virtual RailResult AsyncShowChatWindowWithFriend(RailID rail_id)
		{
			IntPtr intPtr = ((rail_id == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (rail_id != null)
			{
				RailConverter.Csharp2Cpp(rail_id, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailFloatingWindow_AsyncShowChatWindowWithFriend(swigCPtr_, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}
	}
}
