using System;
using System.Collections.Generic;
using UnityEngine;

public class Gadget : MonoBehaviour
{
	public enum Id
	{
		NONE = 0,
		EXTRACTOR_DRILL_NOVICE = 100,
		EXTRACTOR_DRILL_ADVANCED = 101,
		EXTRACTOR_DRILL_MASTER = 102,
		EXTRACTOR_DRILL_TITAN = 103,
		EXTRACTOR_PUMP_NOVICE = 110,
		EXTRACTOR_PUMP_ADVANCED = 111,
		EXTRACTOR_PUMP_MASTER = 112,
		EXTRACTOR_PUMP_ABYSSAL = 113,
		EXTRACTOR_APIARY_NOVICE = 120,
		EXTRACTOR_APIARY_ADVANCED = 121,
		EXTRACTOR_APIARY_MASTER = 122,
		EXTRACTOR_APIARY_ROYAL = 123,
		TELEPORTER_PINK = 200,
		TELEPORTER_BLUE = 201,
		TELEPORTER_VIOLET = 202,
		TELEPORTER_GREY = 203,
		TELEPORTER_GREEN = 204,
		TELEPORTER_RED = 205,
		TELEPORTER_AMBER = 206,
		TELEPORTER_GOLD = 207,
		TELEPORTER_BERRY = 208,
		TELEPORTER_COCOA = 209,
		TELEPORTER_BUTTERSCOTCH = 210,
		WARP_DEPOT_PINK = 300,
		WARP_DEPOT_BLUE = 301,
		WARP_DEPOT_VIOLET = 302,
		WARP_DEPOT_GREY = 303,
		WARP_DEPOT_GREEN = 304,
		WARP_DEPOT_RED = 305,
		WARP_DEPOT_AMBER = 306,
		WARP_DEPOT_GOLD = 307,
		WARP_DEPOT_BERRY = 308,
		WARP_DEPOT_COCOA = 309,
		WARP_DEPOT_BUTTERSCOTCH = 310,
		MARKET_LINK = 390,
		MED_STATION = 400,
		RAPID_MED_STATION = 401,
		HYDRO_TURRET = 410,
		SUPER_HYDRO_TURRET = 411,
		HYDRO_SHOWER = 420,
		SUPER_HYDRO_SHOWER = 421,
		TAMING_BELL = 430,
		ELITE_TAMING_BELL = 431,
		SPRING_PAD = 460,
		POTTED_TACTUS = 461,
		REFINERY_LINK = 462,
		SLIME_HOOP = 463,
		SLIME_STAGE = 464,
		ECHO_NET = 465,
		LAMP_PINK = 500,
		LAMP_BLUE = 501,
		LAMP_VIOLET = 502,
		LAMP_GREY = 503,
		LAMP_GREEN = 504,
		LAMP_RED = 505,
		LAMP_AMBER = 506,
		LAMP_GOLD = 507,
		LAMP_BERRY = 508,
		LAMP_COCOA = 509,
		LAMP_BUTTERSCOTCH = 510,
		FASHION_POD_HANDLEBAR = 600,
		FASHION_POD_SHADY = 601,
		FASHION_POD_CLIP_ON = 602,
		FASHION_POD_GOOGLY = 603,
		FASHION_POD_SERIOUS = 604,
		FASHION_POD_SMART = 605,
		FASHION_POD_CUTE = 606,
		FASHION_POD_ROYAL = 607,
		FASHION_POD_DANDY = 608,
		FASHION_POD_PIRATEY = 609,
		FASHION_POD_HEROIC = 610,
		FASHION_POD_SCIFI = 611,
		FASHION_POD_PARTY_GLASSES = 612,
		FASHION_POD_SCUBA = 613,
		FASHION_POD_REMOVER = 699,
		GORDO_SNARE_NOVICE = 700,
		GORDO_SNARE_ADVANCED = 701,
		GORDO_SNARE_MASTER = 702,
		SPONGE_TREE = 11000,
		SPONGE_SHRUB = 11001,
		PINK_CORAL_COLUMNS = 11002,
		CORAL_GRASS_PATCH = 11003,
		MOSSY_TREE = 12000,
		MOSSY_TREE_STUMP = 12001,
		GLOW_CONES = 12002,
		WILDFLOWER_PATCH = 12003,
		JUMBO_SHROOM = 12004,
		MINTY_GRASS_PATCH = 13000,
		BLUE_CORAL_COLUMNS = 13001,
		HEXIUM_FORMATION = 13002,
		CRYSTAL_CLUSTER = 13003,
		FIREFLOWER_PATCH = 13004,
		SUNBURST_TREE = 14000,
		VERDANT_GRASS_PATCH = 14001,
		STAR_FLOWER_PATCH = 14002,
		RUINED_PILLAR = 14003,
		GLOW_STICKS = 14004,
		CRYSTAL_SCONCE = 14005,
		FIERY_GLASS_SCULPTURE = 15000,
		THUNDERING_GLASS_SCULPTURE = 15001,
		TOWERING_GLASS_SCULPTURE = 15002,
		PALM_TREE = 15003,
		PALM_SPROUT = 15004,
		COIL_GRASS = 15005,
		RUINED_DESERT_COLUMN = 15006,
		RUINED_DESERT_BLOCKS = 15007,
		DESERT_DECO_9 = 15008,
		WILDS_ROCKS_1 = 16000,
		WILDS_ROCKS_2 = 16001,
		WILDS_ROCKS_3 = 16002,
		WILDS_TREE = 16003,
		WILDS_CORAL_COLUMN = 16004,
		WILDS_GRASS_PATCH = 16005,
		WILDS_FLOWER_PATCH = 16006,
		MAGNETICORE_ARRAY_SMALL = 17000,
		MAGNETICORE_ARRAY_TALL = 17001,
		MAGNETICORE_ARRAY_STURDY = 17002,
		MAGNETICORE_ARRAY_ORNATE = 17003,
		NIMBLE_GRASS_PATCH = 17004,
		NIMBLE_NEEDLE_TREE = 17005,
		DECO_BATTERY_TOWER = 17100,
		DECO_DIGI_PANEL = 17101,
		DECO_DIGI_SHRUB = 17102,
		DECO_DIGI_TREE = 17103,
		DECO_FIELD_KIT = 17104,
		DECO_SUPPLY_DROP = 17105,
		DRONE = 18000,
		DRONE_ADVANCED = 18001,
		CHICKEN_CLONER = 19000,
		PORTABLE_WATER_TAP = 19001,
		PORTABLE_SLIME_BAIT_FRUIT = 19002,
		PORTABLE_SCARECROW = 19003,
		DASH_PAD = 19005,
		PORTABLE_SLIME_BAIT_VEGGIE = 19006,
		PORTABLE_SLIME_BAIT_MEAT = 19007
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public bool Equals(Id id1, Id id2)
		{
			return id1 == id2;
		}

