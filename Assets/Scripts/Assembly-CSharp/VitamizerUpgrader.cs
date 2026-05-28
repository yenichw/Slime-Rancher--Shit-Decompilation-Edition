using UnityEngine;

public class VitamizerUpgrader : PlotUpgrader
{
	public GameObject vitamizer;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.VITAMIZER)
		{
			vitamizer.SetActive(value: true);
		}
	}
}
