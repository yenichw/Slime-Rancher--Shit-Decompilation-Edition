using UnityEngine;

public class MineralSoilUpgrader : PlotUpgrader
{
	public GameObject soil;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.SOIL)
		{
			soil.SetActive(value: true);
		}
	}
}
