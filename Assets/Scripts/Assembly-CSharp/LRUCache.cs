using System.Collections.Generic;

public class LRUCache<K, V>
{
	private int capacity;

	private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();

	private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

	public LRUCache(int capacity)
	{
		this.capacity = capacity;
	}

	public bool contains(K key)
	{
		return cacheMap.ContainsKey(key);
	}

	public V get(K key)
	{
		if (cacheMap.TryGetValue(key, out var value))
		{
			V value2 = value.Value.value;
			lruList.Remove(value);
			lruList.AddLast(value);
			return value2;
		}
		throw new KeyNotFoundException();
	}

	public void put(K key, V val)
	{
		if (cacheMap.Count >= capacity)
		{
			removeFirst();
		}
		LinkedListNode<LRUCacheItem<K, V>> linkedListNode = new LinkedListNode<LRUCacheItem<K, V>>(new LRUCacheItem<K, V>(key, val));
		lruList.AddLast(linkedListNode);
		cacheMap.Add(key, linkedListNode);
	}

	public void clear(K key)
	{
		if (cacheMap.TryGetValue(key, out var value))
		{
			lruList.Remove(value);
			cacheMap.Remove(key);
		}
	}

	protected void removeFirst()
	{
		LinkedListNode<LRUCacheItem<K, V>> first = lruList.First;
		lruList.RemoveFirst();
		cacheMap.Remove(first.Value.key);
	}
}
