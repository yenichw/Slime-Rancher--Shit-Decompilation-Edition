using UnityEngine;

public class WallUpgrader : PlotUpgrader
{
	public GameObject standardWalls;

	public GameObject upgradeWalls;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.WALLS)
		{
			standardWalls.SetActive(value: false);
			upgradeWalls.SetActive(value: true);
		}
	}
}
