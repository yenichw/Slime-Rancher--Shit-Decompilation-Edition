public class SRSingleton<T> : SRBehaviour where T : SRSingleton<T>
{
	private class Nested
	{
		internal static T instance;

		static Nested()
		{
		}
	}

	public static T Instance => Nested.instance;

	public virtual void Awake()
	{
		if (!(Nested.instance == this))
		{
			if (Nested.instance != null)
			{
				Log.Error(string.Concat("An instance of the singleton ", typeof(T), " already exists, attempting to create additional."));
			}
			Nested.instance = (T)this;
		}
	}

	public virtual void OnDestroy()
	{
		if (Nested.instance == this)
		{
			Nested.instance = null;
		}
	}
}
