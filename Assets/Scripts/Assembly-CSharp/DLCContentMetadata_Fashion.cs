using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Content/Fashion Metadata")]
public class DLCContentMetadata_Fashion : DLCContentMetadata
{
	public GameObject prefab;

	public VacItemDefinition vacItemDefinition;

	public GadgetDefinition gadgetDefinition;

	public override void Register()
	{
		SRSingleton<GameContext>.Instance.LookupDirector.RegisterFashion(prefab, vacItemDefinition, gadgetDefinition);
		SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel().RegisterPotentialAmmo(PlayerState.AmmoMode.DEFAULT, prefab);
		SRSingleton<SceneContext>.Instance.GameModel.GetGadgetsModel().RegisterBlueprint(gadgetDefinition.id);
	}
}
