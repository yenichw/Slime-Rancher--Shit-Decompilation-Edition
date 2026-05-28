using System;
using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class Identifiable : SRBehaviour, ActorModel.Participant
{
	public enum Id
	{
		NONE = 0,
		RAD_SLIME = 1,
		ROCK_SLIME = 2,
		PINK_SLIME = 3,
		RAD_PLORT = 4,
		ROCK_PLORT = 5,
		PINK_PLORT = 6,
		GOLD_PLORT = 7,
		CUBERRY_FRUIT = 8,
		MANGO_FRUIT = 9,
		TARR_SLIME = 10,
		GOLD_SLIME = 11,
		PINK_ROCK_LARGO = 12,
		RAD_ROCK_LARGO = 13,
		PINK_RAD_LARGO = 14,
		PLAYER = 15,
		HEN = 16,
		ROOSTER = 17,
		CHICK = 18,
		CARROT_VEGGIE = 19,
		OCAOCA_VEGGIE = 20,
		BOOM_SLIME = 21,
		PINK_BOOM_LARGO = 22,
		BOOM_ROCK_LARGO = 23,
		BOOM_RAD_LARGO = 24,
		BOOM_PLORT = 25,
		PEAR_FRUIT = 26,
		POGO_FRUIT = 27,
		PARSNIP_VEGGIE = 28,
		BEET_VEGGIE = 29,
		SCARECROW = 30,
		PHOSPHOR_SLIME = 31,
		PHOSPHOR_ROCK_LARGO = 32,
		BOOM_PHOSPHOR_LARGO = 33,
		PHOSPHOR_RAD_LARGO = 34,
		PINK_PHOSPHOR_LARGO = 35,
		PHOSPHOR_PLORT = 36,
		TABBY_SLIME = 37,
		TABBY_PLORT = 38,
		PINK_TABBY_LARGO = 39,
		BOOM_TABBY_LARGO = 40,
		RAD_TABBY_LARGO = 41,
		ROCK_TABBY_LARGO = 42,
		PHOSPHOR_TABBY_LARGO = 43,
		CRATE_REEF_01 = 44,
		CRATE_QUARRY_01 = 45,
		CRATE_MOSS_01 = 46,
		CRATE_DESERT_01 = 47,
		WATER_LIQUID = 48,
		ELDER_HEN = 49,
		ELDER_ROOSTER = 50,
		HUNTER_SLIME = 51,
		HUNTER_PLORT = 52,
		PINK_HUNTER_LARGO = 53,
		BOOM_HUNTER_LARGO = 54,
		RAD_HUNTER_LARGO = 55,
		ROCK_HUNTER_LARGO = 56,
		PHOSPHOR_HUNTER_LARGO = 57,
		TABBY_HUNTER_LARGO = 58,
		HONEY_SLIME = 59,
		HONEY_PLORT = 60,
		PINK_HONEY_LARGO = 61,
		HONEY_HUNTER_LARGO = 62,
		HONEY_BOOM_LARGO = 63,
		HONEY_RAD_LARGO = 64,
		HONEY_ROCK_LARGO = 65,
		HONEY_PHOSPHOR_LARGO = 66,
		HONEY_TABBY_LARGO = 67,
		STONY_HEN = 68,
		BRIAR_HEN = 69,
		STONY_CHICK = 70,
		BRIAR_CHICK = 71,
		PUDDLE_SLIME = 72,
		PUDDLE_PLORT = 73,
		DAILY_EXCHANGE_CRATE = 74,
		SPECIAL_EXCHANGE_CRATE = 75,
		KEY = 76,
		LUCKY_SLIME = 77,
		CRYSTAL_PLORT = 78,
		CRYSTAL_SLIME = 79,
		PINK_CRYSTAL_LARGO = 80,
		ROCK_CRYSTAL_LARGO = 81,
		TABBY_CRYSTAL_LARGO = 82,
		PHOSPHOR_CRYSTAL_LARGO = 83,
		BOOM_CRYSTAL_LARGO = 84,
		RAD_CRYSTAL_LARGO = 85,
		HONEY_CRYSTAL_LARGO = 86,
		HUNTER_CRYSTAL_LARGO = 87,
		ONION_VEGGIE = 88,
		QUANTUM_SLIME = 89,
		PINK_QUANTUM_LARGO = 90,
		QUANTUM_BOOM_LARGO = 91,
		QUANTUM_CRYSTAL_LARGO = 92,
		QUANTUM_HONEY_LARGO = 93,
		QUANTUM_HUNTER_LARGO = 94,
		QUANTUM_PHOSPHOR_LARGO = 95,
		QUANTUM_RAD_LARGO = 96,
		QUANTUM_ROCK_LARGO = 97,
		QUANTUM_TABBY_LARGO = 98,
		QUANTUM_PLORT = 99,
		LEMON_FRUIT = 100,
		LEMON_PHASE = 101,
		DERVISH_SLIME = 102,
		DERVISH_PLORT = 103,
		MOSAIC_SLIME = 104,
		MOSAIC_PLORT = 105,
		TANGLE_SLIME = 106,
		TANGLE_PLORT = 107,
		FIRE_SLIME = 108,
		FIRE_PLORT = 109,
		PAINTED_HEN = 110,
		PAINTED_CHICK = 111,
		POLLEN_CLOUD = 112,
		MAGIC_WATER_LIQUID = 113,
		FIRE_COLUMN = 114,
		PINK_TANGLE_LARGO = 115,
		QUANTUM_TANGLE_LARGO = 116,
		HONEY_TANGLE_LARGO = 117,
		PHOSPHOR_TANGLE_LARGO = 118,
		TANGLE_BOOM_LARGO = 119,
		TANGLE_RAD_LARGO = 120,
		TANGLE_ROCK_LARGO = 121,
		TANGLE_TABBY_LARGO = 122,
		TANGLE_HUNTER_LARGO = 123,
		TANGLE_CRYSTAL_LARGO = 124,
		PINK_MOSAIC_LARGO = 125,
		QUANTUM_MOSAIC_LARGO = 126,
		HONEY_MOSAIC_LARGO = 127,
		PHOSPHOR_MOSAIC_LARGO = 128,
		MOSAIC_TANGLE_LARGO = 129,
		MOSAIC_BOOM_LARGO = 130,
		MOSAIC_RAD_LARGO = 131,
		MOSAIC_ROCK_LARGO = 132,
		MOSAIC_TABBY_LARGO = 133,
		MOSAIC_HUNTER_LARGO = 134,
		MOSAIC_CRYSTAL_LARGO = 135,
		PINK_DERVISH_LARGO = 136,
		QUANTUM_DERVISH_LARGO = 137,
		HONEY_DERVISH_LARGO = 138,
		PHOSPHOR_DERVISH_LARGO = 139,
		TANGLE_DERVISH_LARGO = 140,
		MOSAIC_DERVISH_LARGO = 141,
		BOOM_DERVISH_LARGO = 142,
		RAD_DERVISH_LARGO = 143,
		ROCK_DERVISH_LARGO = 144,
		TABBY_DERVISH_LARGO = 145,
		HUNTER_DERVISH_LARGO = 146,
		CRYSTAL_DERVISH_LARGO = 147,
		GINGER_VEGGIE = 148,
		SPICY_TOFU = 149,
		SABER_SLIME = 150,
		SABER_PINK_LARGO = 151,
		SABER_QUANTUM_LARGO = 152,
		SABER_HONEY_LARGO = 153,
		SABER_PHOSPHOR_LARGO = 154,
		SABER_TANGLE_LARGO = 155,
		SABER_MOSAIC_LARGO = 156,
		SABER_BOOM_LARGO = 157,
		SABER_RAD_LARGO = 158,
		SABER_ROCK_LARGO = 159,
		SABER_TABBY_LARGO = 160,
		SABER_HUNTER_LARGO = 161,
		SABER_CRYSTAL_LARGO = 162,
		SABER_DERVISH_LARGO = 163,
		SABER_PLORT = 164,
		KOOKADOBA_FRUIT = 165,
		QUICKSILVER_SLIME = 166,
		QUICKSILVER_PLORT = 167,
		KOOKADOBA_BALL = 168,
		CRATE_RUINS_01 = 169,
		CRATE_WILDS_01 = 170,
		VALLEY_AMMO_1 = 171,
		VALLEY_AMMO_2 = 172,
		VALLEY_AMMO_3 = 173,
		VALLEY_AMMO_4 = 174,
		PORTABLE_SCARECROW = 175,
		RAD_GORDO = 10000,
		ROCK_GORDO = 10001,
		PINK_GORDO = 10002,
		BOOM_GORDO = 10003,
		PHOSPHOR_GORDO = 10004,
		TABBY_GORDO = 10005,
		HUNTER_GORDO = 10006,
		HONEY_GORDO = 10007,
		PUDDLE_GORDO = 10008,
		CRYSTAL_GORDO = 10009,
		QUANTUM_GORDO = 10010,
		DERVISH_GORDO = 10011,
		MOSAIC_GORDO = 10012,
		TANGLE_GORDO = 10013,
		GOLD_GORDO = 10014,
		PRIMORDY_OIL_CRAFT = 11000,
		DEEP_BRINE_CRAFT = 11001,
		SPIRAL_STEAM_CRAFT = 11002,
		LAVA_DUST_CRAFT = 11003,
		BUZZ_WAX_CRAFT = 11004,
		WILD_HONEY_CRAFT = 11005,
		HEXACOMB_CRAFT = 11006,
		ROYAL_JELLY_CRAFT = 11007,
		JELLYSTONE_CRAFT = 11008,
		INDIGONIUM_CRAFT = 11009,
		SLIME_FOSSIL_CRAFT = 11010,
		STRANGE_DIAMOND_CRAFT = 11011,
		RED_ECHO = 11012,
		GREEN_ECHO = 11013,
		BLUE_ECHO = 11014,
		GOLD_ECHO = 11015,
		SILKY_SAND_CRAFT = 11016,
		PEPPER_JAM_CRAFT = 11017,
		GLASS_SHARD_CRAFT = 11018,
		MANIFOLD_CUBE_CRAFT = 11019,
		HANDLEBAR_FASHION = 12000,
		SHADY_FASHION = 12001,
		CLIP_ON_FASHION = 12002,
		GOOGLY_FASHION = 12003,
		SERIOUS_FASHION = 12004,
		SMART_FASHION = 12005,
		CUTE_FASHION = 12006,
		ROYAL_FASHION = 12007,
		DANDY_FASHION = 12008,
		PARTY_FASHION = 12009,
		PIRATEY_FASHION = 12010,
		HEROIC_FASHION = 12011,
		SCIFI_FASHION = 12012,
		SCUBA_FASHION = 12013,
		PARTY_GLASSES_FASHION = 12014,
		REMOVER_FASHION = 12099,
		BEACH_BALL_TOY = 13000,
		BIG_ROCK_TOY = 13001,
		YARN_BALL_TOY = 13002,
		NIGHT_LIGHT_TOY = 13003,
		POWER_CELL_TOY = 13004,
		BOMB_BALL_TOY = 13005,
		BUZZY_BEE_TOY = 13006,
		RUBBER_DUCKY_TOY = 13007,
		CRYSTAL_BALL_TOY = 13008,
		STUFFED_CHICKEN_TOY = 13009,
		PUZZLE_CUBE_TOY = 13010,
		DISCO_BALL_TOY = 13011,
		GYRO_TOP_TOY = 13012,
		SOL_MATE_TOY = 13013,
		CHARCOAL_BRICK_TOY = 13014,
		STEGO_BUDDY_TOY = 13015,
		TREASURE_CHEST_TOY = 13016,
		BOP_GOBLIN_TOY = 13017,
		ROBOT_TOY = 13018,
		OCTO_BUDDY_TOY = 13019,
		PINK_ORNAMENT = 14000,
		ROCK_ORNAMENT = 14001,
		TABBY_ORNAMENT = 14002,
		PHOSPHOR_ORNAMENT = 14003,
		RAD_ORNAMENT = 14004,
		BOOM_ORNAMENT = 14005,
		HONEY_ORNAMENT = 14006,
		HUNTER_ORNAMENT = 14007,
		QUANTUM_ORNAMENT = 14008,
		PUDDLE_ORNAMENT = 14009,
		TANGLE_ORNAMENT = 14010,
		DERVISH_ORNAMENT = 14011,
		MOSAIC_ORNAMENT = 14012,
		LUCKY_ORNAMENT = 14013,
		GOLD_ORNAMENT = 14014,
		TARR_ORNAMENT = 14015,
		STACHE_ORNAMENT = 14016,
		CRYSTAL_ORNAMENT = 14017,
		QUICKSILVER_ORNAMENT = 14018,
		FIRE_ORNAMENT = 14019,
		HENHEN_ORNAMENT = 14020,
		SEVENZ_ORNAMENT = 14021,
		CHEEVO_ORNAMENT = 14022,
		CLOUD_ORNAMENT = 14023,
		CLOVER_ORNAMENT = 14024,
		HEART_ORNAMENT = 14025,
		BRIAR_HEN_ORNAMENT = 14026,
		ELDER_HEN_ORNAMENT = 14027,
		PAINTED_HEN_ORNAMENT = 14028,
		STONY_HEN_ORNAMENT = 14029,
		JACK_ORNAMENT = 14030,
		NEWBUCK_ORNAMENT = 14031,
		PINK_PARTY_ORNAMENT = 14032,
		RAINBOW_ORNAMENT = 14033,
		SNOWFLAKE_ORNAMENT = 14034,
		STAR_ORNAMENT = 14035,
		STRIPES_GREEN_ORNAMENT = 14036,
		STRIPES_PURPLE_ORNAMENT = 14037,
		GLITCH_ORNAMENT = 14038,
		SABER_ORNAMENT = 14039,
		IMPOSTER_ORNAMENT = 14040,
		DRONE_ORNAMENT = 14041,
		DRONE_SLEEPY_ORNAMENT = 14042,
		STRIPES_RED_ORNAMENT = 14043,
		STRIPES_PINK_ORNAMENT = 14044,
		STRIPES_BLUE_ORNAMENT = 14045,
		STRIPES_TEAL_ORNAMENT = 14046,
		SUNNY_ORNAMENT = 14047,
		WILDFLOWER_ORNAMENT = 14048,
		FIREFLOWER_ORNAMENT = 14049,
		TARR_LANTERN_ORNAMENT = 14050,
		TWINKLE_ORNAMENT = 14051,
		SLIME_MOON_ORNAMENT = 14052,
		DUCKY_ORNAMENT = 14053,
		STEGO_ORNAMENT = 14054,
		BUZZY_ORNAMENT = 14055,
		IMPOSTER_TABBY_ORNAMENT = 14056,
		TREEFOX_ORNAMENT = 14057,
		PARTY_GORDO = 15000,
		CRATE_PARTY_01 = 15100,
		GLITCH_SLIME = 16000,
		GLITCH_DEBUG_SPRAY_LIQUID = 16001,
		GLITCH_BUG_REPORT = 16002,
		GLITCH_TARR_SLIME = 16003,
		GLITCH_TARR_PORTAL = 16004,
		ECHO_NOTE_01 = 17000,
		ECHO_NOTE_02 = 17001,
		ECHO_NOTE_03 = 17002,
		ECHO_NOTE_04 = 17003,
		ECHO_NOTE_05 = 17004,
		ECHO_NOTE_06 = 17005,
		ECHO_NOTE_07 = 17006,
		ECHO_NOTE_08 = 17007,
		ECHO_NOTE_09 = 17008,
		ECHO_NOTE_10 = 17009,
		ECHO_NOTE_11 = 17010,
		ECHO_NOTE_12 = 17011,
		ECHO_NOTE_13 = 17012
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public bool Equals(Id id1, Id id2)
		{
			return id1 == id2;
		}

		public int GetHashCode(Id id)
		{
			return (int)id;
		}
	}

	public delegate void OnDestroyListener(Identifiable obj);

	public static IdComparer idComparer;

	public Id id;

	private ActorModel model;

	private RegionMember member;

	private const string VEGGIE_SUFFIX = "_VEGGIE";

	private const string FRUIT_SUFFIX = "_FRUIT";

	private const string TOFU_SUFFIX = "_TOFU";

	private const string HEN_SUFFIX = "HEN";

	private const string ROOSTER_SUFFIX = "ROOSTER";

	private const string CHICK_SUFFIX = "_CHICK";

	private const string SLIME_SUFFIX = "_SLIME";

	private const string LARGO_SUFFIX = "_LARGO";

	private const string GORDO_SUFFIX = "_GORDO";

	private const string PLORT_SUFFIX = "_PLORT";

	private const string LIQUID_SUFFIX = "_LIQUID";

	private const string CRAFT_SUFFIX = "_CRAFT";

	private const string FASHION_SUFFIX = "_FASHION";

	private const string ECHO_SUFFIX = "_ECHO";

	private const string ECHO_NOTE_PREFIX = "ECHO_NOTE_";

	private const string ORNAMENT_SUFFIX = "_ORNAMENT";

	private const string TOY_SUFFIX = "_TOY";

	private const string TANGLE_STRING = "TANGLE";

	public static HashSet<Id> VEGGIE_CLASS;

	public static HashSet<Id> FRUIT_CLASS;

	public static HashSet<Id> MEAT_CLASS;

	public static HashSet<Id> TOFU_CLASS;

	public static HashSet<Id> SLIME_CLASS;

	public static HashSet<Id> LARGO_CLASS;

	public static HashSet<Id> GORDO_CLASS;

	public static HashSet<Id> PLORT_CLASS;

	public static HashSet<Id> FOOD_CLASS;

	public static HashSet<Id> NON_SLIMES_CLASS;

	public static HashSet<Id> CHICK_CLASS;

	public static HashSet<Id> LIQUID_CLASS;

	public static HashSet<Id> CRAFT_CLASS;

	public static HashSet<Id> FASHION_CLASS;

	public static HashSet<Id> ECHO_CLASS;

	public static HashSet<Id> ECHO_NOTE_CLASS;

	public static HashSet<Id> ORNAMENT_CLASS;

	public static HashSet<Id> TOY_CLASS;

	public static HashSet<Id> EATERS_CLASS;

	public static HashSet<Id> ALLERGY_FREE_CLASS;

	public static HashSet<Id> STANDARD_CRATE_CLASS;

	public static HashSet<Id> ELDER_CLASS;

	public static HashSet<Id> TARR_CLASS;

	public static HashSet<Id> BOOP_CLASS;

	public static readonly HashSet<Id> SCENE_OBJECTS;

	public OnDestroyListener NotifyOnDestroy;

	private static readonly Dictionary<Id, Gadget.Id> GADGET_NAME_DICT;

	public bool isPhysicsInitialized { get; private set; }

	static Identifiable()
	{
		idComparer = new IdComparer();
		VEGGIE_CLASS = new HashSet<Id>(idComparer);
		FRUIT_CLASS = new HashSet<Id>(idComparer);
		MEAT_CLASS = new HashSet<Id>(idComparer);
		TOFU_CLASS = new HashSet<Id>(idComparer);
		SLIME_CLASS = new HashSet<Id>(idComparer);
		LARGO_CLASS = new HashSet<Id>(idComparer);
		GORDO_CLASS = new HashSet<Id>(idComparer);
		PLORT_CLASS = new HashSet<Id>(idComparer);
		FOOD_CLASS = new HashSet<Id>(idComparer);
		NON_SLIMES_CLASS = new HashSet<Id>(idComparer);
		CHICK_CLASS = new HashSet<Id>(idComparer);
		LIQUID_CLASS = new HashSet<Id>(idComparer);
		CRAFT_CLASS = new HashSet<Id>(idComparer);
		FASHION_CLASS = new HashSet<Id>(idComparer);
		ECHO_CLASS = new HashSet<Id>(idComparer);
		ECHO_NOTE_CLASS = new HashSet<Id>(idComparer);
		ORNAMENT_CLASS = new HashSet<Id>(idComparer);
		TOY_CLASS = new HashSet<Id>(idComparer);
		EATERS_CLASS = new HashSet<Id>(idComparer);
		ALLERGY_FREE_CLASS = new HashSet<Id>(idComparer);
		STANDARD_CRATE_CLASS = new HashSet<Id>(idComparer)
		{
			Id.CRATE_DESERT_01,
			Id.CRATE_MOSS_01,
			Id.CRATE_QUARRY_01,
			Id.CRATE_REEF_01,
			Id.CRATE_RUINS_01,
			Id.CRATE_WILDS_01
		};
		ELDER_CLASS = new HashSet<Id>(idComparer)
		{
			Id.ELDER_HEN,
			Id.ELDER_ROOSTER
		};
		TARR_CLASS = new HashSet<Id>(idComparer)
		{
			Id.TARR_SLIME,
			Id.GLITCH_TARR_SLIME
		};
		BOOP_CLASS = new HashSet<Id>(idComparer)
		{
			Id.TABBY_SLIME,
			Id.PINK_TABBY_LARGO,
			Id.BOOM_TABBY_LARGO,
			Id.RAD_TABBY_LARGO,
			Id.ROCK_TABBY_LARGO,
			Id.PHOSPHOR_TABBY_LARGO,
			Id.TABBY_HUNTER_LARGO,
			Id.HONEY_TABBY_LARGO,
			Id.TABBY_CRYSTAL_LARGO,
			Id.QUANTUM_TABBY_LARGO,
			Id.TANGLE_TABBY_LARGO,
			Id.MOSAIC_TABBY_LARGO,
			Id.TABBY_DERVISH_LARGO,
			Id.SABER_TABBY_LARGO
		};
		SCENE_OBJECTS = new HashSet<Id>(idComparer)
		{
			Id.PLAYER,
			Id.FIRE_COLUMN,
			Id.SCARECROW,
			Id.PORTABLE_SCARECROW,
			Id.GLITCH_TARR_PORTAL
		};
		GADGET_NAME_DICT = new Dictionary<Id, Gadget.Id>(idComparer)
		{
			{
				Id.HANDLEBAR_FASHION,
				Gadget.Id.FASHION_POD_HANDLEBAR
			},
			{
				Id.SHADY_FASHION,
				Gadget.Id.FASHION_POD_SHADY
			},
			{
				Id.CLIP_ON_FASHION,
				Gadget.Id.FASHION_POD_CLIP_ON
			},
			{
				Id.GOOGLY_FASHION,
				Gadget.Id.FASHION_POD_GOOGLY
			},
			{
				Id.SERIOUS_FASHION,
				Gadget.Id.FASHION_POD_SERIOUS
			},
			{
				Id.SMART_FASHION,
				Gadget.Id.FASHION_POD_SMART
			},
			{
				Id.CUTE_FASHION,
				Gadget.Id.FASHION_POD_CUTE
			},
			{
				Id.ROYAL_FASHION,
				Gadget.Id.FASHION_POD_ROYAL
			},
			{
				Id.DANDY_FASHION,
				Gadget.Id.FASHION_POD_DANDY
			},
			{
				Id.PIRATEY_FASHION,
				Gadget.Id.FASHION_POD_PIRATEY
			},
			{
				Id.HEROIC_FASHION,
				Gadget.Id.FASHION_POD_HEROIC
			},
			{
				Id.SCIFI_FASHION,
				Gadget.Id.FASHION_POD_SCIFI
			},
			{
				Id.SCUBA_FASHION,
				Gadget.Id.FASHION_POD_SCUBA
			},
			{
				Id.PARTY_GLASSES_FASHION,
				Gadget.Id.FASHION_POD_PARTY_GLASSES
			},
			{
				Id.REMOVER_FASHION,
				Gadget.Id.FASHION_POD_REMOVER
			}
		};
		foreach (Id value in Enum.GetValues(typeof(Id)))
		{
			string text = Enum.GetName(typeof(Id), value);
			if (text.EndsWith("_VEGGIE"))
			{
				VEGGIE_CLASS.Add(value);
				FOOD_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (text.EndsWith("_FRUIT"))
			{
				FRUIT_CLASS.Add(value);
				FOOD_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (text.EndsWith("_TOFU"))
			{
				TOFU_CLASS.Add(value);
				FOOD_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (text.EndsWith("_SLIME"))
			{
				SLIME_CLASS.Add(value);
			}
			else if (text.EndsWith("_LARGO"))
			{
				LARGO_CLASS.Add(value);
			}
			else if (text.EndsWith("_GORDO"))
			{
				GORDO_CLASS.Add(value);
			}
			else if (text.EndsWith("_PLORT"))
			{
				PLORT_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (text.EndsWith("HEN") || text.EndsWith("ROOSTER"))
			{
				MEAT_CLASS.Add(value);
				FOOD_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (value == Id.CHICK || text.EndsWith("_CHICK"))
			{
				NON_SLIMES_CLASS.Add(value);
				CHICK_CLASS.Add(value);
			}
			else if (text.EndsWith("_LIQUID"))
			{
				LIQUID_CLASS.Add(value);
			}
			else if (text.EndsWith("_CRAFT"))
			{
				CRAFT_CLASS.Add(value);
				NON_SLIMES_CLASS.Add(value);
			}
			else if (text.EndsWith("_FASHION"))
			{
				FASHION_CLASS.Add(value);
			}
			else if (text.EndsWith("_ECHO"))
			{
				ECHO_CLASS.Add(value);
			}
			else if (text.StartsWith("ECHO_NOTE_"))
			{
				ECHO_NOTE_CLASS.Add(value);
			}
			else if (text.EndsWith("_ORNAMENT"))
			{
				ORNAMENT_CLASS.Add(value);
			}
			else if (text.EndsWith("_TOY") || value == Id.KOOKADOBA_BALL)
			{
				TOY_CLASS.Add(value);
			}
			if (text.Contains("TANGLE"))
			{
				ALLERGY_FREE_CLASS.Add(value);
			}
		}
		EATERS_CLASS.UnionWith(SLIME_CLASS);
		EATERS_CLASS.UnionWith(LARGO_CLASS);
	}

	private IEnumerator SetPhysicsInitialized()
	{
		yield return new WaitForFixedUpdate();
		isPhysicsInitialized = true;
	}

	public void Awake()
	{
		member = GetComponent<RegionMember>();
		if (id == Id.PLAYER)
		{
			SRSingleton<SceneContext>.Instance.Player = base.gameObject;
		}
		if (SCENE_OBJECTS.Contains(id))
		{
			SRSingleton<SceneContext>.Instance.GameModel.RegisterStartingActor(base.gameObject, GetStartingActorRegion());
		}
	}

	private RegionRegistry.RegionSetId GetStartingActorRegion()
	{
		switch (id)
		{
		case Id.PLAYER:
			return RegionRegistry.RegionSetId.HOME;
		case Id.FIRE_COLUMN:
			return RegionRegistry.RegionSetId.DESERT;
		case Id.GLITCH_TARR_PORTAL:
			return RegionRegistry.RegionSetId.SLIMULATIONS;
		case Id.SCARECROW:
		case Id.PORTABLE_SCARECROW:
			return GetComponentInParent<Region>().setId;
		default:
			throw new ArgumentException($"Failed to get RegionSetId for starting actor. [id={id}]");
		}
	}

	public void OnEnable()
	{
		StartCoroutine(SetPhysicsInitialized());
	}

	public void OnDisable()
	{
		isPhysicsInitialized = false;
	}

	public void OnDestroy()
	{
		if (model != null)
		{
			CellDirector.UnregisterFromAll(member, base.gameObject, model);
		}
		member.regionsChanged -= OnMemberChanged_UpdateRegistration;
		if (NotifyOnDestroy != null)
		{
			NotifyOnDestroy(this);
		}
	}

	private static CellDirector GetDirector(Region region)
	{
		return region.cellDir;
	}

	public static bool IsSlime(Id id)
	{
		if (!SLIME_CLASS.Contains(id))
		{
			return LARGO_CLASS.Contains(id);
		}
		return true;
	}

	public static bool IsGordo(Id id)
	{
		return GORDO_CLASS.Contains(id);
	}

	public static bool IsLargo(Id id)
	{
		return LARGO_CLASS.Contains(id);
	}

	public static bool IsPlort(Id id)
	{
		return PLORT_CLASS.Contains(id);
	}

	public static bool IsAnimal(Id id)
	{
		if (!MEAT_CLASS.Contains(id))
		{
			return CHICK_CLASS.Contains(id);
		}
		return true;
	}

	public static bool IsChick(Id id)
	{
		return CHICK_CLASS.Contains(id);
	}

	public static bool IsFood(Id id)
	{
		return FOOD_CLASS.Contains(id);
	}

	public static bool IsVeggie(Id id)
	{
		return VEGGIE_CLASS.Contains(id);
	}

	public static bool IsFruit(Id id)
	{
		return FRUIT_CLASS.Contains(id);
	}

	public static bool IsCraft(Id id)
	{
		return CRAFT_CLASS.Contains(id);
	}

	public static bool IsEcho(Id id)
	{
		return ECHO_CLASS.Contains(id);
	}

	public static bool IsEchoNote(Id id)
	{
		return ECHO_NOTE_CLASS.Contains(id);
	}

	public static bool IsOrnament(Id id)
	{
		return ORNAMENT_CLASS.Contains(id);
	}

	public static bool IsToy(Id id)
	{
		return TOY_CLASS.Contains(id);
	}

	public static bool IsLiquid(Id id)
	{
		return LIQUID_CLASS.Contains(id);
	}

	public static bool IsWater(Id id)
	{
		if (id != Id.GLITCH_DEBUG_SPRAY_LIQUID)
		{
			return LIQUID_CLASS.Contains(id);
		}
		return false;
	}

	public static bool IsFashion(Id id)
	{
		return FASHION_CLASS.Contains(id);
	}

	public static bool IsNonSlimeResource(Id id)
	{
		return NON_SLIMES_CLASS.Contains(id);
	}

	public static bool IsAllergyFree(Id id)
	{
		return ALLERGY_FREE_CLASS.Contains(id);
	}

	public static bool IsTarr(Id id)
	{
		return TARR_CLASS.Contains(id);
	}

	public static Id Combine(List<Id> ids)
	{
		if (ids.Count == 0)
		{
			return Id.NONE;
		}
		if (ids.Count == 1)
		{
			return ids[0];
		}
		if (ids.Count == 2)
		{
			string text = Enum.GetName(typeof(Id), ids[0]);
			string text2 = Enum.GetName(typeof(Id), ids[1]);
			if (!text.EndsWith("_SLIME") || !text2.EndsWith("_SLIME"))
			{
				throw new ArgumentException(string.Concat("Illegal identifiable to combine, must be slime: ", ids[0], ",", ids[1]));
			}
			string text3 = text.Substring(0, text.Length - "_SLIME".Length);
			string text4 = text2.Substring(0, text2.Length - "_SLIME".Length);
			try
			{
				return (Id)Enum.Parse(typeof(Id), text3 + "_" + text4 + "_LARGO");
			}
			catch (Exception)
			{
				try
				{
					return (Id)Enum.Parse(typeof(Id), text4 + "_" + text3 + "_LARGO");
				}
				catch (Exception)
				{
					return Id.TARR_SLIME;
				}
			}
		}
		return Id.TARR_SLIME;
	}

	public static Id GetId(GameObject gameObject)
	{
		Identifiable component = gameObject.GetComponent<Identifiable>();
		if (component != null)
		{
			return component.id;
		}
		return Id.NONE;
	}

	public static long GetActorId(GameObject gameObject)
	{
		Identifiable component = gameObject.GetComponent<Identifiable>();
		if (component != null)
		{
			return component.GetActorId();
		}
		throw new InvalidOperationException("Tried to get an actor ID for an object that had none: " + gameObject.name);
	}

	public static string GetName(Id id, bool reportMissing = true)
	{
		if (GADGET_NAME_DICT.TryGetValue(id, out var value))
		{
			return Gadget.GetName(value, reportMissing);
		}
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("actor");
		string key = $"l.{Enum.GetName(typeof(Id), id).ToLowerInvariant()}";
		string resourceString = bundle.GetResourceString(key, reportMissing: false);
		if (resourceString != null)
		{
			return resourceString;
		}
		MessageBundle bundle2 = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia");
		key = $"t.{Enum.GetName(typeof(Id), id).ToLowerInvariant()}";
		resourceString = bundle2.GetResourceString(key, reportMissing: false);
		if (resourceString != null)
		{
			return resourceString;
		}
		PediaDirector.Id? pediaId = SRSingleton<SceneContext>.Instance.PediaDirector.GetPediaId(id);
		if (pediaId.HasValue)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia");
			key = $"t.{Enum.GetName(typeof(PediaDirector.Id), pediaId).ToLowerInvariant()}";
			if (resourceString != null)
			{
				return resourceString;
			}
		}
		if (reportMissing)
		{
			Log.Warning("Missing identifiable translation.", "id", id);
		}
		return null;
	}

	public long GetActorId()
	{
		return model.actorId;
	}

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(ActorModel model)
	{
		this.model = model;
		member.regionsChanged += OnMemberChanged_UpdateRegistration;
		foreach (Region region in member.regions)
		{
			region.cellDir.Register(base.gameObject, model);
		}
	}

	private void OnMemberChanged_UpdateRegistration(List<Region> left, List<Region> joined)
	{
		if (left != null)
		{
			foreach (Region item in left)
			{
				try
				{
					GetDirector(item).Unregister(base.gameObject, model);
				}
				catch (MissingReferenceException)
				{
				}
			}
		}
		if (joined == null)
		{
			return;
		}
		foreach (Region item2 in joined)
		{
			try
			{
				GetDirector(item2).Register(base.gameObject, model);
			}
			catch (MissingReferenceException)
			{
			}
		}
	}

	public static bool IsEdible(GameObject gameObject)
	{
		Identifiable component = gameObject.GetComponent<Identifiable>();
		if (component != null)
		{
			return component.IsEdible();
		}
		return false;
	}

	public bool IsEdible()
	{
		if (model != null)
		{
			return model.IsEdible();
		}
		return false;
	}
}
