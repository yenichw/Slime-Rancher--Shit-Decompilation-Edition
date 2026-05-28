using System;
using System.Collections.Generic;
using System.Linq;

public class DroneProgramSourceSilo : DroneProgramSourceSiloStorage
{
	protected override IEnumerable<DroneNetwork.StorageMetadata> GetSources(Predicate<Identifiable.Id> predicate)
	{
		return from s in drone.network.Plots.SelectMany((DroneNetwork.LandPlotMetadata m) => m.silos)
			where predicate(s.id)
			orderby s.count
			select s;
	}
}
