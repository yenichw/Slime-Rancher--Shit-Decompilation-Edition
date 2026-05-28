using System;
using System.Collections.Generic;

namespace rail
{
	public class IRailVoiceChannelImpl : RailObject, IRailVoiceChannel, IRailComponent
	{
		internal IRailVoiceChannelImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailVoiceChannelImpl()
		{
		}

		public virtual RailVoiceChannelID GetVoiceChannelID()
		{
			IntPtr ptr = RAIL_API_PINVOKE.IRailVoiceChannel_GetVoiceChannelID(swigCPtr_);
			RailVoiceChannelID railVoiceChannelID = new RailVoiceChannelID();
			RailConverter.Cpp2Csharp(ptr, railVoiceChannelID);
			return railVoiceChannelID;
		}

		public virtual RailResult GetVoiceChannelName(out string name)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.new_RailString__SWIG_0();
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_GetVoiceChannelName(swigCPtr_, intPtr);
			}
			finally
			{
				name = RAIL_API_PINVOKE.RailString_c_str(intPtr);
				RAIL_API_PINVOKE.delete_RailString(intPtr);
			}
		}

		public virtual RailResult JoinVoiceChannel()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_JoinVoiceChannel(swigCPtr_);
		}

		public virtual RailResult LeaveVoiceChannel()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_LeaveVoiceChannel(swigCPtr_);
		}

		public virtual EnumRailVoiceChannelSpeakerState GetSpeakerState()
		{
			return (EnumRailVoiceChannelSpeakerState)RAIL_API_PINVOKE.IRailVoiceChannel_GetSpeakerState(swigCPtr_);
		}

		public virtual RailResult MuteSpeaker()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_MuteSpeaker(swigCPtr_);
		}

		public virtual RailResult ResumeSpeaker()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_ResumeSpeaker(swigCPtr_);
		}

		public virtual RailResult GetUsers(List<RailID> user_list)
		{
			IntPtr intPtr = ((user_list == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailID__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_GetUsers(swigCPtr_, intPtr);
			}
			finally
			{
				if (user_list != null)
				{
					RailConverter.Cpp2Csharp(intPtr, user_list);
				}
				RAIL_API_PINVOKE.delete_RailArrayRailID(intPtr);
			}
		}

		public virtual RailResult AddUsers(List<RailID> user_list)
		{
			IntPtr intPtr = ((user_list == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailID__SWIG_0());
			if (user_list != null)
			{
				RailConverter.Csharp2Cpp(user_list, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_AddUsers(swigCPtr_, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailID(intPtr);
			}
		}

		public virtual RailResult RemoveUsers(List<RailID> user_list)
		{
			IntPtr intPtr = ((user_list == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailID__SWIG_0());
			if (user_list != null)
			{
				RailConverter.Csharp2Cpp(user_list, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_RemoveUsers(swigCPtr_, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailID(intPtr);
			}
		}

		public virtual RailResult CloseChannel()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceChannel_CloseChannel(swigCPtr_);
		}

		public virtual ulong GetComponentVersion()
		{
			return RAIL_API_PINVOKE.IRailComponent_GetComponentVersion(swigCPtr_);
		}

		public virtual void Release()
		{
			RAIL_API_PINVOKE.IRailComponent_Release(swigCPtr_);
		}
	}
}
