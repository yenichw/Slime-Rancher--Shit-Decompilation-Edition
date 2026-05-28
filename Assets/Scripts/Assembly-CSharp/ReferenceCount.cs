using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReferenceCount<T> : IEnumerable<KeyValuePair<T, int>>, IEnumerable
{
	private class Dictionary_Rigidbody_int
	{
		private Dictionary<Rigidbody, int> instance;
	}

	private class Dictionary_IdentifiableId_int
	{
		private Dictionary<Identifiable.Id, int> instance;
	}

	private readonly Dictionary<T, int> dictionary;

	public IEnumerable<T> Keys => dictionary.Keys;

	public ReferenceCount()
	{
		dictionary = new Dictionary<T, int>();
	}

	public ReferenceCount(IEqualityComparer<T> keyComparer)
	{
		dictionary = new Dictionary<T, int>(keyComparer);
	}

	public ReferenceCount(IEnumerable<KeyValuePair<T, int>> enumerable)
	{
		dictionary = enumerable.ToDictionary((KeyValuePair<T, int> kv) => kv.Key, (KeyValuePair<T, int> kv) => kv.Value);
	}

	public ReferenceCount(IEnumerable<KeyValuePair<T, int>> enumerable, IEqualityComparer<T> keyComparer)
	{
		dictionary = enumerable.ToDictionary((KeyValuePair<T, int> kv) => kv.Key, (KeyValuePair<T, int> kv) => kv.Value, keyComparer);
	}

	private static bool AOTHelper_Dictionary_Rigidbody_int()
	{
		return new Dictionary<Rigidbody, int>() == null;
	}

	private static bool AOTHelper_Dictionary_IdentifiableId_int()
	{
		return new Dictionary<Identifiable.Id, int>() == null;
	}

	public int Increment(T key)
	{
		int num = GetCount(key) + 1;
		dictionary[key] = num;
		return num;
	}

	public int Decrement(T key)
	{
		int num = GetCount(key) - 1;
		if (num < 0)
		{
			throw new InvalidOperationException();
		}
		if (num == 0)
		{
			dictionary.Remove(key);
		}
		else
		{
			dictionary[key] = num;
		}
		return num;
	}

	public int GetCount(T key)
	{
		dictionary.TryGetValue(key, out var value);
		return value;
	}

	public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	private IEnumerator GetEnumerator_private()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator_private();
	}

	public void Remove(T key)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary.Remove(key);
		}
	}

	public void Clear()
	{
		dictionary.Clear();
	}

	public bool ContainsKey(T key)
	{
		return GetCount(key) > 0;
	}
}
