using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialStealthController
{
	private const float CLOAK_THRESHOLD = 0.99f;

	private static readonly int alphaPropertyId = Shader.PropertyToID("_Alpha");

	private static readonly int topColorPropertyId = Shader.PropertyToID("_TopColor");

	private static readonly int middleColorPropertyId = Shader.PropertyToID("_MiddleColor");

	private static readonly int bottomColorPropertyId = Shader.PropertyToID("_BottomColor");

	private readonly Material cloakMaterial;

	private readonly HashSet<Material> cloakingMats = new HashSet<Material>();

	private readonly List<Renderer> renderers = new List<Renderer>();

	private readonly Dictionary<Renderer, Material[]> rendererOriginalMaterials = new Dictionary<Renderer, Material[]>();

	private SlimeShaders slimeShaders;

	private readonly MaterialPropertyBlock colorsPropertyBlock = new MaterialPropertyBlock();

	public MaterialStealthController(GameObject gameObject)
	{
		slimeShaders = SRSingleton<GameContext>.Instance.SlimeShaders;
		cloakMaterial = slimeShaders.cloakMaterial;
		UpdateMaterials(gameObject);
	}

	public void UpdateMaterials(GameObject gameObject)
	{
		cloakingMats.Clear();
		renderers.Clear();
		rendererOriginalMaterials.Clear();
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (material != null && slimeShaders.cloakableShaders.Contains(material.shader))
				{
					cloakingMats.Add(material);
					if (material.HasProperty(topColorPropertyId))
					{
						colorsPropertyBlock.SetColor(topColorPropertyId, material.GetColor(topColorPropertyId));
						colorsPropertyBlock.SetColor(middleColorPropertyId, material.GetColor(middleColorPropertyId));
						colorsPropertyBlock.SetColor(bottomColorPropertyId, material.GetColor(bottomColorPropertyId));
					}
				}
			}
			renderers.Add(renderer);
			rendererOriginalMaterials[renderer] = renderer.sharedMaterials.ToArray();
		}
		cloakingMats.Add(cloakMaterial);
	}

	public void SetOpacity(float opacity)
	{
		bool flag = opacity >= 0.99f;
		bool flag2 = false;
		for (int i = 0; i < renderers.Count; i++)
		{
			Renderer renderer2 = renderers[i];
			if (renderer2 == null)
			{
				flag2 = true;
				continue;
			}
			Material[] sharedMaterials = renderer2.sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				Material material = sharedMaterials[j];
				if (cloakingMats.Contains(material))
				{
					if (!flag && material != cloakMaterial)
					{
						sharedMaterials[j] = cloakMaterial;
					}
					else if (flag && material == cloakMaterial)
					{
						sharedMaterials[j] = rendererOriginalMaterials[renderer2][j];
					}
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					renderer2.GetPropertyBlock(materialPropertyBlock, j);
					materialPropertyBlock.SetFloat(alphaPropertyId, flag ? 1f : opacity);
					materialPropertyBlock.SetColor(topColorPropertyId, colorsPropertyBlock.GetColor(topColorPropertyId));
					materialPropertyBlock.SetColor(middleColorPropertyId, colorsPropertyBlock.GetColor(middleColorPropertyId));
					materialPropertyBlock.SetColor(bottomColorPropertyId, colorsPropertyBlock.GetColor(bottomColorPropertyId));
					renderer2.SetPropertyBlock(materialPropertyBlock, j);
				}
			}
			renderer2.sharedMaterials = sharedMaterials;
		}
		if (flag2)
		{
			renderers.RemoveAll((Renderer renderer) => renderer == null);
		}
	}
}
