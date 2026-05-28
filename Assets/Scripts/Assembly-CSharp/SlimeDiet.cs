using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SlimeDiet
{
	public class EatMapEntry
	{
		public Identifiable.Id eats;

		public bool isFavorite;

		public int favoriteProductionCount = 2;

		public Identifiable.Id producesId;

		public Identifiable.Id becomesId;

		public SlimeEmotions.Emotion driver;

		public float extraDrive;

		public float minDrive;

		public int NumToProduce()
		{
			if (!isFavorite)
			{
				return 1;
			}
			return favoriteProductionCount;
		}
	}

	public SlimeEat.FoodGroup[] MajorFoodGroups;

	public Identifiable.Id[] Favorites;

	public Identifiable.Id[] AdditionalFoods;

	public Identifiable.Id[] Produces;

	public int FavoriteProductionCount;

	[HideInInspector]
	public List<EatMapEntry> EatMap;

	public static SlimeDiet Combine(SlimeDiet diet1, SlimeDiet diet2)
	{
		return new SlimeDiet
		{
			MajorFoodGroups = diet1.MajorFoodGroups.Union(diet2.MajorFoodGroups).ToArray(),
			Favorites = diet1.Favorites.Union(diet2.Favorites).ToArray(),
			AdditionalFoods = diet1.AdditionalFoods.Union(diet2.AdditionalFoods).ToArray(),
			Produces = diet1.Produces.Union(diet2.Produces).ToArray(),
			FavoriteProductionCount = diet1.FavoriteProductionCount
		};
	}

	public IEnumerable<Identifiable.Id> GetDietIdentifiableIds()
	{
		return new HashSet<Identifiable.Id>(MajorFoodGroups.SelectMany((SlimeEat.FoodGroup group) => SlimeEat.GetFoodGroupIds(group)).Concat(AdditionalFoods)).AsEnumerable();
	}

	public void RefreshEatMap(SlimeDefinitions definitions, SlimeDefinition definition)
	{
		EatMap = new List<EatMapEntry>();
		foreach (Identifiable.Id dietIdentifiableId in GetDietIdentifiableIds())
		{
			SlimeEmotions.Emotion driver = ((dietIdentifiableId == Identifiable.Id.SPICY_TOFU) ? SlimeEmotions.Emotion.NONE : SlimeEmotions.Emotion.HUNGER);
			Identifiable.Id[] produces = Produces;
			foreach (Identifiable.Id producesId in produces)
			{
				EatMapEntry item = new EatMapEntry
				{
					eats = dietIdentifiableId,
					producesId = producesId,
					isFavorite = Favorites.Contains(dietIdentifiableId),
					favoriteProductionCount = FavoriteProductionCount,
					driver = driver,
					minDrive = 0f,
					extraDrive = 0f,
					becomesId = Identifiable.Id.NONE
				};
				EatMap.Add(item);
			}
		}
		if (!ProducePlorts() || (!definition.IsLargo && !definition.CanLargofy))
		{
			return;
		}
		foreach (Identifiable.Id item3 in from id in SlimeEat.GetFoodGroupIds(SlimeEat.FoodGroup.PLORTS)
			where id != Identifiable.Id.QUICKSILVER_PLORT
			select id)
		{
			if (Produces.Contains(item3))
			{
				continue;
			}
			Identifiable.Id id2 = Identifiable.Id.NONE;
			if (definition.IsLargo)
			{
				id2 = Identifiable.Id.TARR_SLIME;
			}
			else
			{
				SlimeDefinition largoByPlorts = definitions.GetLargoByPlorts(item3, Produces[0]);
				if (largoByPlorts == null)
				{
					continue;
				}
				id2 = largoByPlorts.IdentifiableId;
			}
			EatMapEntry item2 = new EatMapEntry
			{
				eats = item3,
				producesId = Identifiable.Id.NONE,
				isFavorite = false,
				favoriteProductionCount = FavoriteProductionCount,
				driver = SlimeEmotions.Emotion.AGITATION,
				minDrive = 0.5f,
				extraDrive = 0f,
				becomesId = id2
			};
			EatMap.Add(item2);
		}
	}

	public void AddEatMapEntries(Identifiable.Id id, IList<EatMapEntry> targetEntries)
	{
		for (int i = 0; i < EatMap.Count; i++)
		{
			if (EatMap[i].eats == id)
			{
				targetEntries.Add(EatMap[i]);
			}
		}
	}

	private bool ProducePlorts()
	{
		return Produces.Count((Identifiable.Id id) => Identifiable.IsPlort(id)) > 0;
	}

	public static string GetFoodCategoryMsg(Identifiable.Id id)
	{
		if (Array.IndexOf(SlimeEat.GetFoodGroupIds(SlimeEat.FoodGroup.VEGGIES), id) != -1)
		{
			return "m.foodgroup.veggies";
		}
		if (Array.IndexOf(SlimeEat.GetFoodGroupIds(SlimeEat.FoodGroup.FRUIT), id) != -1)
		{
			return "m.foodgroup.fruit";
		}
		if (Array.IndexOf(SlimeEat.GetFoodGroupIds(SlimeEat.FoodGroup.MEAT), id) != -1)
		{
			return "m.foodgroup.meat";
		}
		if (Array.IndexOf(SlimeEat.GetFoodGroupIds(SlimeEat.FoodGroup.GINGER), id) != -1)
		{
			return "m.foodgroup.ginger";
		}
		return "m.foodgroup.none";
	}

	public string GetDirectFoodGroupsMsg()
	{
		return GetGroupsMsg(MajorFoodGroups);
	}

	public string GetModulesFoodGroupsMsg()
	{
		HashSet<SlimeEat.FoodGroup> hashSet = new HashSet<SlimeEat.FoodGroup>();
		SlimeEat.FoodGroup[] majorFoodGroups = MajorFoodGroups;
		foreach (SlimeEat.FoodGroup item in majorFoodGroups)
		{
			hashSet.Add(item);
		}
		return GetGroupsMsg(hashSet);
	}

	private string GetGroupsMsg(ICollection<SlimeEat.FoodGroup> groups)
	{
		switch (groups.Count)
		{
		case 0:
			return "m.foodgroup.none";
		case 1:
		{
			string text2 = Enum.GetName(typeof(SlimeEat.FoodGroup), groups.First()).ToLowerInvariant();
			return "m.foodgroup." + text2;
		}
		case 3:
			return "m.foodgroup.all";
		default:
		{
			string[] array = new string[groups.Count];
			int num = 0;
			foreach (SlimeEat.FoodGroup group in groups)
			{
				string text = Enum.GetName(typeof(SlimeEat.FoodGroup), group).ToLowerInvariant();
				array[num++] = "m.foodgroup." + text;
			}
			return MessageUtil.Compose("m.andlist" + groups.Count, array);
		}
		}
	}
}
