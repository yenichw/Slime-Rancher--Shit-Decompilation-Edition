using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Content/Shader Materials Metadata")]
public class DLCContentMetadata_MaterialList : DLCContentMetadata
{
	[Tooltip("The set of added materials that can be cloaked.")]
	public Material[] cloakableMaterials;

	public override void Register()
	{
		SRSingleton<GameContext>.Instance.SlimeShaders.RegisterAdditionalMaterials(cloakableMaterials);
	}
}
