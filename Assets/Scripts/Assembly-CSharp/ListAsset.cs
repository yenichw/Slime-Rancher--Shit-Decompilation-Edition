using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ListAsset<T> : ScriptableObject, IEnumerable<T>, IEnumerable
{
	[SerializeField]
	private List<T> items = new List<T>();

	public T this[int index] => items[index];

	public int Count => items.Count;

	public IEnumerator<T> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
