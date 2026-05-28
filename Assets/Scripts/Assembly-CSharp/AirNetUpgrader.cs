using UnityEngine;

public class AirNetUpgrader : PlotUpgrader
{
	[Tooltip("All the air net objects we need to activate/deactivate")]
	public GameObject[] airNets;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.AIR_NET)
		{
			GameObject[] array = airNets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}
}
