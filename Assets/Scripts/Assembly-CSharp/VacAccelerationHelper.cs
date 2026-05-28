using System;
using UnityEngine;

public class VacAccelerationHelper
{
	public readonly float minFactor;

	public readonly float maxFactor;

	public readonly float speed;

	public readonly float duration;

	private float timeBegin;

	private float timeEnd;

	public float Factor
	{
		get
		{
			if (Time.time < timeEnd)
			{
				return Math.Min(minFactor + (Time.time - timeBegin) * speed, maxFactor);
			}
			return minFactor;
		}
	}

	public static VacAccelerationHelper CreateInput()
	{
		return new VacAccelerationHelper(1f, 3f, 0.5f, 1f);
	}

	public static VacAccelerationHelper CreateOutput()
	{
		return new VacAccelerationHelper(1f, 1.75f, 0.5f, 1f);
	}

	public VacAccelerationHelper(float minFactor, float maxFactor, float speed, float duration)
	{
		this.minFactor = minFactor;
		this.maxFactor = maxFactor;
		this.speed = speed;
		this.duration = duration;
	}

	public void OnTriggered()
	{
		timeBegin = ((Time.time >= timeEnd) ? Time.time : timeBegin);
		timeEnd = Time.time + duration;
	}
}
