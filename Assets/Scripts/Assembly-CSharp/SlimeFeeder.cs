using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.UI;

public class SlimeFeeder : SRBehaviour, LandPlotModel.Participant
{
	public enum FeedSpeed
	{
		Normal = 0,
		Slow = 1,
		Fast = 2
	}

	public Dictionary<FeedSpeed, float> hoursByFeedSpeed = new Dictionary<FeedSpeed, float>
	{
		{
			FeedSpeed.Slow,
			9f
		},
		{
			FeedSpeed.Normal,
			6f
		},
		{
			FeedSpeed.Fast,
			3f
		}
	};

	public int itemsPerFeeding = 6;

	public GameObject spawnFX;

	public GameObject feederSpeedUI;

	public Image feederIcon;

	public Sprite slowIcon;

	public Sprite normalIcon;

	public Sprite fastIcon;

	private SiloStorage storage;

	private TimeDirector timeDir;

	private float nextEject;

	private Region region;

	private LandPlotModel model;

	private const float EJECT_DIST = 0.5f;

	private const float EJECT_FORCE = 500f;

	private const float EJECT_NOISE = 400f;

	private const float EJECT_RATE = 0.5f;

	public void Awake()
	{
		storage = GetComponentInParent<SiloStorage>();
		region = GetComponentInParent<Region>();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		SetFeederSpeedIcon(FeedSpeed.Normal);
	}

	public void InitModel(LandPlotModel model)
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		model.nextFeedingTime = timeDir.HoursFromNowOrStart(CalcFeedingCycleHours(model.feederCycleSpeed));
	}

	public void SetModel(LandPlotModel model)
	{
		this.model = model;
		SetFeederSpeedIcon(model.feederCycleSpeed);
	}

	public void Update()
	{
		UpdateToTime(timeDir.WorldTime());
		if (ShouldFeed())
		{
			ProcessFeedOperation(ejectFood: true);
		}
	}

	public void UpdateToTime(double worldTime)
	{
		while (TimeUtil.HasReached(worldTime, model.nextFeedingTime))
		{
			model.remainingFeedOperations += itemsPerFeeding;
			model.nextFeedingTime += 3600f * CalcFeedingCycleHours();
		}
	}

	private bool ShouldFeed()
	{
		if (model.remainingFeedOperations > 0 && Time.time > nextEject)
		{
			return !region.Hibernated;
		}
		return false;
	}

	private void ProcessFeedOperation(bool ejectFood)
	{
		Ammo relevantAmmo = storage.GetRelevantAmmo();
		relevantAmmo.SetAmmoSlot(0);
		if (relevantAmmo.HasSelectedAmmo())
		{
			if (ejectFood)
			{
				EjectFood(relevantAmmo);
			}
			relevantAmmo.DecrementSelectedAmmo();
		}
		model.remainingFeedOperations = Math.Max(0, model.remainingFeedOperations - 1);
	}

	private void EjectFood(Ammo storageAmmo)
	{
		GameObject gameObject = SRBehaviour.InstantiateActor(storageAmmo.GetSelectedStored(), region.setId, base.transform.position + base.transform.forward * 0.5f, base.transform.rotation);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		component.AddForce((base.transform.forward * 500f + UnityEngine.Random.insideUnitSphere * 400f) * component.mass);
		nextEject = Time.time + 0.5f;
		if (spawnFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(spawnFX, gameObject.transform.position, base.transform.rotation);
		}
	}

	public Identifiable.Id GetFoodId()
	{
		return storage.GetRelevantAmmo().GetSelectedId();
	}

	public int GetFoodCount()
	{
		return storage.GetRelevantAmmo().GetSlotCount(0);
	}

	public int RemainingFeedOperationsFastForward()
	{
		return Mathf.Min(model.remainingFeedOperations, GetFoodCount());
	}

	public void ProcessFeedOperationFastForward()
	{
		ProcessFeedOperation(ejectFood: false);
	}

	public void SetFeederSpeed(FeedSpeed speed)
	{
		model.feederCycleSpeed = speed;
		SetFeederSpeedIcon(speed);
	}

	private void SetFeederSpeedIcon(FeedSpeed speed)
	{
		switch (speed)
		{
		case FeedSpeed.Slow:
			feederIcon.sprite = slowIcon;
			break;
		case FeedSpeed.Normal:
			feederIcon.sprite = normalIcon;
			break;
		case FeedSpeed.Fast:
			feederIcon.sprite = fastIcon;
			break;
		}
	}

	public void StepFeederSpeed()
	{
		switch (model.feederCycleSpeed)
		{
		case FeedSpeed.Slow:
			SetFeederSpeed(FeedSpeed.Normal);
			break;
		case FeedSpeed.Normal:
			SetFeederSpeed(FeedSpeed.Fast);
			break;
		case FeedSpeed.Fast:
			SetFeederSpeed(FeedSpeed.Slow);
			break;
		}
	}

	public FeedSpeed GetFeedingCycleSpeed()
	{
		return model.feederCycleSpeed;
	}

	private float CalcFeedingCycleHours()
	{
		return CalcFeedingCycleHours(model.feederCycleSpeed);
	}

	private float CalcFeedingCycleHours(FeedSpeed speed)
	{
		return hoursByFeedSpeed[speed];
	}
}
