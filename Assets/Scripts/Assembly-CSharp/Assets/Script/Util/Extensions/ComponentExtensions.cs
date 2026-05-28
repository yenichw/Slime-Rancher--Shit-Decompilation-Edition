using System;
using UnityEngine;

namespace Assets.Script.Util.Extensions
{
	public static class ComponentExtensions
	{
		public static T GetComponentInParent<T>(this Component component, bool includeInactive = false) where T : Component
		{
			return component.gameObject.GetComponentInParent<T>(includeInactive);
		}

		public static T GetRequiredComponent<T>(this Component component) where T : Component
		{
			return component.gameObject.GetRequiredComponent<T>();
		}

		public static T GetRequiredComponentInParent<T>(this Component component, bool includeInactive = false) where T : Component
		{
			return component.gameObject.GetRequiredComponentInParent<T>(includeInactive);
		}

		public static T GetRequiredComponentInChildren<T>(this Component component, bool includeInactive = false) where T : Component
		{
			return component.gameObject.GetRequiredComponentInChildren<T>(includeInactive);
		}

		public static void DestroyChildren(this Component parent, string source)
		{
			parent.DestroyChildren((GameObject go) => true, source);
		}

		public static void DestroyChildren(this Component parent, Predicate<GameObject> predicate, string source)
		{
			for (int i = 0; i < parent.transform.childCount; i++)
			{
				GameObject gameObject = parent.transform.GetChild(i).gameObject;
				if (predicate(gameObject))
				{
					if (gameObject.GetComponent<Identifiable>() != null)
					{
						Destroyer.DestroyActor(gameObject, source);
					}
					else
					{
						Destroyer.Destroy(gameObject, source);
					}
				}
			}
		}
	}
}
