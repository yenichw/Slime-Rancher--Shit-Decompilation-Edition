using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Util.Extensions
{
	public static class TransformExtensions
	{
		public static string GetHierarchyString(this Transform transform)
		{
			return string.Join("/", (from it in transform.GetHierarchy()
				select it.name).ToArray());
		}

		public static IEnumerable<Transform> GetHierarchy(this Transform transform)
		{
			return transform.GetAscendents().Reverse();
		}

		private static IEnumerable<Transform> GetAscendents(this Transform transform)
		{
			Transform current = transform;
			while (current != null)
			{
				yield return current;
				current = current.parent;
			}
		}
	}
}
