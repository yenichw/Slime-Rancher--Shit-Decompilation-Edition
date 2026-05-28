using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Behaviors/FearProfile")]
public class FearProfile : ScriptableObject
{
	[Serializable]
	public struct ThreatEntry
	{
		public Identifiable.Id id;

		public float minSearchRadius;

		public float maxSearchRadius;

		public float minThreatFearPerSec;

		public float maxThreatFearPerSec;

		public float GetSearchRadius(float currentFearDrive)
		{
			return maxSearchRadius * currentFearDrive + minSearchRadius * (1f - currentFearDrive);
		}

		public float DistToFearAdjust(float dist)
		{
			return minThreatFearPerSec + (maxSearchRadius - dist) / maxSearchRadius * (maxThreatFearPerSec - minThreatFearPerSec);
		}
	}

	public List<ThreatEntry> threats;

	private Dictionary<Identifiable.Id, ThreatEntry> threatsLookup = new Dictionary<Identifiable.Id, ThreatEntry>(Identifiable.idComparer);

	private void OnEnable()
	{
		InitializeFearProfilesLookup();
	}

	private void InitializeFearProfilesLookup()
	{
		foreach (ThreatEntry threat in threats)
		{
			threatsLookup.Add(threat.id, threat);
		}
	}

	public float GetSearchRadius(Identifiable.Id id, float currentFearDrive)
	{
		return GetThreatEntry(id).GetSearchRadius(currentFearDrive);
	}

	public float DistToFearAdjust(Identifiable.Id id, float dist)
	{
		return GetThreatEntry(id).DistToFearAdjust(dist);
	}

	public IEnumerable<Identifiable.Id> GetThreateningIdentifiables()
	{
		return threatsLookup.Keys;
	}

	private ThreatEntry GetThreatEntry(Identifiable.Id id)
	{
		return threatsLookup[id];
	}
}
