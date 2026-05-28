using rail;

public delegate void RailCaptureVoiceCallback(EnumRailVoiceCaptureFormat fmt, bool is_last_package, byte[] encoded_buffer, uint encoded_length);
