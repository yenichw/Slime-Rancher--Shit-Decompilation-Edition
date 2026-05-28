using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceTracker : MonoBehaviour
{
	private float frameCount;

	private float dt;

	private float fps;

	private float updateRate = 1f;

	private double fpsSum;

	private int fpsCount;

	private long maxHeapSize;

	private const int BYTES_PER_MEG = 1048576;

	public void Update()
	{
		frameCount += 1f;
		dt += Time.deltaTime;
		if (dt > updateRate)
		{
			fps = frameCount / dt;
			frameCount = 0f;
			dt -= updateRate;
			fpsSum += fps;
			fpsCount++;
		}
		maxHeapSize = Math.Max(maxHeapSize, Profiler.usedHeapSizeLong);
	}

	public void OnApplicationQuit()
	{
		if (fpsCount > 0)
		{
			AnalyticsUtil.CustomEvent("PerfSummary", new Dictionary<string, object>
			{
				{
					"meanFps",
					(int)Math.Round(fpsSum / (double)fpsCount)
				},
				{
					"maxMem",
					maxHeapSize / 1048576
				}
			}, includeDefaultEventData: false);
		}
	}
}
