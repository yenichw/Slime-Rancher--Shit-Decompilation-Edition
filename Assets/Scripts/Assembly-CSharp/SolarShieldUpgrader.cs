using UnityEngine;

public class SolarShieldUpgrader : PlotUpgrader
{
	[Tooltip("All the solar shield objects we need to activate/deactivate")]
	public GameObject[] shields;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.SOLAR_SHIELD)
		{
			GameObject[] array = shields;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}
}
