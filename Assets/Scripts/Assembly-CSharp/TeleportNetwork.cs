using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleportNetwork : MonoBehaviour
{
	[Serializable]
	private struct Destination
	{
		public string name;

		public List<TeleportDestination> exits;
	}

	private Dictionary<string, Destination> destinationLookup = new Dictionary<string, Destination>();

	public void Register(TeleportDestination exit)
	{
		GetOrCreateDestinationSet(exit.teleportDestinationName).exits.Add(exit);
	}

	public void Deregister(TeleportDestination exit)
	{
		if (!destinationLookup.TryGetValue(exit.teleportDestinationName, out var value))
		{
			Log.Warning("Tried to remove a teleport exit from a non-existent destination.", "exit.teleportDestinationName", exit.teleportDestinationName);
			return;
		}
		value.exits.Remove(exit);
		if (value.exits.Count == 0)
		{
			destinationLookup.Remove(exit.teleportDestinationName);
		}
	}

	public List<TeleportDestination> GetDestinations(string destinationName)
	{
		if (destinationLookup.ContainsKey(destinationName))
		{
			return destinationLookup[destinationName].exits;
		}
		return new List<TeleportDestination>();
	}

	private Destination GetOrCreateDestinationSet(string destinationName)
	{
		if (!destinationLookup.TryGetValue(destinationName, out var value))
		{
			value = default(Destination);
			value.name = destinationName;
			value.exits = new List<TeleportDestination>();
			destinationLookup.Add(destinationName, value);
		}
		return value;
	}

	public bool TeleportToDestination(TeleportablePlayer toTeleport, TeleportSource source, string destinationName, Func<List<TeleportDestination>, TeleportDestination> pickFunction)
	{
		List<TeleportDestination> list = new List<TeleportDestination>(GetDestinations(source.destinationSetName));
		list.RemoveAll((TeleportDestination destination) => !destination.IsLinkActive());
		if (!list.Any())
		{
			return false;
		}
		TeleportDestination teleportDestination = pickFunction(list);
		if (teleportDestination == null)
		{
			return false;
		}
		TeleportToDestination(toTeleport, source, teleportDestination);
		return true;
	}

	private void TeleportToDestination(TeleportablePlayer toTeleport, TeleportSource source, TeleportDestination destination)
	{
		source.OnDepart();
		destination.OnDepart();
		toTeleport.TeleportTo(destination.GetPosition(), rotation: destination.GetEulerAngles(), regionSetId: destination.regionSetId);
		destination.OnArrive();
	}

	public void OnDestroy()
	{
		destinationLookup.Clear();
	}

	public bool IsLinkFullyActive(TeleportSource source)
	{
		if (source.IsLinkActive())
		{
			foreach (TeleportDestination destination in GetDestinations(source.destinationSetName))
			{
				if (destination.IsLinkActive())
				{
					return true;
				}
			}
		}
		return false;
	}
}