		public int GetHashCode(Id obj)
		{
			return (int)obj;
		}
	}

	public interface LinkDestroyer
	{
		bool ShouldDestroyPair();

		LinkDestroyer GetLinked();
	}

	public static IdComparer idComparer;

	public static List<Id> ALL_FASHIONS;

	public static HashSet<Id> EXTRACTOR_CLASS;

	public static HashSet<Id> TELEPORTER_CLASS;

	public static HashSet<Id> WARP_DEPOT_CLASS;

	public static HashSet<Id> LAMP_CLASS;

	public static HashSet<Id> MISC_CLASS;

	public static HashSet<Id> FASHION_POD_CLASS;

	public static HashSet<Id> SNARE_CLASS;

	public static HashSet<Id> ECHO_NET_CLASS;

	public static HashSet<Id> DRONE_CLASS;

	public static HashSet<Id> DECO_CLASS;

	public Id id;

	protected Transform rotationTransform;

	public static void RegisterFashion(Id id)
	{
		ALL_FASHIONS.RemoveAll((Id it) => it == id);
		ALL_FASHIONS.Add(id);
	}

	public virtual void Awake()
	{
		rotationTransform = base.transform;
	}

	static Gadget()
	{
		idComparer = new IdComparer();
		ALL_FASHIONS = new List<Id>
		{
			Id.FASHION_POD_HANDLEBAR,
			Id.FASHION_POD_SHADY,
			Id.FASHION_POD_CLIP_ON,
			Id.FASHION_POD_GOOGLY,
			Id.FASHION_POD_SERIOUS,
			Id.FASHION_POD_SMART,
			Id.FASHION_POD_DANDY,
			Id.FASHION_POD_CUTE,
			Id.FASHION_POD_ROYAL,
			Id.FASHION_POD_PARTY_GLASSES,
			Id.FASHION_POD_SCUBA,
			Id.FASHION_POD_REMOVER
		};
		EXTRACTOR_CLASS = new HashSet<Id>(idComparer);
		TELEPORTER_CLASS = new HashSet<Id>(idComparer);
		WARP_DEPOT_CLASS = new HashSet<Id>(idComparer);
		LAMP_CLASS = new HashSet<Id>(idComparer);
		MISC_CLASS = new HashSet<Id>(idComparer);
		FASHION_POD_CLASS = new HashSet<Id>(idComparer);
		SNARE_CLASS = new HashSet<Id>(idComparer);
		ECHO_NET_CLASS = new HashSet<Id>(idComparer);
		DRONE_CLASS = new HashSet<Id>(idComparer);
		DECO_CLASS = new HashSet<Id>(idComparer);
		foreach (Id value in Enum.GetValues(typeof(Id)))
		{
			if (value >= Id.EXTRACTOR_DRILL_NOVICE && value < Id.TELEPORTER_PINK)
			{
				EXTRACTOR_CLASS.Add(value);
			}
			else if (value >= Id.TELEPORTER_PINK && value < Id.WARP_DEPOT_PINK)
			{
				TELEPORTER_CLASS.Add(value);
			}
			else if (value >= Id.WARP_DEPOT_PINK && value < Id.MARKET_LINK)
			{
				WARP_DEPOT_CLASS.Add(value);
			}
			else if (value >= Id.MARKET_LINK && value < Id.ECHO_NET)
			{
				MISC_CLASS.Add(value);
			}
			else if (value >= Id.ECHO_NET && value < Id.LAMP_PINK)
			{
				ECHO_NET_CLASS.Add(value);
			}
			else if (value >= Id.LAMP_PINK && value < Id.FASHION_POD_HANDLEBAR)
			{
				LAMP_CLASS.Add(value);
			}
			else if (value >= Id.FASHION_POD_HANDLEBAR && value < Id.GORDO_SNARE_NOVICE)
			{
				FASHION_POD_CLASS.Add(value);
			}
			else if (value >= Id.GORDO_SNARE_NOVICE && value < Id.SPONGE_TREE)
			{
				SNARE_CLASS.Add(value);
			}
			else if (value >= Id.SPONGE_TREE && value < Id.DRONE)
			{
				DECO_CLASS.Add(value);
			}
			else if (value >= Id.DRONE && value <= Id.DRONE_ADVANCED)
			{
				DRONE_CLASS.Add(value);
			}
		}
	}

