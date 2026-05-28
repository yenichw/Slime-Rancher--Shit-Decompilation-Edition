using System.Collections.Generic;
using UnityEngine;

public class DestroyOutsideHoursOfDay : SRBehaviour, CaveTrigger.Listener
{
	public float startHour = 18f;

	public float endHour = 6f;

	public float minEndureHoursOutsideWindow = 0.5f;

	public float maxEndureHoursOutsideWindow = 1.5f;

	public bool cavesPreventShutdown = true;

	public GameObject destroyFX;

	private TimeDirector timeDir;

	private double shutdownAt;

	private HashSet<GameObject> caves = new HashSet<GameObject>();

	private bool waitForPhysicsUpdate;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		StartShutdownClock();
	}

	public void OnEnable()
	{
		waitForPhysicsUpdate = true;
	}

	public void FixedUpdate()
	{
		waitForPhysicsUpdate = false;
	}

	public void Update()
	{
		if (waitForPhysicsUpdate)
		{
			return;
		}
		if (timeDir.HasReached(shutdownAt))
		{
			if (destroyFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
			}
			Destroyer.DestroyActor(base.gameObject, "DestroyOutsideHoursOfDay.Update");
		}
		if (caves.Count > 0)
		{
			UnityWorkarounds.SafeRemoveAllNulls(caves);
			if (caves.Count == 0)
			{
				StartShutdownClock();
			}
		}
	}

	public void OnCaveEnter(GameObject caveObj, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		if (caves.Count == 0)
		{
			StopShutdownClock();
		}
		caves.Add(caveObj);
	}

	public void OnCaveExit(GameObject caveObj, bool affectLighting, AmbianceDirector.Zone caveZone)
	{
		caves.Remove(caveObj);
		if (caves.Count == 0)
		{
			StartShutdownClock();
		}
	}

	private void StartShutdownClock()
	{
		float num = timeDir.CurrHourOrStart();
		float inRange = Randoms.SHARED.GetInRange(minEndureHoursOutsideWindow, maxEndureHoursOutsideWindow);
		float num2 = num + inRange;
		if (num2 > 24f)
		{
			num2 %= 24f;
		}
		if (endHour >= startHour)
		{
			if ((num < startHour || num > endHour) && (num2 < startHour || num2 > endHour))
			{
				shutdownAt = timeDir.HoursFromNowOrStart(inRange);
			}
			else
			{
				shutdownAt = timeDir.GetNextHour(endHour) + (double)(inRange * 3600f);
			}
		}
		else if (num > endHour && num < startHour && num2 > endHour && num2 < startHour)
		{
			shutdownAt = timeDir.HoursFromNowOrStart(inRange);
		}
		else
		{
			shutdownAt = timeDir.GetNextHour(endHour) + (double)(inRange * 3600f);
		}
	}

	private void StopShutdownClock()
	{
		shutdownAt = double.PositiveInfinity;
	}
}
