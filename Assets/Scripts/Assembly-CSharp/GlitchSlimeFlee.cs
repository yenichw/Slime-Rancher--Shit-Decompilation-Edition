using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GlitchSlimeFlee : SlimeFlee, ActorModel.Participant
{
	private GlitchSlimeModel model;

	private GlitchMetadata metadata;

	private bool requiresForceBlockCheck = true;

	public override void Awake()
	{
		base.Awake();
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		plexer.activationDelayOverride = metadata.slimeFleeDelay;
		vacuumable.onSetLaunched += OnSetLaunched;
		SetFleeDirection(Quaternion.Euler(0f, Randoms.SHARED.GetInRange(0, 360), 0f) * Vector3.one);
	}

	public void InitModel(ActorModel model)
	{
		GlitchSlimeModel obj = (GlitchSlimeModel)model;
		obj.deathTime = timeDir.HoursFromNow(metadata.slimeLifetime.GetRandom() * (1f / 60f));
		obj.exposureChance = metadata.slimeBaseExposureChance;
	}

	public void SetModel(ActorModel model)
	{
		this.model = (GlitchSlimeModel)model;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		vacuumable.onSetLaunched -= OnSetLaunched;
	}

	public override void Action()
	{
		if (timeDir.HasReached(model.deathTime))
		{
			if (Randoms.SHARED.GetProbability(model.exposureChance))
			{
				metadata.slimeExposure.OnExposed(base.gameObject, null, null, null, null, delegate(GameObject instance)
				{
					instance.GetRequiredComponent<GlitchSlimeFlee>().model.exposureChance = model.exposureChance * (1f - metadata.slimeExposureChanceDegradation);
				});
			}
			SRBehaviour.SpawnAndPlayFX(disappearFX, base.transform.position, base.transform.rotation);
			Destroyer.DestroyActor(base.gameObject, "GlitchSlimeFlee.Action");
		}
		else
		{
			if (plexer.IsBlocked(null, base.fleeDir.Value, 0, requiresForceBlockCheck))
			{
				SetFleeDirection(Quaternion.Euler(0f, Randoms.SHARED.GetInRange(90, 270), 0f) * base.fleeDir.Value);
				requiresForceBlockCheck = true;
			}
			else
			{
				requiresForceBlockCheck = false;
			}
			MoveTowards(base.fleeDir.Value);
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (base.fleeDir.HasValue)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(base.transform.position, base.transform.position + base.fleeDir.Value * 1.5f);
		}
	}

	public void DisableExposureChance()
	{
		model.exposureChance = 0f;
	}

	private void OnSetLaunched(bool launched)
	{
		if (launched)
		{
			DisableExposureChance();
		}
	}
}
