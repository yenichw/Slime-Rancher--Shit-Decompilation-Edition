using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class SECTR_LightmapRef : MonoBehaviour
{
	[Serializable]
	public class RefData
	{
		public Texture2D FarLightmap;

		public Texture2D NearLightmap;

		public int index = -1;
	}

	[SerializeField]
	[HideInInspector]
	private List<RefData> lightmapRefs = new List<RefData>();

	private static int[] globalLightmapRefCount;

	public List<RefData> LightmapRefs => lightmapRefs;

	public static void InitRefCounts()
	{
		int num = LightmapSettings.lightmaps.Length;
		if (globalLightmapRefCount == null || globalLightmapRefCount.Length != num)
		{
			globalLightmapRefCount = new int[num];
		}
		for (int i = 0; i < num; i++)
		{
			LightmapData lightmapData = LightmapSettings.lightmaps[i];
			globalLightmapRefCount[i] = (((bool)lightmapData.lightmapColor || (bool)lightmapData.lightmapDir) ? 1 : 0);
		}
	}

	private void Start()
	{
		if ((Application.isEditor && !Application.isPlaying) || globalLightmapRefCount == null)
		{
			return;
		}
		int num = LightmapSettings.lightmaps.Length;
		int count = lightmapRefs.Count;
		for (int i = 0; i < count; i++)
		{
			RefData refData = lightmapRefs[i];
			if (refData.index < 0 || refData.index >= globalLightmapRefCount.Length)
			{
				continue;
			}
			if (globalLightmapRefCount[refData.index] == 0)
			{
				LightmapData lightmapData = new LightmapData();
				lightmapData.lightmapDir = refData.NearLightmap;
				lightmapData.lightmapColor = refData.FarLightmap;
				LightmapData[] array = new LightmapData[num];
				for (int j = 0; j < num; j++)
				{
					if (refData.index == j)
					{
						array[j] = lightmapData;
					}
					else
					{
						array[j] = LightmapSettings.lightmaps[j];
					}
				}
				LightmapSettings.lightmaps = array;
			}
			globalLightmapRefCount[refData.index]++;
		}
	}

	private void OnDestroy()
	{
		if ((Application.isEditor && !Application.isPlaying) || globalLightmapRefCount == null)
		{
			return;
		}
		int num = LightmapSettings.lightmaps.Length;
		int count = lightmapRefs.Count;
		for (int i = 0; i < count; i++)
		{
			RefData refData = lightmapRefs[i];
			if (refData.index < 0 || refData.index >= globalLightmapRefCount.Length)
			{
				continue;
			}
			globalLightmapRefCount[refData.index]--;
			if (globalLightmapRefCount[refData.index] != 0)
			{
				continue;
			}
			LightmapData[] array = new LightmapData[num];
			for (int j = 0; j < num; j++)
			{
				if (refData.index == j)
				{
					array[j] = new LightmapData();
				}
				else
				{
					array[j] = LightmapSettings.lightmaps[j];
				}
			}
			LightmapSettings.lightmaps = array;
		}
	}
}
