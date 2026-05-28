using System.Collections.Generic;
using UnityEngine;

public class SlimeShaders : SRBehaviour
{
	public Material cloakMaterial;

	[Tooltip("The default whitelist of materials that can be cloaked.")]
	public Material[] defaultCloakableMaterials;

	public HashSet<Shader> cloakableShaders = new HashSet<Shader>();

	private int AlphaPropertyId;

	private int unscaledTimePropertyId;

	public void Awake()
	{
		unscaledTimePropertyId = Shader.PropertyToID("UnscaledTime");
		Material[] array = defaultCloakableMaterials;
		foreach (Material material in array)
		{
			cloakableShaders.Add(material.shader);
		}
	}

	public void Update()
	{
		Shader.SetGlobalFloat(unscaledTimePropertyId, Time.unscaledTime);
	}

	public void RegisterAdditionalMaterials(Material[] materials)
	{
		foreach (Material material in materials)
		{
			cloakableShaders.Add(material.shader);
		}
	}
}
