using System;

internal class LRUCacheItem<K, V>
{
	public K key;

	private readonly WeakReference internalValue;

	public V value => (V)internalValue.Target;

	public LRUCacheItem(K k, V v)
	{
		key = k;
		internalValue = new WeakReference(v);
	}
}
