using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Script.Util.Extensions
{
	public static class GameObjectExtensions
	{
		private class CoroutineRunner : MonoBehaviour
		{
			public IEnumerator CoroutineWrapper(IEnumerator coroutine)
			{
				yield return coroutine;
				Destroyer.Destroy(this, "CoroutineRunner.CoroutineWrapper");
			}

			public void OnDisable()
			{
				Destroyer.Destroy(this, "CoroutineRunner.OnDisable");
			}
		}

		public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
		{
			return gameObject.GetComponentsInParent<T>(includeInactive).FirstOrDefault();
		}

		public static T GetRequiredComponent<T>(this GameObject gameObject) where T : Component
		{
			return gameObject.GetComponent<T>();
		}

		public static T GetRequiredComponentInParent<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
		{
			return gameObject.GetComponentInParent<T>(includeInactive);
		}

		public static T GetRequiredComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
		{
			return gameObject.GetComponentInChildren<T>(includeInactive);
		}

		public static void StartCoroutine(this GameObject gameObject, IEnumerator coroutine)
		{
			CoroutineRunner coroutineRunner = gameObject.AddComponent<CoroutineRunner>();
			coroutineRunner.StartCoroutine(coroutineRunner.CoroutineWrapper(coroutine));
		}

		public static void DestroyChildren(this GameObject parent, string source)
		{
			parent.transform.DestroyChildren(source);
		}

		public static void DestroyChildren(this Transform parent, string source)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Destroyer.Destroy(parent.GetChild(i).gameObject, source);
			}
		}
	}
}
