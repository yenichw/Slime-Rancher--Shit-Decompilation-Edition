using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Content/Slime Appearance Metadata")]
public class DLCContentMetadata_SlimeAppearance : DLCContentMetadata
{
	[Tooltip("SlimeDefinition to add the appearance to.")]
	public SlimeDefinition definition;

	[Tooltip("SlimeAppearance to add to the definition.")]
	public SlimeAppearance appearance;

	public override void Register()
	{
		definition.RegisterDynamicAppearance(appearance);
	}
}
