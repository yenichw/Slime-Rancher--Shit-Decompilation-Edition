using UnityEngine;

namespace Assets.Script.Util.Extensions
{
	public static class Vector3Extensions
	{
		public static bool IsNaN(this Vector3 instance)
		{
			if (!float.IsNaN(instance.x) && !float.IsNaN(instance.y))
			{
				return float.IsNaN(instance.z);
			}
			return true;
		}
	}
}
