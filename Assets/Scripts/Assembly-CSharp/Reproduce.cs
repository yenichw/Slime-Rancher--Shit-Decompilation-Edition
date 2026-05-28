using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class Reproduce : RegisteredActorBehaviour, RegistryUpdateable, ActorModel.Participant
{
	public Identifiable nearMateId;

	public float maxDistToMate = 10f;

	public Identifiable[] densityIds;

	public float densityDist = 10f;

	public int maxDensity = 10;

	public GameObject childPrefab;

	public GameObject produceFX;

	public float minReproduceGameHours = 6f;

	public float maxReproduceGameHours = 12f;

	public float deluxeDensityFactor = 2f;

	private TimeDirector timeDir;

	private RegionMember regionMember;

	private AnimalModel model;

	private const float NON_COOP_REPRO_PROB = 0.5f;

	private List<GameObject> nearbyGameObjects = new List<GameObject>();

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		regionMember = GetComponent<RegionMember>();
	}

	public void InitModel(ActorModel model)
	{
		((AnimalModel)model).nextReproduceTime = timeDir.HoursFromNowOrStart(ReproducePeriod());
	}

	public void SetModel(ActorModel model)
	{
		this.model = (AnimalModel)model;
	}

	public void RegistryUpdate()
	{
		if (!timeDir.HasReached(model.nextReproduceTime))
		{
			return;
		}
		GameObject gameObject = NearMateAndSparseEnough();
		if (ShouldReproduce() && gameObject != null)
		{
			CreateChick();
			if (WithinVitamizer() && Randoms.SHARED.GetProbability(0.5f))
			{
				CreateChick();
			}
			TransformChanceOnReproduce component = GetComponent<TransformChanceOnReproduce>();
			TransformChanceOnReproduce component2 = gameObject.GetComponent<TransformChanceOnReproduce>();
			if (component != null)
			{
				component.MaybeTransform();
			}
			if (component2 != null)
			{
				component2.MaybeTransform();
			}
		}
		model.nextReproduceTime = timeDir.HoursFromNow(ReproducePeriod());
	}

	private GameObject CreateChick()
	{
		GameObject obj = SRBehaviour.InstantiateActor(childPrefab, regionMember.setId, base.transform.position, base.transform.rotation);
		EggActivator component = obj.GetComponent<EggActivator>();
		if (component != null)
		{
			component.AddEgg();
		}
		return obj;
	}

	private bool ShouldReproduce()
	{
		if (!WithinCoop())
		{
			return Randoms.SHARED.GetProbability(0.5f);
		}
		return true;
	}

	private bool WithinCoop()
	{
		return CoopRegion.IsWithin(base.transform.position);
	}

	private bool WithinDeluxeCoop()
	{
		return CoopRegion.IsWithinDeluxe(base.transform.position);
	}

	private bool WithinVitamizer()
	{
		return VitamizerRegion.IsWithin(base.transform.position);
	}

	private float ReproducePeriod()
	{
		if (maxReproduceGameHours < minReproduceGameHours)
		{
			throw new InvalidOperationException("Invalid reproduce periods. min:" + minReproduceGameHours + " max: " + maxReproduceGameHours);
		}
		return Randoms.SHARED.GetInRange(minReproduceGameHours, maxReproduceGameHours);
	}

	private int MaxDensity()
	{
		if (!WithinDeluxeCoop())
		{
			return maxDensity;
		}
		return Mathf.RoundToInt((float)maxDensity * deluxeDensityFactor);
	}

	private GameObject NearMateAndSparseEnough()
	{
		Vector3 position = base.transform.position;
		float num = maxDistToMate * maxDistToMate;
		nearbyGameObjects.Clear();
		CellDirector.Get(nearMateId.id, regionMember, nearbyGameObjects);
		GameObject gameObject = null;
		for (int i = 0; i < nearbyGameObjects.Count; i++)
		{
			GameObject gameObject2 = nearbyGameObjects[i];
			if ((gameObject2.transform.position - position).sqrMagnitude <= num)
			{
				gameObject = gameObject2;
				break;
			}
		}
		nearbyGameObjects.Clear();
		if (gameObject == null)
		{
			return gameObject;
		}
		int num2 = 0;
		float num3 = densityDist * densityDist;
		int num4 = MaxDensity();
		for (int j = 0; j < densityIds.Length; j++)
		{
			if (num2 > num4)
			{
				break;
			}
			Identifiable obj = densityIds[j];
			nearbyGameObjects.Clear();
			CellDirector.Get(obj.id, regionMember, nearbyGameObjects);
			for (int k = 0; k < nearbyGameObjects.Count; k++)
			{
				if (num2 > num4)
				{
					break;
				}
				if ((nearbyGameObjects[k].transform.position - position).sqrMagnitude <= num3)
				{
					num2++;
				}
			}
		}
		nearbyGameObjects.Clear();
		if (num2 > num4)
		{
			return null;
		}
		return gameObject;
	}
}
