using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class MaybeCullOnReenable : SRBehaviour, ActorModel.Participant
{
	private SlimeEmotions emotions;

	private SlimeModel model;

	public void Awake()
	{
		emotions = GetComponent<SlimeEmotions>();
		if (!base.enabled)
		{
			model.disabledAtTime = SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNowOrStart(0f);
		}
	}

	public void Start()
	{
		if (base.enabled)
		{
			DoCullCheck();
		}
	}

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(ActorModel model)
	{
		this.model = (SlimeModel)model;
	}

	public void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && model != null)
		{
			model.disabledAtTime = SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNowOrStart(0f);
		}
	}

	public void OnEnable()
	{
		DoCullCheck();
	}

	private void DoCullCheck()
	{
		if (model != null)
		{
			if (model.disabledAtTime.HasValue && !CellDirector.IsOnRanch(GetComponent<RegionMember>()))
			{
				double num = SRSingleton<SceneContext>.Instance.TimeDirector.WorldTime() - model.disabledAtTime.Value;
				MaybeDestroy((float)(num / 86400.0));
			}
			model.disabledAtTime = null;
		}
	}

	private void MaybeDestroy(float daysPassed)
	{
		float p = Mathf.Pow(DestroyProbabilityPerDay(), 1f / daysPassed);
		if (Randoms.SHARED.GetProbability(p))
		{
			Destroyer.DestroyActor(base.gameObject, "MaybeCullOnReenable.MaybeDestroy");
		}
	}

	private float DestroyProbabilityPerDay()
	{
		return 0.5f + emotions.GetMax() * 0.4f;
	}
}
