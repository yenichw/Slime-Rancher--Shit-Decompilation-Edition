using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ObjectPool
{
	private class PreloadComponent : MonoBehaviour
	{
		public ObjectPool pool;

		private List<GameObject> prefabs;

		private int index;

		public void Init(IEnumerable<GameObject> prefabs, ObjectPool targetPool)
		{
			pool = targetPool;
			this.prefabs = new List<GameObject>(prefabs);
			index = 0;
		}

		public void Update()
		{
			while (index < prefabs.Count)
			{
				GameObject gameObject = prefabs[index];
				if (!pool.PoolHasMaxInstances(gameObject))
				{
					pool.Recycle(UnityEngine.Object.Instantiate(gameObject), gameObject);
					return;
				}
				index++;
			}
			Destroyer.Destroy(base.gameObject, "PreloadComponent.Update");
		}
	}

	private static List<GameObject> tempList = new List<GameObject>();

	private Dictionary<GameObject, int> pooledObjectMaxInstances = new Dictionary<GameObject, int>();

	private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

	public GameObject poolRoot;

	public ObjectPoolConfig config;

	private bool startupPoolsCreated;

	public void CreateStartupPools()
	{
		if (startupPoolsCreated)
		{
			return;
		}
		startupPoolsCreated = true;
		ObjectPoolConfig.StartupPool[] startupPools = config.startupPools;
		if (startupPools != null && startupPools.Length != 0)
		{
			for (int i = 0; i < startupPools.Length; i++)
			{
				CreatePool(startupPools[i].prefab, startupPools[i].size, startupPools[i].maxSize);
			}
		}
	}

	public void CreatePool<T>(T prefab, int initialPoolSize, int maxPoolSize) where T : Component
	{
		CreatePool(prefab.gameObject, initialPoolSize, maxPoolSize);
	}

	public void CreatePool(GameObject prefab, int initialPoolSize, int maxPoolSize)
	{
		if (!(prefab != null) || pooledObjects.ContainsKey(prefab))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		pooledObjects.Add(prefab, list);
		pooledObjectMaxInstances.Add(prefab, Math.Max(initialPoolSize, maxPoolSize));
		if (initialPoolSize > 0)
		{
			bool activeSelf = prefab.activeSelf;
			prefab.SetActive(value: false);
			Transform transform = poolRoot.transform;
			while (list.Count < initialPoolSize)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
				gameObject.transform.SetParent(transform, worldPositionStays: false);
				list.Add(gameObject);
			}
			prefab.SetActive(activeSelf);
		}
	}

	public T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
	}

	public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
	}

	public T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
	}

	public T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
	}

	public T Spawn<T>(T prefab, Transform parent) where T : Component
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject;
		if (pooledObjects.TryGetValue(prefab, out var value))
		{
			gameObject = null;
			if (value.Count > 0)
			{
				while (gameObject == null && value.Count > 0)
				{
					gameObject = value[value.Count - 1];
					value.RemoveAt(value.Count - 1);
				}
				if (gameObject != null)
				{
					Transform transform = gameObject.transform;
					transform.SetParent(parent, worldPositionStays: false);
					transform.localPosition = position;
					transform.localRotation = rotation;
					gameObject.SetActive(value: true);
					spawnedObjects.Add(gameObject, prefab);
					return gameObject;
				}
			}
			gameObject = UnityEngine.Object.Instantiate(prefab);
			Transform transform2 = gameObject.transform;
			transform2.SetParent(parent, worldPositionStays: false);
			transform2.localPosition = position;
			transform2.localRotation = rotation;
			spawnedObjects.Add(gameObject, prefab);
			return gameObject;
		}
		gameObject = UnityEngine.Object.Instantiate(prefab);
		Transform component = gameObject.GetComponent<Transform>();
		component.SetParent(parent, worldPositionStays: false);
		component.localPosition = position;
		component.localRotation = rotation;
		return gameObject;
	}

	public GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
	{
		return Spawn(prefab, parent, position, Quaternion.identity);
	}

	public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Spawn(prefab, null, position, rotation);
	}

	public GameObject Spawn(GameObject prefab, Transform parent)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}

	public GameObject Spawn(GameObject prefab, Vector3 position)
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}

	public GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public void Recycle<T>(T obj) where T : Component
	{
		Recycle(obj.gameObject);
	}

	public void Recycle(GameObject obj)
	{
		if (spawnedObjects.TryGetValue(obj, out var value) && !PoolHasMaxInstances(value))
		{
			Recycle(obj, value);
		}
		else if (obj != null)
		{
			obj.transform.SetParent(null, worldPositionStays: false);
			Destroyer.Destroy(obj.gameObject, "ObjectPool.Recycle");
		}
	}

	public void RecycleAfterFrame(GameObject toRecycle)
	{
		SRSingleton<SceneContext>.Instance.StartCoroutine(RecycleAfterFrame_Coroutine(toRecycle));
	}

	private IEnumerator RecycleAfterFrame_Coroutine(GameObject toRecycle)
	{
		yield return new WaitForEndOfFrame();
		Recycle(toRecycle);
	}

	private bool PoolHasMaxInstances(GameObject prefab)
	{
		return pooledObjects[prefab].Count >= pooledObjectMaxInstances[prefab];
	}

	private void Recycle(GameObject obj, GameObject prefab)
	{
		spawnedObjects.Remove(obj);
		if (obj != null)
		{
			pooledObjects[prefab].Add(obj);
			obj.transform.SetParent(poolRoot.transform, worldPositionStays: false);
			obj.SetActive(value: false);
		}
	}

	public void RecycleAll<T>(T prefab) where T : Component
	{
		RecycleAll(prefab.gameObject);
	}

	public void RecycleAll(GameObject prefab)
	{
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				tempList.Add(spawnedObject.Key);
			}
		}
		for (int i = 0; i < tempList.Count; i++)
		{
			Recycle(tempList[i]);
		}
		tempList.Clear();
	}

	public void RecycleAll()
	{
		tempList.AddRange(spawnedObjects.Keys);
		for (int i = 0; i < tempList.Count; i++)
		{
			Recycle(tempList[i]);
		}
		tempList.Clear();
	}

	public bool IsSpawned(GameObject obj)
	{
		return spawnedObjects.ContainsKey(obj);
	}

	public int CountPooled<T>(T prefab) where T : Component
	{
		return CountPooled(prefab.gameObject);
	}

	public int CountPooled(GameObject prefab)
	{
		if (pooledObjects.TryGetValue(prefab, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public int CountSpawned<T>(T prefab) where T : Component
	{
		return CountSpawned(prefab.gameObject);
	}

	public int CountSpawned(GameObject prefab)
	{
		int num = 0;
		foreach (GameObject value in spawnedObjects.Values)
		{
			if (prefab == value)
			{
				num++;
			}
		}
		return num;
	}

	public int CountAllPooled()
	{
		int num = 0;
		foreach (List<GameObject> value in pooledObjects.Values)
		{
			num += value.Count;
		}
		return num;
	}

	public List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		if (pooledObjects.TryGetValue(prefab, out var value))
		{
			list.AddRange(value);
		}
		return list;
	}

	public List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		if (pooledObjects.TryGetValue(prefab.gameObject, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				list.Add(value[i].GetComponent<T>());
			}
		}
		return list;
	}

	public List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				list.Add(spawnedObject.Key);
			}
		}
		return list;
	}

	public List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		GameObject gameObject = prefab.gameObject;
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in spawnedObjects)
		{
			if (spawnedObject.Value == gameObject)
			{
				list.Add(spawnedObject.Key.GetComponent<T>());
			}
		}
		return list;
	}

	public void DestroyPooled(GameObject prefab)
	{
		if (pooledObjects.TryGetValue(prefab, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				Destroyer.Destroy(value[i], "ObjectPool.DestroyPooled");
			}
			value.Clear();
		}
	}

	public void DestroyPooled<T>(T prefab) where T : Component
	{
		DestroyPooled(prefab.gameObject);
	}

	public void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}

	public void DestroyAll<T>(T prefab) where T : Component
	{
		DestroyAll(prefab.gameObject);
	}

	public GameObject Preload(Dictionary<GameObject, int> prefabs)
	{
		foreach (KeyValuePair<GameObject, int> prefab in prefabs)
		{
			CreatePool(prefab.Key, 0, prefab.Value);
			pooledObjectMaxInstances.TryGetValue(prefab.Key, out var value);
			pooledObjectMaxInstances[prefab.Key] = Mathf.Max(prefab.Value, value);
		}
		GameObject gameObject = new GameObject("ObjectPool.Preload");
		gameObject.AddComponent<PreloadComponent>().Init(prefabs.Keys, this);
		gameObject.transform.SetParent(poolRoot.transform, worldPositionStays: false);
		return gameObject;
	}
}
