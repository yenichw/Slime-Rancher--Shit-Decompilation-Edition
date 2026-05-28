using System.Collections.Generic;
using UnityEngine;

public static class SRBehaviourStatic
{
	public static I GetInterfaceComponent<I>(this GameObject obj) where I : class
	{
		return obj.GetComponent(typeof(I)) as I;
	}

	public static I GetInterfaceComponentInParent<I>(this GameObject obj) where I : class
	{
		return obj.GetComponentInParent(typeof(I)) as I;
	}

	public static V Get<K, V>(this Dictionary<K, V> dict, K key)
	{
		dict.TryGetValue(key, out var value);
		return value;
	}
}
