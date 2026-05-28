using UnityEngine;

public class SprinklerUpgrader : PlotUpgrader
{
	public GameObject sprinkler;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.SPRINKLER)
		{
			sprinkler.SetActive(value: true);
		}
	}
}
