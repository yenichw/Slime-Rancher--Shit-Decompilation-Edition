using System;
using UnityEngine;

public class FilteredTrackCollisions : TrackCollisions
{
	private Predicate<GameObject> filter;

	public void SetFilter(Predicate<GameObject> filter)
	{
		this.filter = filter;
	}

	protected override void OnTriggerEnter(Collider other)
	{
		if (filter == null || filter(other.gameObject))
		{
			base.OnTriggerEnter(other);
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		if (filter == null || filter(other.gameObject))
		{
			base.OnTriggerExit(other);
		}
	}
}
