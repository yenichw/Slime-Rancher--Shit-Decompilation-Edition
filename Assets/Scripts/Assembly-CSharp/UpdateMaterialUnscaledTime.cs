using System.Collections.Generic;
using UnityEngine;

public class UpdateMaterialUnscaledTime : MonoBehaviour
{
	public Material[] mats;

	private int unscaledTimeVarId;

	private List<Material> adjustedMats = new List<Material>();

	public void Awake()
	{
		unscaledTimeVarId = Shader.PropertyToID("_UnscaledTime");
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			Material[] array = new Material[sharedMaterials.Length];
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				bool flag = false;
				for (int k = 0; k < mats.Length; k++)
				{
					if (sharedMaterials[j] == mats[k])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Material material = new Material(sharedMaterials[j]);
					adjustedMats.Add(material);
					array[j] = material;
				}
				else
				{
					array[j] = sharedMaterials[j];
				}
			}
			renderer.materials = array;
		}
	}

	public void Update()
	{
		foreach (Material adjustedMat in adjustedMats)
		{
			adjustedMat.SetFloat(unscaledTimeVarId, Time.unscaledTime);
		}
	}

	public void OnDestroy()
	{
		foreach (Material adjustedMat in adjustedMats)
		{
			Destroyer.Destroy(adjustedMat, "UpdateMaterialUnscaledTime.OnDestroy");
		}
		adjustedMats.Clear();
	}
}
