using System.Collections.Generic;
using System.Linq;

public class DroneProgramDestinationFeeder : DroneProgramDestinationSiloStorage<DroneProgramDestinationFeeder.Destination>
{
	public class Destination : DroneNetwork.StorageMetadata
	{
		public class Comparer : SRComparer<Destination>
		{
			public new static Comparer<Destination> Default = from m in new Comparer()
				orderby m.seeded descending
				orderby m.count
				orderby m.corral.anyFavorite descending
				orderby m.corral.available
				select m;
		}

		public readonly DroneProgramDestinationCorral.Destination corral;

		public readonly bool seeded;

		public Destination(DroneNetwork.StorageMetadata storage, DroneNetwork.LandPlotMetadata metadata, Identifiable.Id id)
			: base(storage)
		{
			corral = new DroneProgramDestinationCorral.Destination(metadata, id);
			seeded = storage.id == id;
		}
	}

	protected override IEnumerable<Destination> GetDestinations(Identifiable.Id id, bool overflow)
	{
		return from m in drone.network.Plots.SelectMany((DroneNetwork.LandPlotMetadata m) => from s in m.feeders
				where s.storage.CanAccept(id, s.index, overflow)
				select new Destination(s, m, id))
			where m.seeded || m.corral.anyEat
			select m;
	}

	protected override IEnumerable<Destination> Prioritize(IEnumerable<Destination> destinations)
	{
		return destinations.OrderBy((Destination d) => d, Destination.Comparer.Default);
	}
}
