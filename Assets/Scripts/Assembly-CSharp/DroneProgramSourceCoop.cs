using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneProgramSourceCoop : DroneProgramSourceLandPlot
{
	private const int SKIP_ROOSTER = 2;

	private const int SKIP_HEN_RARE = 1;

	private const int SKIP_HEN = 4;

	protected override LandPlot.Id GetLandPlotID()
	{
		return LandPlot.Id.COOP;
	}

	protected override IEnumerable<Identifiable> GetSources(Predicate<Identifiable.Id> predicate, DroneNetwork.LandPlotMetadata metadata)
	{
		TrackContainedIdentifiables container = metadata.trackers.First();
		return from s in (from pair in container.GetAllTracked()
				where predicate(pair.Key)
				select pair).SelectMany((KeyValuePair<Identifiable.Id, HashSet<Identifiable>> pair) => pair.Value.Skip(GetSkipCount(pair.Key, container)))
			where SourcePredicate(metadata, s)
			select s;
	}

	protected override int GetMaxPickup(Identifiable.Id id)
	{
		int b = int.MaxValue;
		if (currentLandPlot != null && Identifiable.MEAT_CLASS.Contains(id))
		{
			TrackContainedIdentifiables trackContainedIdentifiables = currentLandPlot.trackers.First();
			b = trackContainedIdentifiables.Count(id) - GetSkipCount(id, trackContainedIdentifiables);
		}
		return Mathf.Min(base.GetMaxPickup(id), b);
	}

	private static int GetSkipCount(Identifiable.Id id, TrackContainedIdentifiables container)
	{
		switch (id)
		{
		case Identifiable.Id.ROOSTER:
			return 2;
		case Identifiable.Id.PAINTED_HEN:
		{
			int num8 = Mathf.Min(1, container.Count(Identifiable.Id.BRIAR_HEN));
			int num9 = Mathf.Min(1, container.Count(Identifiable.Id.STONY_HEN));
			return Math.Max(1, 4 - num8 - num9);
		}
		case Identifiable.Id.BRIAR_HEN:
		{
			int num6 = container.Count(Identifiable.Id.PAINTED_HEN);
			int num7 = Mathf.Min(1, container.Count(Identifiable.Id.STONY_HEN));
			return Math.Max(1, 4 - num6 - num7);
		}
		case Identifiable.Id.STONY_HEN:
		{
			int num4 = container.Count(Identifiable.Id.PAINTED_HEN);
			int num5 = container.Count(Identifiable.Id.BRIAR_HEN);
			return Math.Max(1, 4 - num4 - num5);
		}
		case Identifiable.Id.HEN:
		{
			int num = container.Count(Identifiable.Id.PAINTED_HEN);
			int num2 = container.Count(Identifiable.Id.BRIAR_HEN);
			int num3 = container.Count(Identifiable.Id.STONY_HEN);
			return Mathf.Max(0, 4 - num - num2 - num3);
		}
		default:
			return 0;
		}
	}
}
