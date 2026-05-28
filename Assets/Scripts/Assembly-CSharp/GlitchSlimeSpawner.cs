using MonomiPark.SlimeRancher.Serializable.Optional;
using UnityEngine;

public class GlitchSlimeSpawner : DirectedSlimeSpawner
{
	[Tooltip("If enabled, overrides GlitchMetadata.dittoStandard.probability.")]
	public Float probablityStandard;

	[Tooltip("If enabled, overrides GlitchMetadata.dittoLargo.probability.")]
	public Float probablityLargo;

	private GlitchMetadata metadata;

	public override void Awake()
	{
		base.Awake();
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
	}

	protected override void OnActorSpawned(GameObject instance)
	{
		base.OnActorSpawned(instance);
		GlitchSlime component = instance.GetComponent<GlitchSlime>();
		if (component != null && Randoms.SHARED.GetProbability(metadata.GetDittoProbability(Identifiable.GetId(instance), this)))
		{
			component.enabled = true;
		}
	}
}
