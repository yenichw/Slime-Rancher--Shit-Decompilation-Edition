using UnityEngine;

public class MusicBoxUpgrader : PlotUpgrader
{
	public GameObject musicBox;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.MUSIC_BOX)
		{
			musicBox.SetActive(value: true);
		}
	}
}
