using UnityEngine;

public class ScareslimeUpgrader : PlotUpgrader
{
	public GameObject scareslime;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.SCARESLIME)
		{
			scareslime.SetActive(value: true);
		}
	}
}
