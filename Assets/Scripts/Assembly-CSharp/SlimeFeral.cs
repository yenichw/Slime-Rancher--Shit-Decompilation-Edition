using System;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeFeral : RegisteredActorBehaviour, RegistryUpdateable, ActorModel.Participant
{
	[Tooltip("The aura we use to indicate when a slime has gone feral.")]
	public GameObject auraPrefab;

	[Tooltip("Whether the feralness of the slime can be made true on the fly")]
	public bool dynamicToFeral;

	[Tooltip("Whether the feralness of the slime can be made false on the fly")]
	public bool dynamicFromFeral = true;

	[Tooltip("Hours after which a feral should poof.")]
	public float feralLifetimeHours = 3f;

	[Tooltip("The FX to play when ferality causes us to poof.")]
	public GameObject destroyFX;

	private SlimeEmotions emotions;

	private RegionMember member;

	private GameObject aura;

	private TimeDirector timeDir;

	private SlimeModel model;

	private double expireAt = double.PositiveInfinity;

	private const float AGITATION_FERAL_TRIGGER = 0.999f;

	private const float DEFERAL_AGITATION_ADJUST = -0.5f;

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		member = GetComponent<RegionMember>();
		SlimeEat component = GetComponent<SlimeEat>();
		component.onEat = (SlimeEat.OnEatDelegate)Delegate.Combine(component.onEat, new SlimeEat.OnEatDelegate(DidEat));
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		if (GetComponent<Vacuumable>().size == Vacuumable.Size.NORMAL)
		{
			Destroyer.Destroy(this, "SlimeFeral.Awake");
		}
	}

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(ActorModel model)
	{
		this.model = (SlimeModel)model;
		if (this.model.isFeral)
		{
			MakeFeral();
		}
		else
		{
			MakeNotFeral(deagitate: false);
		}
	}

	public void RegistryUpdate()
	{
		if (dynamicToFeral && !model.isFeral && emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) >= 0.999f)
		{
			SetFeral();
		}
		if (!timeDir.HasReached(expireAt))
		{
			return;
		}
		if (CellDirector.IsOnRanch(member) || CellDirector.IsInWilds(member))
		{
			expireAt += 3600.0;
			return;
		}
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, Quaternion.identity);
		}
		Destroyer.DestroyActor(base.gameObject, "SlimeFeral.RegistryUpdate");
	}

	public void DidEat(Identifiable.Id id)
	{
		if (dynamicFromFeral && model.isFeral && id != Identifiable.Id.PLAYER)
		{
			ClearFeral();
		}
	}

	public void SetFeral()
	{
		if (!model.isFeral)
		{
			MakeFeral();
		}
	}

	private void MakeFeral()
	{
		AttackPlayer component = GetComponent<AttackPlayer>();
		if (component != null)
		{
			component.shouldAttackPlayer = true;
		}
		GotoPlayer component2 = GetComponent<GotoPlayer>();
		if (component2 != null)
		{
			component2.shouldGotoPlayer = true;
		}
		FindConsumable[] components = GetComponents<FindConsumable>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].UpdateSearchIds();
		}
		aura = UnityEngine.Object.Instantiate(auraPrefab);
		aura.transform.SetParent(base.transform, worldPositionStays: false);
		GetComponent<SlimeFaceAnimator>().SetFeral();
		model.isFeral = true;
		expireAt = timeDir.HoursFromNowOrStart(feralLifetimeHours);
	}

	public void ClearFeral(bool deagitate = false)
	{
		if (model.isFeral)
		{
			MakeNotFeral(deagitate);
		}
	}

	private void MakeNotFeral(bool deagitate)
	{
		SlimeAudio component = GetComponent<SlimeAudio>();
		if (component != null)
		{
			component.Play(component.slimeSounds.unferalCue);
		}
		AttackPlayer component2 = GetComponent<AttackPlayer>();
		if (component2 != null)
		{
			component2.shouldAttackPlayer = false;
		}
		GotoPlayer component3 = GetComponent<GotoPlayer>();
		if (component3 != null)
		{
			component3.shouldGotoPlayer = false;
		}
		FindConsumable[] components = GetComponents<FindConsumable>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].UpdateSearchIds();
		}
		Destroyer.Destroy(aura, "SlimeFeral.ClearFeral");
		GetComponent<SlimeFaceAnimator>().ClearFeral();
		model.isFeral = false;
		if (deagitate)
		{
			emotions.Adjust(SlimeEmotions.Emotion.AGITATION, -0.5f);
		}
		expireAt = double.PositiveInfinity;
	}

	public bool IsFeral()
	{
		return model.isFeral;
	}
}
