using System.Collections.Generic;
using System.Linq;

public class DroneProgramSourceGarden : DroneProgramSourceLandPlot
{
	protected override LandPlot.Id GetLandPlotID()
	{
		return LandPlot.Id.GARDEN;
	}

	public override IEnumerable<DroneFastForwarder.GatherGroup> GetFastForwardGroups(double endTime)
	{
		return base.GetFastForwardGroups(endTime).Concat(from g in drone.network.Plots.Where((DroneNetwork.LandPlotMetadata m) => m.plot.typeId == LandPlot.Id.GARDEN).SelectMany((DroneNetwork.LandPlotMetadata m) => m.plot.GetComponentsInChildren<SpawnResource>()).SelectMany((SpawnResource r) => r.GetFastForwardGroups(endTime))
			where predicate(g.id)
			select g);
	}

	protected override float GetPickupRadius()
	{
		float num = base.GetPickupRadius();
		if (Identifiable.IsFruit(source.id))
		{
			ResourceCycle component = source.GetComponent<ResourceCycle>();
			if (component != null && component.GetState() == ResourceCycle.State.RIPE)
			{
				num *= 1.5f;
			}
		}
		return num;
	}
}
