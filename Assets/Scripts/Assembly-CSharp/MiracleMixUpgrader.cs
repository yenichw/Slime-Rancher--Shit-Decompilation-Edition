using UnityEngine;

public class MiracleMixUpgrader : PlotUpgrader
{
	public GameObject miracleMix;

	public GameObject normSoil;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.MIRACLE_MIX)
		{
			miracleMix.SetActive(value: true);
			normSoil.SetActive(value: false);
		}
	}
}
