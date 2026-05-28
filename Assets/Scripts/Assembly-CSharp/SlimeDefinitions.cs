using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Slime Definitions")]
public class SlimeDefinitions : ScriptableObject
{
	private struct PlortPair
	{
		public class EqualityComparer : IEqualityComparer<PlortPair>
		{
			public static EqualityComparer Default = new EqualityComparer();

			public bool Equals(PlortPair x, PlortPair y)
			{
				if (x.Plort1 == y.Plort1)
				{
					return x.Plort2 == y.Plort2;
				}
				return false;
			}

			public int GetHashCode(PlortPair obj)
			{
				return (int)((((int)obj.Plort1 << 5) + obj.Plort1) ^ obj.Plort2);
			}
		}

		public Identifiable.Id Plort1;

		public Identifiable.Id Plort2;

		public PlortPair(Identifiable.Id plort1, Identifiable.Id plort2)
		{
			if (plort1 <= plort2)
			{
				Plort1 = plort1;
				Plort2 = plort2;
			}
			else
			{
				Plort1 = plort2;
				Plort2 = plort1;
			}
		}
	}

	private struct SlimeDefinitionPair
	{
		public class EqualityComparer : IEqualityComparer<SlimeDefinitionPair>
		{
			public static EqualityComparer Default = new EqualityComparer();

			public bool Equals(SlimeDefinitionPair x, SlimeDefinitionPair y)
			{
				if (x.SlimeDefinition1 == y.SlimeDefinition1)
				{
					return x.SlimeDefinition2 == y.SlimeDefinition2;
				}
				return false;
			}

			public int GetHashCode(SlimeDefinitionPair obj)
			{
				return ((obj.SlimeDefinition1.GetHashCode() << 5) + obj.SlimeDefinition1.GetHashCode()) ^ obj.SlimeDefinition2.GetHashCode();
			}
		}

		public SlimeDefinition SlimeDefinition1;

		public SlimeDefinition SlimeDefinition2;

		public SlimeDefinitionPair(SlimeDefinition slimeDefinition1, SlimeDefinition slimeDefinition2)
		{
			if (slimeDefinition1.GetHashCode() <= slimeDefinition2.GetHashCode())
			{
				SlimeDefinition1 = slimeDefinition1;
				SlimeDefinition2 = slimeDefinition2;
			}
			else
			{
				SlimeDefinition1 = slimeDefinition1;
				SlimeDefinition2 = slimeDefinition2;
			}
		}
	}

	public SlimeDefinition[] Slimes;

	private Dictionary<Identifiable.Id, SlimeDefinition> slimeDefinitionsByIdentifiable = new Dictionary<Identifiable.Id, SlimeDefinition>(Identifiable.idComparer);

	private Dictionary<PlortPair, SlimeDefinition> largoDefinitionByBasePlorts = new Dictionary<PlortPair, SlimeDefinition>(PlortPair.EqualityComparer.Default);

	private Dictionary<SlimeDefinitionPair, SlimeDefinition> largoDefinitionByBaseDefinitions = new Dictionary<SlimeDefinitionPair, SlimeDefinition>(SlimeDefinitionPair.EqualityComparer.Default);

	public void OnEnable()
	{
		RefreshIndexes();
		RefreshDefinitions();
	}

	public void RefreshIndexes()
	{
		SlimeDefinition[] slimes = Slimes;
		foreach (SlimeDefinition slimeDefinition in slimes)
		{
			try
			{
				slimeDefinitionsByIdentifiable.Add(slimeDefinition.IdentifiableId, slimeDefinition);
				if (slimeDefinition.IsLargo && slimeDefinition.BaseSlimes.Length == 2)
				{
					largoDefinitionByBasePlorts.Add(new PlortPair(slimeDefinition.BaseSlimes[0].Diet.Produces[0], slimeDefinition.BaseSlimes[1].Diet.Produces[0]), slimeDefinition);
					largoDefinitionByBaseDefinitions.Add(new SlimeDefinitionPair(slimeDefinition.BaseSlimes[0], slimeDefinition.BaseSlimes[1]), slimeDefinition);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception caught while attempting to process slime.", "name", slimeDefinition.Name, "Exception", ex.Message, "Stacktrace", ex.StackTrace.ToString());
			}
		}
	}

	public void RefreshDefinitions()
	{
		SlimeDefinition[] slimes = Slimes;
		foreach (SlimeDefinition slimeDefinition in slimes)
		{
			slimeDefinition.Diet.RefreshEatMap(this, slimeDefinition);
		}
	}

	public SlimeDefinition GetLargoByPlorts(Identifiable.Id plort1, Identifiable.Id plort2)
	{
		SlimeDefinition value = null;
		largoDefinitionByBasePlorts.TryGetValue(new PlortPair(plort1, plort2), out value);
		return value;
	}

	public SlimeDefinition GetLargoByBaseSlimes(SlimeDefinition slimeDefinition1, SlimeDefinition slimeDefinition2)
	{
		SlimeDefinition value = null;
		largoDefinitionByBaseDefinitions.TryGetValue(new SlimeDefinitionPair(slimeDefinition1, slimeDefinition2), out value);
		return value;
	}

	public SlimeDefinition GetSlimeByIdentifiableId(Identifiable.Id id)
	{
		SlimeDefinition value = null;
		slimeDefinitionsByIdentifiable.TryGetValue(id, out value);
		return value;
	}
}
