using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class TransformAfterTime : SRBehaviour, ActorModel.Participant
{
	[Serializable]
	public class TransformOpt
	{
		public GameObject targetPrefab;

		public float weight;
	}

	public float delayGameHours = 6f;

	public GameObject transformFX;

	public List<TransformOpt> options;

	private TimeDirector timeDir;

	private List<FeederRegion> feeders = new List<FeederRegion>();

	private double lastWorldTime;

	private AnimalModel model;

	private RegionMember regionMember;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		regionMember = GetComponent<RegionMember>();
		lastWorldTime = timeDir.HoursFromNowOrStart(0f);
	}

	public void InitModel(ActorModel model)
	{
		((AnimalModel)model).transformTime = timeDir.HoursFromNowOrStart(delayGameHours);
	}

	public void SetModel(ActorModel model)
	{
		this.model = (AnimalModel)model;
	}

	public void Update()
	{
		if (feeders.Count > 0)
		{
			model.transformTime -= (timeDir.WorldTime() - lastWorldTime) * 1.0;
		}
		if (timeDir.HasReached(model.transformTime) && options.Count > 0)
		{
			Dictionary<GameObject, float> dictionary = new Dictionary<GameObject, float>();
			foreach (TransformOpt option in options)
			{
				dictionary[option.targetPrefab] = option.weight;
			}
			SRBehaviour.SpawnAndPlayFX(transformFX, base.transform.position, base.transform.rotation);
			Destroyer.DestroyActor(base.gameObject, "TransformAfterTime.Update");
			SRBehaviour.InstantiateActor(Randoms.SHARED.Pick(dictionary, null), regionMember.setId, base.transform.position, base.transform.rotation);
		}
		lastWorldTime = timeDir.WorldTime();
	}

	public void AddFeeder(FeederRegion feeder)
	{
		feeders.Add(feeder);
	}

	public void RemoveFeeder(FeederRegion feeder)
	{
		feeders.Remove(feeder);
	}
}
