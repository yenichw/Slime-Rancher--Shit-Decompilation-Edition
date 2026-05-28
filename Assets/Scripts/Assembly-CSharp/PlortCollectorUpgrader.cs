using UnityEngine;

public class PlortCollectorUpgrader : PlotUpgrader
{
	[Tooltip("The collector object we need to activate/deactivate")]
	public GameObject collector;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.PLORT_COLLECTOR)
		{
			collector.SetActive(value: true);
		}
	}
}
