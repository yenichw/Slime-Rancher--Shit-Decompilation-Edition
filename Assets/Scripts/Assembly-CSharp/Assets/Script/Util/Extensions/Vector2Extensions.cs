using UnityEngine;

namespace Assets.Script.Util.Extensions
{
	public static class Vector2Extensions
	{
		public static float GetRandom(this Vector2 range)
		{
			return range.GetRandom(Randoms.SHARED);
		}

		public static float GetRandom(this Vector2 range, Randoms random)
		{
			return random.GetInRange(range.x, range.y);
		}

		public static float Lerp(this Vector2 range, float t)
		{
			return Mathf.Lerp(range.x, range.y, t);
		}
	}
}
