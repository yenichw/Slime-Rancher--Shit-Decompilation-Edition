using System;
using UnityEngine;

public class CyclePrefabs : SRBehaviour
{
	[Serializable]
	public struct PrefabEntry
	{
		public GameObject prefab;

		public float cameraDist;
	}

	public PrefabEntry[] prefabs;
}
