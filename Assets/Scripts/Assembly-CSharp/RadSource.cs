using System;
using UnityEngine;

public class RadSource : MonoBehaviour
{
	[Tooltip("Radiation to apply to the player per second while in this rad source. Stacks with other rad sources.")]
	public float radPerSecond = 1f;

	private PlayerRadAbsorber absorber;

	private double startTime;

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void OnDisable()
	{
		if (absorber != null)
		{
			ClearAbsorber();
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (!collider.isTrigger)
		{
			PlayerRadAbsorber component = collider.gameObject.GetComponent<PlayerRadAbsorber>();
			if (component != null)
			{
				absorber = component;
				startTime = timeDir.HoursFromNowOrStart(0f);
			}
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (!collider.isTrigger)
		{
			PlayerRadAbsorber component = collider.gameObject.GetComponent<PlayerRadAbsorber>();
			if (component != null && absorber == component)
			{
				ClearAbsorber();
			}
		}
	}

	private void ClearAbsorber()
	{
		absorber = null;
		int val = (int)Math.Floor((timeDir.WorldTime() - startTime) * 0.01666666753590107);
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.EXTENDED_RAD_EXPOSURE, val);
		}
	}

	public void FixedUpdate()
	{
		if (absorber != null)
		{
			absorber.Absorb(base.gameObject, radPerSecond * Time.fixedDeltaTime);
		}
	}
}
