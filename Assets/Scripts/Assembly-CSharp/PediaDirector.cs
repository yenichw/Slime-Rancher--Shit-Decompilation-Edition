using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class PediaDirector : SRBehaviour, PediaModel.Participant
{
	public enum Id
	{
		TUTORIALS = 0,
		SLIMES = 1,
		RESOURCES = 2,
		RANCH = 3,
		WORLD = 4,
		SCIENCE = 5,
		SPLASH = 1000,
		BASICS = 1001,
		VACING = 1002,
		CAPTURETANKS = 1003,
		ENERGY = 1004,
		CORRALLING = 1005,
		FEEDING = 1006,
		PLORTS = 1007,
		SSBASICS = 1008,
		GADGETMODE = 1009,
		WILDS_TUTORIAL = 1010,
		VALLEY_TUTORIAL = 1011,
		SLIMULATIONS_TUTORIAL = 1012,
		PINK_SLIME = 2000,
		ROCK_SLIME = 2001,
		PHOSPHOR_SLIME = 2002,
		TABBY_SLIME = 2003,
		RAD_SLIME = 2004,
		BOOM_SLIME = 2005,
		HUNTER_SLIME = 2006,
		HONEY_SLIME = 2007,
		PUDDLE_SLIME = 2008,
		CRYSTAL_SLIME = 2009,
		TARR_SLIME = 2900,
		GOLD_SLIME = 2901,
		LUCKY_SLIME = 2902,
		LARGO_SLIME = 2980,
		GORDO_SLIME = 2981,
		FERAL_SLIME = 2982,
		QUANTUM_SLIME = 2983,
		FIRE_SLIME = 2984,
		MOSAIC_SLIME = 2985,
		DERVISH_SLIME = 2986,
		TANGLE_SLIME = 2987,
		SABER_SLIME = 2988,
		QUICKSILVER_SLIME = 2989,
		PARTY_GORDO_SLIME = 2990,
		ECHO_NOTE_GORDO_SLIME = 2991,
		GLITCH_SLIME = 2992,
		CARROT = 3000,
		OCAOCA = 3001,
		BEET = 3002,
		PARSNIP = 3003,
		POGO = 3004,
		MANGO = 3005,
		CUBERRY = 3006,
		PEAR = 3007,
		HENHEN = 3008,
		BRIAR_HEN = 3009,
		STONY_HEN = 3010,
		ROOSTRO = 3011,
		CHICKADOO = 3012,
		BRIAR_CHICKADOO = 3013,
		STONY_CHICKADOO = 3014,
		ELDER_HEN = 3015,
		ELDER_ROOSTRO = 3016,
		ONION = 3017,
		LEMON = 3018,
		PAINTED_HEN = 3019,
		PAINTED_CHICKADOO = 3020,
		GINGER = 3021,
		KOOKADOBA = 3022,
		SPICY_TOFU = 3023,
		PRIMORDY_OIL_CRAFT = 3900,
		DEEP_BRINE_CRAFT = 3901,
		SPIRAL_STEAM_CRAFT = 3902,
		LAVA_DUST_CRAFT = 3903,
		BUZZ_WAX_CRAFT = 3904,
		WILD_HONEY_CRAFT = 3905,
		HEXACOMB_CRAFT = 3906,
		ROYAL_JELLY_CRAFT = 3907,
		JELLYSTONE_CRAFT = 3908,
		INDIGONIUM_CRAFT = 3909,
		SLIME_FOSSIL_CRAFT = 3910,
		STRANGE_DIAMOND_CRAFT = 3911,
		ECHOES = 3912,
		SLIME_TOYS = 3913,
		SILKY_SAND_CRAFT = 3914,
		PEPPER_JAM_CRAFT = 3915,
		GLASS_SHARD_CRAFT = 3916,
		ECHO_NOTES = 3917,
		MANIFOLD_CUBE_CRAFT = 3918,
		ORNAMENTS = 3919,
		CORRAL = 4000,
		COOP = 4001,
		GARDEN = 4002,
		SILO = 4003,
		INCINERATOR = 4004,
		POND = 4005,
		PLORT_MARKET = 4006,
		OVERGROWTH = 4007,
		GROTTO = 4008,
		LAB = 4009,
		CHROMA = 4010,
		PARTNER = 4011,
		DOCKS = 4012,
		OGDEN_RETREAT = 4013,
		MOCHI_MANOR = 4014,
		VALLEY = 4015,
		VIKTOR_LAB = 4016,
		REEF = 5000,
		QUARRY = 5001,
		MOSS = 5002,
		DESERT = 5003,
		SEA = 5004,
		THE_RANCH = 5005,
		KEYS = 5006,
		EXTRACTORS = 5007,
		UTILITIES = 5008,
		WARP_TECH = 5009,
		DECORATIONS = 5010,
		CURIOS = 5011,
		REFINERY = 5012,
		FABRICATOR = 5013,
		BLUEPRINTS = 5014,
		RUINS = 5015,
		WILDS = 5016,
		SLIMULATIONS_WORLD = 5017,
		DRONES = 6010,
		LOCKED = 10000
	}

	[Serializable]
	public class IdEntry
	{
		public Id id;

		public Sprite icon;
	}

	[Serializable]
	public class IdentMapEntry
	{
		public Identifiable.Id identId;

		public Id pediaId;
	}

	private class PediaPopupCreator : PopupDirector.PopupCreator
	{
		private PediaDirector pediaDir;

		private Id id;

		public PediaPopupCreator(PediaDirector pediaDir, Id id)
		{
			this.pediaDir = pediaDir;
			this.id = id;
		}

		public override void Create()
		{
			pediaDir.pediaModel.Unlock(id);
			PediaPopupUI.CreatePediaPopup(pediaDir.Get(id));
		}

		public override bool Equals(object other)
		{
			if (other is PediaPopupCreator)
			{
				return ((PediaPopupCreator)other).id == id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override bool ShouldClear()
		{
			return false;
		}
	}

	public static HashSet<Id> HIDDEN_ENTRIES = new HashSet<Id>
	{
		Id.PARTY_GORDO_SLIME,
		Id.ECHO_NOTE_GORDO_SLIME,
		Id.ECHO_NOTES
	};

	public const Id DEFAULT_PEDIA_ENTRY = Id.BASICS;

	public Sprite unknownIcon;

	public IdEntry lockedEntry;

	public IdEntry[] entries;

	public Id[] initUnlocked;

	public IdentMapEntry[] identMapEntries;

	public GameObject pediaPopupPrefab;

	public GameObject pediaPanelPrefab;

	public GameObject pediaListingPrefab;

	private Dictionary<Identifiable.Id, Id> identDict = new Dictionary<Identifiable.Id, Id>(Identifiable.idComparer);

	private Dictionary<Id, IdEntry> entryDict = new Dictionary<Id, IdEntry>();

	private AchievementsDirector achieveDir;

	private PopupDirector popupDir;

	private GameObject pediaUiObject;

	private PediaModel pediaModel;

	public IEnumerable<Id> GetInitUnlocked()
	{
		return initUnlocked;
	}

	public void OnUnlockedChanged(HashSet<Id> unlocked)
	{
		if (entries.All((IdEntry e) => unlocked.Contains(e.id) || HIDDEN_ENTRIES.Contains(e.id)))
		{
			achieveDir.MaybeUpdateMaxStat(AchievementsDirector.IntStat.COMPLETED_SLIMEPEDIA, 1);
		}
	}

	public void UnlockScience()
	{
		pediaModel.Unlock(Id.REFINERY, Id.FABRICATOR, Id.BLUEPRINTS, Id.EXTRACTORS, Id.UTILITIES, Id.WARP_TECH, Id.DECORATIONS, Id.CURIOS, Id.DRONES, Id.SSBASICS, Id.GADGETMODE);
	}

	public void Unlock(params Id[] ids)
	{
		pediaModel.Unlock(ids);
	}

	public void Awake()
	{
		IdEntry[] array = entries;
		foreach (IdEntry idEntry in array)
		{
			entryDict[idEntry.id] = idEntry;
		}
		IdentMapEntry[] array2 = identMapEntries;
		foreach (IdentMapEntry identMapEntry in array2)
		{
			identDict[identMapEntry.identId] = identMapEntry.pediaId;
		}
		popupDir = SRSingleton<SceneContext>.Instance.PopupDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPedia(this);
	}

	public void InitModel(PediaModel pediaModel)
	{
		pediaModel.ResetUnlocked(initUnlocked);
	}

	public void SetModel(PediaModel pediaModel)
	{
		this.pediaModel = pediaModel;
	}

	public void Update()
	{
		if (SRInput.Actions.pedia.WasPressed)
		{
			Id pediaId = Id.BASICS;
			PediaPopupUI pediaPopupUI = UnityEngine.Object.FindObjectOfType<PediaPopupUI>();
			if (pediaPopupUI != null)
			{
				pediaId = pediaPopupUI.GetId();
				Destroyer.Destroy(pediaPopupUI.gameObject, "PediaDirector.Update");
			}
			ShowPedia(pediaId);
		}
	}

	public IdEntry Get(Id id)
	{
		if (IsUnlocked(id))
		{
			if (entryDict.ContainsKey(id))
			{
				return entryDict.Get(id);
			}
			return lockedEntry;
		}
		return lockedEntry;
	}

	public int GetUnlockedCount()
	{
		return pediaModel.unlocked.Count;
	}

	public void UnlockWithoutPopup(Id id)
	{
		pediaModel.Unlock(id);
	}

	public void MaybeShowPopup(Id id)
	{
		if (IsUnlocked(id))
		{
			return;
		}
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser)
		{
			pediaModel.unlocked.Add(id);
			return;
		}
		PediaPopupCreator creator = new PediaPopupCreator(this, id);
		if (!popupDir.IsQueued(creator))
		{
			popupDir.QueueForPopup(creator);
			popupDir.MaybePopupNext();
		}
	}

	public void MaybeShowPopup(Identifiable.Id identId)
	{
		if (identDict.ContainsKey(identId))
		{
			MaybeShowPopup(identDict[identId]);
		}
	}

	public GameObject ShowPedia(Id pediaId)
	{
		pediaUiObject = UnityEngine.Object.Instantiate(pediaPanelPrefab);
		PediaUI component = pediaUiObject.GetComponent<PediaUI>();
		component.SelectEntry(pediaId, selectTab: true, pediaId);
		return component.gameObject;
	}

	public bool IsPediaOpen()
	{
		return pediaUiObject != null;
	}

	public Id? GetPediaId(Identifiable.Id identId)
	{
		if (identDict.ContainsKey(identId))
		{
			return identDict[identId];
		}
		return null;
	}

	public bool IsUnlocked(Id id)
	{
		return pediaModel.unlocked.Contains(id);
	}
}
