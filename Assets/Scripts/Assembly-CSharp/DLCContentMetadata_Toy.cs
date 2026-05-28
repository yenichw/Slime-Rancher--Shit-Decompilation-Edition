using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Content/Toy Metadata")]
public class DLCContentMetadata_Toy : DLCContentMetadata
{
	public GameObject prefab;

	public ToyDefinition toyDefinition;

	public override void Register()
	{
		SRSingleton<GameContext>.Instance.LookupDirector.RegisterToy(toyDefinition, prefab);
		SRSingleton<GameContext>.Instance.ToyDirector.Register(toyDefinition.ToyId);
	}
}
