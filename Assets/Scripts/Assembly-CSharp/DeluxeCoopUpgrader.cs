using UnityEngine;

public class DeluxeCoopUpgrader : PlotUpgrader
{
	public GameObject deluxeStuff;

	public CoopRegion[] coopRegions;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.DELUXE_COOP)
		{
			if (deluxeStuff != null && !deluxeStuff.activeSelf)
			{
				deluxeStuff.SetActive(value: true);
			}
			CoopRegion[] array = coopRegions;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetDeluxe();
			}
		}
	}
}
