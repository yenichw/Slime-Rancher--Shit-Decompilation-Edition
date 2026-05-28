using System.Collections.Generic;
using UnityEngine;

public class BoomMaterialAnimator : SRBehaviour
{
	public interface BoomMaterialInformer
	{
		float GetReadiness();

		float GetRecoveriness();
	}

	private Material[] boomMaterials;

	private BoomMaterialInformer boomSlime;

	private float lastReadiness = float.PositiveInfinity;

	private float lastRecoveriness = float.PositiveInfinity;

	private const string CRACK_AMOUNT_PROP = "_CrackAmount";

	private const string CHAR_PROP = "_Char";

	public void Awake()
	{
		List<Material> list = new List<Material>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.sharedMaterial.HasProperty("_CrackAmount"))
			{
				list.Add(renderer.material);
			}
		}
		boomMaterials = list.ToArray();
		boomSlime = GetComponent<BoomMaterialInformer>();
		Update();
	}

	public void Update()
	{
		float readiness = boomSlime.GetReadiness();
		float recoveriness = boomSlime.GetRecoveriness();
		if (Mathf.Abs(recoveriness - lastRecoveriness) >= 0.05f || Mathf.Abs(readiness - lastReadiness) >= 0.05f)
		{
			float num = ((recoveriness > 0.4f) ? 1f : (recoveriness * 2.5f));
			Material[] array = boomMaterials;
			foreach (Material obj in array)
			{
				obj.SetFloat("_CrackAmount", (1f - num) * Mathf.Lerp(0.1f, 1f, readiness));
				obj.SetFloat("_Char", num);
			}
			lastRecoveriness = recoveriness;
			lastReadiness = readiness;
		}
	}

	public void OnDestroy()
	{
		Material[] array = boomMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			Destroyer.Destroy(array[i], "BoomMaterialAnimator.OnDestroy");
		}
	}
}
