using System;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchSlime : SRBehaviour, LiquidConsumer, ActorModel.Participant
{
	private SlimeModel model;

	private RegionMember regionMember;

	private Identifiable.Id id;

	public void Awake()
	{
		regionMember = GetComponent<RegionMember>();
		id = Identifiable.GetId(base.gameObject);
		base.enabled = false;
	}

	public void InitModel(ActorModel model)
	{
		((SlimeModel)model).isGlitch = false;
	}

	public void SetModel(ActorModel model)
	{
		this.model = (SlimeModel)model;
		base.enabled = this.model.isGlitch;
	}

	public void Start()
	{
		model.isGlitch = true;
		GetRequiredComponent<SlimeFaceAnimator>().SetGlitch();
		Vacuumable requiredComponent = GetRequiredComponent<Vacuumable>();
		requiredComponent.onSetHeld += delegate
		{
			OnExposed();
		};
		requiredComponent.consume += delegate
		{
			OnExposed();
		};
		SlimeHealth requiredComponent2 = GetRequiredComponent<SlimeHealth>();
		requiredComponent2.onDamage = (SlimeHealth.OnDamage)Delegate.Combine(requiredComponent2.onDamage, (SlimeHealth.OnDamage)delegate(GameObject s)
		{
			OnExposed(s);
		});
	}

	public void AddLiquid(Identifiable.Id id, float units)
	{
		if (base.enabled && id == Identifiable.Id.GLITCH_DEBUG_SPRAY_LIQUID)
		{
			OnExposed();
		}
	}

	private void OnExposed(GameObject source = null)
	{
		Destroyer.DestroyActor(base.gameObject, "GlitchSlime.OnExposed");
		SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.GetDittoExposureMetadata(id).OnExposed(base.gameObject, null, null, null, source);
	}
}
