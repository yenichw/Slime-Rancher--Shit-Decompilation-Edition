using System.Collections.Generic;

public class DroneProgramSourcePond : DroneProgramSourceLandPlot
{
	protected override LandPlot.Id GetLandPlotID()
	{
		return LandPlot.Id.POND;
	}

	protected override IEnumerable<Orientation> GetTargetOrientations(Identifiable source)
	{
		return GetTargetOrientations_Gather(source.gameObject, new GatherConfig
		{
			distanceVertical = 1.25f
		});
	}
}
