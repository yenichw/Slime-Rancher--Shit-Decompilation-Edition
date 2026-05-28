using System.Collections;
using System.Collections.Generic;

public class Lookup<K, V> : IEnumerable
{
	private Dictionary<K, V> dictionary;

	private Dictionary<V, K> dictionaryReversed;

	public IEnumerable<K> Keys => dictionary.Keys;

	public Lookup(IEqualityComparer<K> keyComparer)
	{
		dictionary = new Dictionary<K, V>(keyComparer);
		dictionaryReversed = new Dictionary<V, K>();
	}

	public void Add(K key, V value)
	{
		dictionary.Add(key, value);
		dictionaryReversed.Add(value, key);
	}

	public IEnumerator GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	public bool TryGetValue(K key, out V value)
	{
		return dictionary.TryGetValue(key, out value);
	}

	public bool TryGetValue(V key, out K value)
	{
		return dictionaryReversed.TryGetValue(key, out value);
	}
}
