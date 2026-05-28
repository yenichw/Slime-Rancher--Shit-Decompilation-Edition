using System;

namespace rail
{
	public class IRailVoiceHelperImpl : RailObject, IRailVoiceHelper
	{
		internal IRailVoiceHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailVoiceHelperImpl()
		{
		}

		public virtual IRailVoiceChannel AsyncCreateVoiceChannel(CreateVoiceChannelOption options, string channel_name, string user_data, out int result)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_CreateVoiceChannelOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailVoiceHelper_AsyncCreateVoiceChannel__SWIG_0(swigCPtr_, intPtr, channel_name, user_data, out result);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailVoiceChannelImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_CreateVoiceChannelOption(intPtr);
			}
		}

		public virtual IRailVoiceChannel AsyncCreateVoiceChannel(CreateVoiceChannelOption options, string channel_name, string user_data)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_CreateVoiceChannelOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailVoiceHelper_AsyncCreateVoiceChannel__SWIG_1(swigCPtr_, intPtr, channel_name, user_data);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailVoiceChannelImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_CreateVoiceChannelOption(intPtr);
			}
		}

		public virtual IRailVoiceChannel AsyncCreateVoiceChannel(CreateVoiceChannelOption options, string channel_name)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_CreateVoiceChannelOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailVoiceHelper_AsyncCreateVoiceChannel__SWIG_2(swigCPtr_, intPtr, channel_name);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailVoiceChannelImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_CreateVoiceChannelOption(intPtr);
			}
		}

		public virtual IRailVoiceChannel AsyncCreateVoiceChannel(CreateVoiceChannelOption options)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_CreateVoiceChannelOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailVoiceHelper_AsyncCreateVoiceChannel__SWIG_3(swigCPtr_, intPtr);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailVoiceChannelImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_CreateVoiceChannelOption(intPtr);
			}
		}

		public virtual IRailVoiceChannel OpenVoiceChannel(RailVoiceChannelID voice_channel_id, out int result)
		{
			IntPtr intPtr = ((voice_channel_id == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailVoiceChannelID__SWIG_0());
			if (voice_channel_id != null)
			{
				RailConverter.Csharp2Cpp(voice_channel_id, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailVoiceHelper_OpenVoiceChannel(swigCPtr_, intPtr, out result);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailVoiceChannelImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailVoiceChannelID(intPtr);
			}
		}

		public virtual RailResult SetupVoiceCapture(RailVoiceCaptureOption options, RailCaptureVoiceCallback callback)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailVoiceCaptureOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_SetupVoiceCapture__SWIG_0(swigCPtr_, intPtr, callback);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailVoiceCaptureOption(intPtr);
			}
		}

		public virtual RailResult SetupVoiceCapture(RailVoiceCaptureOption options)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailVoiceCaptureOption__SWIG_0());
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_SetupVoiceCapture__SWIG_1(swigCPtr_, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailVoiceCaptureOption(intPtr);
			}
		}

		public virtual RailResult StartVoiceCapturing(uint duration_milliseconds, bool repeat)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_StartVoiceCapturing__SWIG_0(swigCPtr_, duration_milliseconds, repeat);
		}

		public virtual RailResult StartVoiceCapturing(uint duration_milliseconds)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_StartVoiceCapturing__SWIG_1(swigCPtr_, duration_milliseconds);
		}

		public virtual RailResult StartVoiceCapturing()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_StartVoiceCapturing__SWIG_2(swigCPtr_);
		}

		public virtual RailResult StopVoiceCapturing()
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_StopVoiceCapturing(swigCPtr_);
		}

		public virtual RailResult GetCapturedVoiceData(byte[] buffer, uint buffer_length, out uint encoded_bytes_written)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_GetCapturedVoiceData(swigCPtr_, buffer, buffer_length, out encoded_bytes_written);
		}

		public virtual RailResult DecodeVoice(byte[] encoded_buffer, uint encoded_length, byte[] pcm_buffer, uint pcm_buffer_length, out uint pcm_buffer_written)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailVoiceHelper_DecodeVoice(swigCPtr_, encoded_buffer, encoded_length, pcm_buffer, pcm_buffer_length, out pcm_buffer_written);
		}
	}
}
