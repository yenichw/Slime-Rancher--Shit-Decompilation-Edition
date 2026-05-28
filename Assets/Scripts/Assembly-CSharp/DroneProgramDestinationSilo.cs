using System.Collections.Generic;
using System.Linq;

public class DroneProgramDestinationSilo : DroneProgramDestinationSiloStorage<DroneNetwork.StorageMetadata>
{
	protected override IEnumerable<DroneNetwork.StorageMetadata> GetDestinations(Identifiable.Id id, bool overflow)
	{
		return from s in drone.network.Plots.SelectMany((DroneNetwork.LandPlotMetadata m) => m.silos)
			where s.storage.CanAccept(id, s.index, overflow)
			select s;
	}

	protected override IEnumerable<DroneNetwork.StorageMetadata> Prioritize(IEnumerable<DroneNetwork.StorageMetadata> destinations)
	{
		return destinations.OrderByDescending((DroneNetwork.StorageMetadata s) => s.count);
	}
}
