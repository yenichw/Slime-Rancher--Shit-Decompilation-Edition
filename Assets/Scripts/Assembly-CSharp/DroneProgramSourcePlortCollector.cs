using System;
using System.Collections.Generic;
using System.Linq;

public class DroneProgramSourcePlortCollector : DroneProgramSourceSiloStorage
{
	protected override IEnumerable<DroneNetwork.StorageMetadata> GetSources(Predicate<Identifiable.Id> predicate)
	{
		return from s in drone.network.Plots.SelectMany((DroneNetwork.LandPlotMetadata m) => m.plortCollectors)
			where predicate(s.id)
			orderby s.count descending
			select s;
	}
}
