using System.Collections.Generic;
using UnityEngine;

public class HydroShower : SRBehaviour, GadgetInteractor
{
	public Identifiable.Id liquidId = Identifiable.Id.WATER_LIQUID;

	public FilteredTrackCollisions tracker;

	public GameObject showerFX;

	[Tooltip("How long we should keep the shower going in game-minutes")]
	public float waterDuration = 5f;

	[Tooltip("How long between activations in game-minutes")]
	public float timeBetweenShowers = 60f;

	public float wateringUnitsPerPulse = 0.5f;

	[Tooltip("How long between pulses in game-minutes")]
	public float pulseDelay = 0.5f;

	private double waterUntil;

	private double nextWaterPulse;

	private double nextShower;

	private TimeDirector timeDir;

	private WaitForChargeup waiter;

	public void Awake()
	{
		waiter = GetComponentInParent<WaitForChargeup>();
		tracker.SetFilter(IsTarr);
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void Update()
	{
		if (waiter.IsWaiting())
		{
			return;
		}
		if (timeDir.HasReached(waterUntil))
		{
			showerFX.SetActive(value: false);
			nextWaterPulse = double.PositiveInfinity;
			return;
		}
		if (timeDir.HasReached(nextWaterPulse))
		{
			DoWaterPulse();
			nextWaterPulse = timeDir.HoursFromNow(pulseDelay * (1f / 60f));
		}
		showerFX.SetActive(value: true);
	}

	public void OnInteract()
	{
		if (timeDir.HasReached(nextShower))
		{
			waterUntil = timeDir.HoursFromNow(waterDuration * (1f / 60f));
			nextWaterPulse = 0.0;
			nextShower = timeDir.HoursFromNow(timeBetweenShowers * (1f / 60f));
		}
	}

	public bool CanInteract()
	{
		return true;
	}

	private bool IsTarr(GameObject gameObj)
	{
		return Identifiable.IsTarr(Identifiable.GetId(gameObj));
	}

	private void DoWaterPulse()
	{
		HashSet<GameObject> hashSet = tracker.CurrColliders();
		if (hashSet.Count <= 0)
		{
			return;
		}
		HashSet<LiquidConsumer> hashSet2 = new HashSet<LiquidConsumer>();
		foreach (GameObject item2 in hashSet)
		{
			if (item2 != null)
			{
				LiquidConsumer[] componentsInParent = item2.GetComponentsInParent<LiquidConsumer>();
				foreach (LiquidConsumer item in componentsInParent)
				{
					hashSet2.Add(item);
				}
			}
		}
		foreach (LiquidConsumer item3 in hashSet2)
		{
			item3.AddLiquid(liquidId, wateringUnitsPerPulse);
		}
	}
}
