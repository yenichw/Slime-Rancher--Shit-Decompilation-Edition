using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RanchData : DataModule<RanchData>
{
	[Serializable]
	public class LandPlotData
	{
		public Vector3 pos;

		public Vector3 rot;

		public LandPlot.Id id;

		public List<LandPlot.Upgrade> upgrades;

		public SpawnResource.Id attachedId;

		public float attachedDeathTime;

		public Dictionary<SiloStorage.StorageType, Ammo.AmmoData[]> siloAmmo = new Dictionary<SiloStorage.StorageType, Ammo.AmmoData[]>();

		public float feederNextTime;

		public int feederPendingCount;

		public float collectorNextTime;

		public float fastforwarderDisableTime;
	}

	public const int CURR_FORMAT_ID = 3;

	private LandPlotData[] plots;

	private Dictionary<Vector3, AccessDoor.State> accessDoorStates;

	private const float MAX_DIST_MATCH = 5f;

	private const float MAX_DIST_MATCH_SQR = 25f;

	public LandPlotData[] GetPlots()
	{
		return plots;
	}

	public Dictionary<Vector3, AccessDoor.State> GetAccessDoorStates()
	{
		return accessDoorStates;
	}

	public static void AssertEquals(RanchData dataA, RanchData dataB)
	{
		if (dataA.plots.Length == dataB.plots.Length)
		{
			for (int i = 0; i < dataA.plots.Length; i++)
			{
				AssertEqualPlots(dataA.plots[i], dataB.plots[i]);
			}
		}
	}

	private static void AssertEqualPlots(LandPlotData plotA, LandPlotData plotB)
	{
		new TestUtil.Vector3Comparer();
	}
}
