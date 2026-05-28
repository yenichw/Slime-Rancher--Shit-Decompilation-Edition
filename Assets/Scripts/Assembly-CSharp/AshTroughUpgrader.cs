using UnityEngine;

public class AshTroughUpgrader : PlotUpgrader
{
	public GameObject ashTrough;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.ASH_TROUGH)
		{
			ashTrough.SetActive(value: true);
		}
	}
}
