using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlimeAppearanceObjectPool : MonoBehaviour, SlimeAppearanceObjectProvider
{
	public enum StartupPoolMode
	{
		Awake = 0,
		Start = 1,
		CallManually = 2
	}

	[Serializable]
	public class StartupPool
	{
		public int size;

		public GameObject prefab;

		public int maxSize;

		public bool doesNotSelfDestruct;
	}

	private class PreloadComponent : MonoBehaviour
	{
		private List<GameObject> prefabs;

		private int index;

		public void Init(IEnumerable<GameObject> prefabs)
		{
			this.prefabs = new List<GameObject>(prefabs);
			index = 0;
		}

		public void Update()
		{
			while (index < prefabs.Count)
			{
				GameObject gameObject = prefabs[index];
				if (!PoolHasMaxInstances(gameObject))
				{
					Recycle(UnityEngine.Object.Instantiate(gameObject), gameObject);
					return;
				}
				index++;
			}
			Destroyer.Destroy(base.gameObject, "PreloadComponent.Update");
		}
	}

	private static SlimeAppearanceObjectPool _instance;

	private static List<GameObject> tempList = new List<GameObject>();

	private Dictionary<GameObject, int> pooledObjectMaxInstances = new Dictionary<GameObject, int>();

	private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

	public StartupPoolMode startupPoolMode;

	public StartupPool[] startupPools;

	private bool startupPoolsCreated;

	public static SlimeAppearanceObjectPool instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			_instance = UnityEngine.Object.FindObjectOfType<SlimeAppearanceObjectPool>();
			if (_instance != null)
			{
				return _instance;
			}
			GameObject obj = new GameObject("SlimeAppearanceObjectPool");
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			_instance = obj.AddComponent<SlimeAppearanceObjectPool>();
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
		if (startupPoolMode == StartupPoolMode.Awake)
		{
			CreateStartupPools();
		}
	}

	private void Start()
	{
		if (startupPoolMode == StartupPoolMode.Start)
		{
			CreateStartupPools();
		}
	}

	public static void CreateStartupPools()
	{
		if (instance.startupPoolsCreated)
		{
			return;
		}
		instance.startupPoolsCreated = true;
		StartupPool[] array = instance.startupPools;
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				CreatePool(array[i].prefab, array[i].size, array[i].maxSize);
			}
		}
	}

	public static void CreatePool<T>(T prefab, int initialPoolSize, int maxPoolSize) where T : Component
	{
		CreatePool(prefab.gameObject, initialPoolSize, maxPoolSize);
	}

	public static void CreatePool(GameObject prefab, int initialPoolSize, int maxPoolSize)
	{
		if (!(prefab != null) || instance.pooledObjects.ContainsKey(prefab))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		instance.pooledObjects.Add(prefab, list);
		instance.pooledObjectMaxInstances.Add(prefab, Math.Max(initialPoolSize, maxPoolSize));
		if (initialPoolSize > 0)
		{
			bool activeSelf = prefab.activeSelf;
			prefab.SetActive(value: false);
			Transform parent = instance.transform;
			while (list.Count < initialPoolSize)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
				gameObject.transform.parent = parent;
				list.Add(gameObject);
			}
			prefab.SetActive(activeSelf);
		}
	}

	public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Transform parent) where T : Component
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject;
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
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
					Transform obj = gameObject.transform;
					obj.SetParent(parent, worldPositionStays: false);
					obj.localPosition = position;
					obj.localRotation = rotation;
					gameObject.SetActive(value: true);
					instance.spawnedObjects.Add(gameObject, prefab);
					return gameObject;
				}
			}
			gameObject = UnityEngine.Object.Instantiate(prefab);
			Transform obj2 = gameObject.transform;
			obj2.SetParent(parent, worldPositionStays: false);
			obj2.localPosition = position;
			obj2.localRotation = rotation;
			instance.spawnedObjects.Add(gameObject, prefab);
			return gameObject;
		}
		gameObject = UnityEngine.Object.Instantiate(prefab);
		Transform component = gameObject.GetComponent<Transform>();
		component.SetParent(parent, worldPositionStays: false);
		component.localPosition = position;
		component.localRotation = rotation;
		return gameObject;
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
	{
		return Spawn(prefab, parent, position, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Spawn(prefab, null, position, rotation);
	}

	public static GameObject Spawn(GameObject prefab, Transform parent)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab, Vector3 position)
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static void Recycle<T>(T obj) where T : Component
	{
		Recycle(obj.gameObject);
	}

	public static void Recycle(GameObject obj)
	{
		if (instance.spawnedObjects.TryGetValue(obj, out var value) && !PoolHasMaxInstances(value))
		{
			Recycle(obj, value);
			return;
		}
		obj.transform.parent = null;
		Destroyer.Destroy(obj.gameObject, "SlimeAppearanceObjectPool.Recycle");
	}

	private static bool PoolHasMaxInstances(GameObject prefab)
	{
		return instance.pooledObjects[prefab].Count >= instance.pooledObjectMaxInstances[prefab];
	}

	private static void Recycle(GameObject obj, GameObject prefab)
	{
		instance.pooledObjects[prefab].Add(obj);
		instance.spawnedObjects.Remove(obj);
		obj.transform.SetParent(instance.transform, worldPositionStays: false);
		obj.SetActive(value: false);
	}

	public static void RecycleAll<T>(T prefab) where T : Component
	{
		RecycleAll(prefab.gameObject);
	}

	public static void RecycleAll(GameObject prefab)
	{
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
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

	public static void RecycleAll()
	{
		tempList.AddRange(instance.spawnedObjects.Keys);
		for (int i = 0; i < tempList.Count; i++)
		{
			Recycle(tempList[i]);
		}
		tempList.Clear();
	}

	public static bool IsSpawned(GameObject obj)
	{
		return instance.spawnedObjects.ContainsKey(obj);
	}

	public static int CountPooled<T>(T prefab) where T : Component
	{
		return CountPooled(prefab.gameObject);
	}

	public static int CountPooled(GameObject prefab)
	{
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public static int CountSpawned<T>(T prefab) where T : Component
	{
		return CountSpawned(prefab.gameObject);
	}

	public static int CountSpawned(GameObject prefab)
	{
		int num = 0;
		foreach (GameObject value in instance.spawnedObjects.Values)
		{
			if (prefab == value)
			{
				num++;
			}
		}
		return num;
	}

	public static int CountAllPooled()
	{
		int num = 0;
		foreach (List<GameObject> value in instance.pooledObjects.Values)
		{
			num += value.Count;
		}
		return num;
	}

	public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
		{
			list.AddRange(value);
		}
		return list;
	}

	public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		if (instance.pooledObjects.TryGetValue(prefab.gameObject, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				list.Add(value[i].GetComponent<T>());
			}
		}
		return list;
	}

	public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				list.Add(spawnedObject.Key);
			}
		}
		return list;
	}

	public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
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
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == gameObject)
			{
				list.Add(spawnedObject.Key.GetComponent<T>());
			}
		}
		return list;
	}

	public static void DestroyPooled(GameObject prefab)
	{
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				Destroyer.Destroy(value[i], "ObjectPool.DestroyPooled");
			}
			value.Clear();
		}
	}

	public static void DestroyPooled<T>(T prefab) where T : Component
	{
		DestroyPooled(prefab.gameObject);
	}

	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}

	public static void DestroyAll<T>(T prefab) where T : Component
	{
		DestroyAll(prefab.gameObject);
	}

	public List<string> CheckPooledConfiguration()
	{
		List<string> list = new List<string>();
		if (startupPools == null || startupPools.Length == 0)
		{
			list.Add("No pools are configured");
			return list;
		}
		for (int i = 0; i < startupPools.Length; i++)
		{
			StartupPool startupPool = startupPools[i];
			if (startupPool == null)
			{
				list.Add($"Pool {i} is null.");
			}
			else if (startupPool.prefab == null)
			{
				list.Add($"Pool {i} has a null prefab.");
			}
			else if (startupPool.size == 0)
			{
				list.Add($"Pool {i} has a pool count of zero.");
			}
		}
		return list;
	}

	public static GameObject Preload(Dictionary<GameObject, int> prefabs)
	{
		foreach (KeyValuePair<GameObject, int> prefab in prefabs)
		{
			CreatePool(prefab.Key, 0, prefab.Value);
			instance.pooledObjectMaxInstances.TryGetValue(prefab.Key, out var value);
			instance.pooledObjectMaxInstances[prefab.Key] = Mathf.Max(prefab.Value, value);
		}
		GameObject obj = new GameObject("SlimeAppearanceObjectPool.Preload");
		obj.AddComponent<PreloadComponent>().Init(prefabs.Keys);
		obj.transform.SetParent(instance.transform, worldPositionStays: false);
		return obj;
	}

	public SlimeAppearanceObject Get(SlimeAppearanceObject appearanceObjectPrefab, GameObject targetParent)
	{
		return Spawn(appearanceObjectPrefab, targetParent.transform, appearanceObjectPrefab.transform.position, appearanceObjectPrefab.transform.rotation);
	}

	public void Put(SlimeAppearanceObject appearanceObjectPrefab, SlimeAppearanceObject appearanceObject)
	{
		Recycle(appearanceObject);
	}
}
