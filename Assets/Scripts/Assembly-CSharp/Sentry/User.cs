using System;

namespace Sentry
{
	[Serializable]
	public class User
	{
		public string id = SentrySdk.SESSION_USER_ID;
	}
}
