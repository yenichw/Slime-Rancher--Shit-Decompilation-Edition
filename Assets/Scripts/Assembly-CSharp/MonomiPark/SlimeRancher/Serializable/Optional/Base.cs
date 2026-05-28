using UnityEngine;

namespace MonomiPark.SlimeRancher.Serializable.Optional
{
	public abstract class Base
	{
		[Tooltip("Indicates if the property value should be used.")]
		public bool HasValue;

		public static bool operator true(Base instance)
		{
			return instance.HasValue;
		}

		public static bool operator false(Base instance)
		{
			return !instance.HasValue;
		}
	}
	public abstract class Base<T> : Base
	{
		[Tooltip("Property value (requires HasValue to be enabled).")]
		public T Value;

		public T GetOrDefault(T defaultValue)
		{
			if (!HasValue)
			{
				return defaultValue;
			}
			return Value;
		}
	}
}
