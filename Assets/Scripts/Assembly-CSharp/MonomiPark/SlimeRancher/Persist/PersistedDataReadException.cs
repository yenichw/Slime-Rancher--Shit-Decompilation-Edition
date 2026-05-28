using System;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PersistedDataReadException : Exception
	{
		public PersistedDataReadException(string message)
			: base(message)
		{
		}

		public PersistedDataReadException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
