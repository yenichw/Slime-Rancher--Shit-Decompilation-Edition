using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GameObjectActorModelIdentifiableIndex
{
	public struct Entry : IEquatable<Entry>
	{
		private Identifiable.Id id;

		private GameObject gameObject;

		private ActorModel actorModel;

		public Identifiable.Id Id => id;

		public GameObject GameObject => gameObject;

		public Entry(GameObject gameObject, ActorModel actorModel)
		{
			id = actorModel.ident;
			this.gameObject = gameObject;
			this.actorModel = actorModel;
		}

		public bool Equals(Entry other)
		{
			return actorModel.actorId == other.actorModel.actorId;
		}

		public override int GetHashCode()
		{
			return actorModel.actorId.GetHashCode();
		}
	}

	private readonly List<Entry> EMPTY_ENTRY_LIST = new List<Entry>();

	private Dictionary<Identifiable.Id, List<Entry>> objects = new Dictionary<Identifiable.Id, List<Entry>>(Identifiable.idComparer);

	private List<Entry> slimes = new List<Entry>();

	private List<Entry> animals = new List<Entry>();

	private List<Entry> largos = new List<Entry>();

	private List<Entry> toys = new List<Entry>();

	public void Register(GameObject obj, ActorModel actorModel)
	{
		Entry item = new Entry(obj, actorModel);
		Identifiable.Id id = item.Id;
		objects.TryGetValue(id, out var value);
		if (value == null)
		{
			value = new List<Entry>();
			objects[id] = value;
		}
		value.Add(item);
		if (Identifiable.IsSlime(id))
		{
			slimes.Add(item);
		}
		if (Identifiable.IsAnimal(id))
		{
			animals.Add(item);
		}
		if (Identifiable.IsLargo(id))
		{
			largos.Add(item);
		}
		if (Identifiable.IsToy(id))
		{
			toys.Add(item);
		}
	}

	public void Deregister(GameObject obj, ActorModel actorModel)
	{
		Entry item = new Entry(obj, actorModel);
		Identifiable.Id id = item.Id;
		objects.TryGetValue(id, out var value);
		if (value != null)
		{
			value.Remove(item);
			if (value.Count <= 0)
			{
				objects[id] = null;
			}
		}
		if (Identifiable.IsSlime(id))
		{
			slimes.Remove(item);
		}
		if (Identifiable.IsAnimal(id))
		{
			animals.Remove(item);
		}
		if (Identifiable.IsLargo(id))
		{
			largos.Remove(item);
		}
		if (Identifiable.IsToy(id))
		{
			toys.Remove(item);
		}
	}

	public bool IsRegistered(Identifiable.Id id, GameObject gameObject, ActorModel actorModel)
	{
		Entry item = new Entry(gameObject, actorModel);
		if (objects.TryGetValue(id, out var value) && value != null)
		{
			return value.Contains(item);
		}
		return false;
	}

	public IList<Entry> GetObjectsByIdentifiableId(Identifiable.Id id)
	{
		if (objects.TryGetValue(id, out var value) && value != null)
		{
			return value;
		}
		return EMPTY_ENTRY_LIST;
	}

	public IList<Entry> GetSlimes()
	{
		return slimes;
	}

	public int GetSlimeCount()
	{
		return slimes.Count;
	}

	public IList<Entry> GetToys()
	{
		return toys;
	}

	public int GetToyCount()
	{
		return toys.Count;
	}

	public IList<Entry> GetLargos()
	{
		return largos;
	}

	public int GetLargoCount()
	{
		return largos.Count;
	}

	public IList<Entry> GetAnimals()
	{
		return animals;
	}

	public int GetAnimalCount()
	{
		return animals.Count;
	}

	public IEnumerable<Entry> GetAllRegistered()
	{
		foreach (List<Entry> entries in objects.Values)
		{
			if (entries != null)
			{
				int ii = 0;
				while (ii < entries.Count)
				{
					yield return entries[ii];
					int num = ii + 1;
					ii = num;
				}
			}
		}
	}
}
