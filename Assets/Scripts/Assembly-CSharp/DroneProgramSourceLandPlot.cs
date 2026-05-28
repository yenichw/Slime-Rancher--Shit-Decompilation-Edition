using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DroneProgramSourceLandPlot : DroneProgramSourceDynamic
{
	private class Intermediate
	{
		public class Comparer : SRComparer<Intermediate>
		{
		}

		public DroneNetwork.LandPlotMetadata metadata;

		public IEnumerable<Identifiable> sources;
	}

	protected DroneNetwork.LandPlotMetadata currentLandPlot;

	private static ReferenceCount<int> GRAYLIST = new ReferenceCount<int>();

	private int? grayListHashCode;

	public override void Awake()
	{
		base.Awake();
		base.plexer.onSubbehaviourSelected += OnDroneSubbehaviourSelected;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		base.plexer.onSubbehaviourSelected -= OnDroneSubbehaviourSelected;
	}

	public override bool Relevancy()
	{
		if (base.Relevancy())
		{
			currentLandPlot = drone.network.GetContaining(source);
			if (currentLandPlot != null)
			{
				grayListHashCode = currentLandPlot.GetHashCode();
				GRAYLIST.Increment(grayListHashCode.Value);
			}
			return true;
		}
		return false;
	}

	public override void Deselected()
	{
		base.Deselected();
		if (grayListHashCode.HasValue)
		{
			GRAYLIST.Decrement(grayListHashCode.Value);
			grayListHashCode = null;
		}
	}

	protected override bool CanCancel()
	{
		if (!base.CanCancel() && currentLandPlot != null)
		{
			return !currentLandPlot.Contains(source);
		}
		return true;
	}

	protected override IEnumerable<Identifiable> GetSources(Predicate<Identifiable.Id> predicate)
	{
		return (from m in drone.network.Plots
			where m.plot.typeId == GetLandPlotID()
			select new Intermediate
			{
				metadata = m,
				sources = GetSources(predicate, m)
			}).OrderBy((Intermediate o) => o, from o in new Intermediate.Comparer()
			orderby o.metadata == currentLandPlot descending
			orderby GRAYLIST.ContainsKey(o.metadata.GetHashCode())
			orderby o.sources.Count() descending
			select o).SelectMany((Intermediate o) => o.sources);
	}

	protected virtual IEnumerable<Identifiable> GetSources(Predicate<Identifiable.Id> predicate, DroneNetwork.LandPlotMetadata metadata)
	{
		return metadata.trackers.SelectMany((TrackContainedIdentifiables tracker) => from id in (from kv in tracker.GetAllTracked()
				where predicate(kv.Key)
				select kv).SelectMany((KeyValuePair<Identifiable.Id, HashSet<Identifiable>> kv) => kv.Value)
			where SourcePredicate(metadata, id)
			select id);
	}

	protected override bool SourcePredicate(DroneNetwork.LandPlotMetadata metadata, Identifiable source)
	{
		if (base.SourcePredicate(metadata, source) && metadata != null)
		{
			return metadata.plot.typeId == GetLandPlotID();
		}
		return false;
	}

	private void OnDroneSubbehaviourSelected(DroneSubbehaviour subbehaviour)
	{
		if (subbehaviour != this && !(subbehaviour is DroneSubbehaviourIdle))
		{
			currentLandPlot = null;
		}
	}

	protected override GardenDroneSubnetwork GetSubnetwork()
	{
		if (currentLandPlot == null)
		{
			return null;
		}
		return currentLandPlot.subnetwork;
	}

	protected abstract LandPlot.Id GetLandPlotID();
}
