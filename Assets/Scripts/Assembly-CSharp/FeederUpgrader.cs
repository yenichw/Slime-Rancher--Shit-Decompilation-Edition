using UnityEngine;

public class FeederUpgrader : PlotUpgrader
{
	public GameObject feeder;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.FEEDER)
		{
			feeder.SetActive(value: true);
		}
	}
}