	public static bool IsExtractor(Id gadgetId)
	{
		return EXTRACTOR_CLASS.Contains(gadgetId);
	}

	public static bool IsTeleporter(Id gadgetId)
	{
		return TELEPORTER_CLASS.Contains(gadgetId);
	}

	public static bool IsWarpDepot(Id gadgetId)
	{
		return WARP_DEPOT_CLASS.Contains(gadgetId);
	}

	public static bool IsMisc(Id gadgetId)
	{
		return MISC_CLASS.Contains(gadgetId);
	}

	public static bool IsEchoNet(Id gadgetId)
	{
		return ECHO_NET_CLASS.Contains(gadgetId);
	}

	public static bool IsDrone(Id gadgetId)
	{
		return DRONE_CLASS.Contains(gadgetId);
	}

	public static bool IsLamp(Id gadgetId)
	{
		return LAMP_CLASS.Contains(gadgetId);
	}

	public static bool IsFashionPod(Id gadgetId)
	{
		return FASHION_POD_CLASS.Contains(gadgetId);
	}

	public static bool IsSnare(Id gadgetId)
	{
		return SNARE_CLASS.Contains(gadgetId);
	}

	public static bool IsDeco(Id gadgetId)
	{
		return DECO_CLASS.Contains(gadgetId);
	}

	public bool DestroysLinkedPairOnRemoval()
	{
		return GetComponentInChildren<LinkDestroyer>()?.ShouldDestroyPair() ?? false;
	}

	public bool DestroysOnRemoval()
	{
		return SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id).destroyOnRemoval;
	}

	public static bool IsLinkDestroyerType(Id id)
	{
		string text = id.ToString();
		if (!text.StartsWith("TELEPORTER_"))
		{
			return text.StartsWith("WARP_DEPOT_");
		}
		return true;
	}

	public void DestroyGadget()
	{
		GadgetSite componentInParent = GetComponentInParent<GadgetSite>();
		if (componentInParent != null)
		{
			componentInParent.DestroyAttached();
		}
	}

	public void AddRotation(float adjustment)
	{
		SetRotation(GetRotation() + adjustment);
	}

	public void SetRotation(float rotation)
	{
		rotationTransform.localRotation = Quaternion.Euler(0f, rotation, 0f);
	}

	public float GetRotation()
	{
		return rotationTransform.localRotation.eulerAngles.y;
	}

	public virtual void OnUserDestroyed()
	{
	}

	public static string GetName(Id id, bool reportMissing = true)
	{
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia");
		string arg = Enum.GetName(typeof(Id), id).ToLowerInvariant();
		string resourceString = bundle.GetResourceString($"m.gadget.name.{arg}");
		if (reportMissing && resourceString == null)
		{
			Log.Warning("Missing gadget translation.", "id", id);
		}
		return resourceString;
	}
}
