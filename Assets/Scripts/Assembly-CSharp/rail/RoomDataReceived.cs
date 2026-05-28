namespace rail
{
	public class RoomDataReceived : EventBase
	{
		public uint message_type;

		public uint data_len;

		public ulong room_id;

		public string data_buffer;
	}
}
