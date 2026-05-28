using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeAppearanceObjectPoolPreloader : MonoBehaviour
{
	[Serializable]
	public class PreloadEntry
	{
		[Tooltip("Prefab to preload.")]
		public GameObject prefab;

		[Tooltip("Number of prefab instances to preload.")]
		public int count;
	}

	[Tooltip("List of prefab entries to preload.")]
	public List<PreloadEntry> preloads;

	private Dictionary<GameObject, int> preloadsDict;

	private GameObject handler;

	public void Awake()
	{
		preloadsDict = preloads.ToDictionary((PreloadEntry p) => p.prefab, (PreloadEntry p) => p.count);
	}

	public void OnEnable()
	{
		Destroyer.Destroy(handler, "ObjectPoolPreloader.OnEnable");
		handler = SlimeAppearanceObjectPool.Preload(preloadsDict);
	}

	public void OnDisable()
	{
		Destroyer.Destroy(handler, "ObjectPoolPreloader.OnDisable");
		foreach (GameObject key in preloadsDict.Keys)
		{
			SlimeAppearanceObjectPool.DestroyPooled(key);
		}
	}
}
