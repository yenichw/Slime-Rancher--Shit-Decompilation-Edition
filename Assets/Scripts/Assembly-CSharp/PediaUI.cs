using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PediaUI : BaseUI
{
	public RectTransform listingPanel;

	public ScrollRect listingScroller;

	public TMP_Text titleText;

	public TMP_Text introText;

	public Image image;

	public ScrollRect descScroller;

	public TMP_Text longDescText;

	public TMP_Text dietText;

	public TMP_Text favoriteText;

	public TMP_Text biologyText;

	public TMP_Text risksText;

	public TMP_Text plortText;

	public TMP_Text instructionsText;

	public TMP_Text vacDescText;

	public TMP_Text resourceTypeText;

	public TMP_Text favoredByLabel;

	public TMP_Text favoredByText;

	public TMP_Text howToUseText;

	public GameObject howToUseArea;

	public TMP_Text resourceDescText;

	public RectTransform upgradesPanel;

	public TMP_Text ranchDescText;

	public Toggle vacpackTab;

	public Toggle slimesTab;

	public Toggle resourcesTab;

	public Toggle ranchTab;

	public Toggle worldTab;

	public Toggle scienceTab;

	public TabByMenuKeys tabs;

	public RectTransform genericDescPanel;

	public RectTransform vacpackDescPanel;

	public RectTransform slimesDescPanel;

	public RectTransform resourcesDescPanel;

	public RectTransform ranchDescPanel;

	private PediaDirector pediaDir;

	private MessageBundle pediaBundle;

	private static PediaDirector.Id[] TUTORIALS_ENTRIES = new PediaDirector.Id[12]
	{
		PediaDirector.Id.BASICS,
		PediaDirector.Id.VACING,
		PediaDirector.Id.CAPTURETANKS,
		PediaDirector.Id.ENERGY,
		PediaDirector.Id.CORRALLING,
		PediaDirector.Id.FEEDING,
		PediaDirector.Id.PLORTS,
		PediaDirector.Id.SSBASICS,
		PediaDirector.Id.GADGETMODE,
		PediaDirector.Id.WILDS_TUTORIAL,
		PediaDirector.Id.VALLEY_TUTORIAL,
		PediaDirector.Id.SLIMULATIONS_TUTORIAL
	};

	private static PediaDirector.Id[] SLIMES_ENTRIES = new PediaDirector.Id[26]
	{
		PediaDirector.Id.PINK_SLIME,
		PediaDirector.Id.ROCK_SLIME,
		PediaDirector.Id.TABBY_SLIME,
		PediaDirector.Id.PHOSPHOR_SLIME,
		PediaDirector.Id.RAD_SLIME,
		PediaDirector.Id.BOOM_SLIME,
		PediaDirector.Id.HONEY_SLIME,
		PediaDirector.Id.PUDDLE_SLIME,
		PediaDirector.Id.CRYSTAL_SLIME,
		PediaDirector.Id.HUNTER_SLIME,
		PediaDirector.Id.QUANTUM_SLIME,
		PediaDirector.Id.FIRE_SLIME,
		PediaDirector.Id.DERVISH_SLIME,
		PediaDirector.Id.TANGLE_SLIME,
		PediaDirector.Id.MOSAIC_SLIME,
		PediaDirector.Id.SABER_SLIME,
		PediaDirector.Id.QUICKSILVER_SLIME,
		PediaDirector.Id.GLITCH_SLIME,
		PediaDirector.Id.GOLD_SLIME,
		PediaDirector.Id.LUCKY_SLIME,
		PediaDirector.Id.LARGO_SLIME,
		PediaDirector.Id.GORDO_SLIME,
		PediaDirector.Id.PARTY_GORDO_SLIME,
		PediaDirector.Id.ECHO_NOTE_GORDO_SLIME,
		PediaDirector.Id.FERAL_SLIME,
		PediaDirector.Id.TARR_SLIME
	};

	private static PediaDirector.Id[] RESOURCES_ENTRIES = new PediaDirector.Id[44]
	{
		PediaDirector.Id.CARROT,
		PediaDirector.Id.OCAOCA,
		PediaDirector.Id.BEET,
		PediaDirector.Id.PARSNIP,
		PediaDirector.Id.ONION,
		PediaDirector.Id.GINGER,
		PediaDirector.Id.POGO,
		PediaDirector.Id.MANGO,
		PediaDirector.Id.CUBERRY,
		PediaDirector.Id.LEMON,
		PediaDirector.Id.PEAR,
		PediaDirector.Id.KOOKADOBA,
		PediaDirector.Id.CHICKADOO,
		PediaDirector.Id.HENHEN,
		PediaDirector.Id.ROOSTRO,
		PediaDirector.Id.STONY_CHICKADOO,
		PediaDirector.Id.STONY_HEN,
		PediaDirector.Id.BRIAR_CHICKADOO,
		PediaDirector.Id.BRIAR_HEN,
		PediaDirector.Id.PAINTED_CHICKADOO,
		PediaDirector.Id.PAINTED_HEN,
		PediaDirector.Id.ELDER_HEN,
		PediaDirector.Id.ELDER_ROOSTRO,
		PediaDirector.Id.SPICY_TOFU,
		PediaDirector.Id.MANIFOLD_CUBE_CRAFT,
		PediaDirector.Id.PRIMORDY_OIL_CRAFT,
		PediaDirector.Id.DEEP_BRINE_CRAFT,
		PediaDirector.Id.SILKY_SAND_CRAFT,
		PediaDirector.Id.SPIRAL_STEAM_CRAFT,
		PediaDirector.Id.LAVA_DUST_CRAFT,
		PediaDirector.Id.BUZZ_WAX_CRAFT,
		PediaDirector.Id.WILD_HONEY_CRAFT,
		PediaDirector.Id.PEPPER_JAM_CRAFT,
		PediaDirector.Id.HEXACOMB_CRAFT,
		PediaDirector.Id.ROYAL_JELLY_CRAFT,
		PediaDirector.Id.JELLYSTONE_CRAFT,
		PediaDirector.Id.INDIGONIUM_CRAFT,
		PediaDirector.Id.GLASS_SHARD_CRAFT,
		PediaDirector.Id.SLIME_FOSSIL_CRAFT,
		PediaDirector.Id.STRANGE_DIAMOND_CRAFT,
		PediaDirector.Id.ECHOES,
		PediaDirector.Id.SLIME_TOYS,
		PediaDirector.Id.ECHO_NOTES,
		PediaDirector.Id.ORNAMENTS
	};

	private static PediaDirector.Id[] RANCH_ENTRIES = new PediaDirector.Id[16]
	{
		PediaDirector.Id.CORRAL,
		PediaDirector.Id.COOP,
		PediaDirector.Id.GARDEN,
		PediaDirector.Id.SILO,
		PediaDirector.Id.INCINERATOR,
		PediaDirector.Id.POND,
		PediaDirector.Id.PLORT_MARKET,
		PediaDirector.Id.OVERGROWTH,
		PediaDirector.Id.GROTTO,
		PediaDirector.Id.DOCKS,
		PediaDirector.Id.LAB,
		PediaDirector.Id.OGDEN_RETREAT,
		PediaDirector.Id.MOCHI_MANOR,
		PediaDirector.Id.VIKTOR_LAB,
		PediaDirector.Id.PARTNER,
		PediaDirector.Id.CHROMA
	};

	private static PediaDirector.Id[] WORLD_ENTRIES = new PediaDirector.Id[11]
	{
		PediaDirector.Id.THE_RANCH,
		PediaDirector.Id.REEF,
		PediaDirector.Id.QUARRY,
		PediaDirector.Id.MOSS,
		PediaDirector.Id.RUINS,
		PediaDirector.Id.DESERT,
		PediaDirector.Id.WILDS,
		PediaDirector.Id.VALLEY,
		PediaDirector.Id.SLIMULATIONS_WORLD,
		PediaDirector.Id.SEA,
		PediaDirector.Id.KEYS
	};

	public static PediaDirector.Id[] SCIENCE_ENTRIES = new PediaDirector.Id[9]
	{
		PediaDirector.Id.REFINERY,
		PediaDirector.Id.FABRICATOR,
		PediaDirector.Id.BLUEPRINTS,
		PediaDirector.Id.EXTRACTORS,
		PediaDirector.Id.UTILITIES,
		PediaDirector.Id.WARP_TECH,
		PediaDirector.Id.DECORATIONS,
		PediaDirector.Id.CURIOS,
		PediaDirector.Id.DRONES
	};

	private Dictionary<PediaDirector.Id, Toggle> listItems = new Dictionary<PediaDirector.Id, Toggle>();

	public override void Awake()
	{
		base.Awake();
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
		bool flag = true;
		PediaDirector.Id[] sCIENCE_ENTRIES = SCIENCE_ENTRIES;
		foreach (PediaDirector.Id id in sCIENCE_ENTRIES)
		{
			if (pediaDir.IsUnlocked(id))
			{
				flag = false;
			}
		}
		if (flag)
		{
			scienceTab.gameObject.SetActive(value: false);
		}
		SelectEntry(TUTORIALS_ENTRIES[0], selectTab: true, TUTORIALS_ENTRIES[0]);
	}

	public void InitBundles(MessageDirector msgDir)
	{
		pediaBundle = msgDir.GetBundle("pedia");
	}

	public void SelectVacpack(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(TUTORIALS_ENTRIES[0], selectTab: true, TUTORIALS_ENTRIES[0]);
		}
	}

	public void SelectSlimes(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(SLIMES_ENTRIES[0], selectTab: true, SLIMES_ENTRIES[0]);
		}
	}

	public void SelectResources(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(RESOURCES_ENTRIES[0], selectTab: true, RESOURCES_ENTRIES[0]);
		}
	}

	public void SelectRanch(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(RANCH_ENTRIES[0], selectTab: true, RANCH_ENTRIES[0]);
		}
	}

	public void SelectWorld(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(WORLD_ENTRIES[0], selectTab: true, WORLD_ENTRIES[0]);
		}
	}

	public void SelectScience(GameObject toggleObj)
	{
		if (toggleObj.GetComponent<Toggle>().isOn)
		{
			SelectEntry(SCIENCE_ENTRIES[0], selectTab: true, SCIENCE_ENTRIES[0]);
		}
	}

	public void SelectEntry(PediaDirector.Id id, bool selectTab, PediaDirector.Id listingId)
	{
		PediaDirector.IdEntry idEntry = pediaDir.Get(id);
		if (selectTab)
		{
			SelectTabForId(id);
		}
		if (idEntry == null)
		{
			Debug.Log("Missing Pedia entry, using fallback icons and text.");
		}
		string text = ((idEntry == null) ? "*UNKNOWN*" : Enum.GetName(typeof(PediaDirector.Id), idEntry.id).ToLowerInvariant());
		titleText.text = pediaBundle.Get("t." + text);
		introText.text = pediaBundle.Get("m.intro." + text);
		image.sprite = ((idEntry == null) ? pediaDir.unknownIcon : idEntry.icon);
		genericDescPanel.gameObject.SetActive(value: false);
		vacpackDescPanel.gameObject.SetActive(value: false);
		slimesDescPanel.gameObject.SetActive(value: false);
		resourcesDescPanel.gameObject.SetActive(value: false);
		ranchDescPanel.gameObject.SetActive(value: false);
		if (Array.IndexOf(TUTORIALS_ENTRIES, id) != -1 && id != PediaDirector.Id.SPLASH)
		{
			PopulateVacpackDesc(text);
		}
		else if (Array.IndexOf(SLIMES_ENTRIES, id) != -1)
		{
			PopulateSlimesDesc(text);
		}
		else if (Array.IndexOf(RESOURCES_ENTRIES, id) != -1)
		{
			PopulateResourcesDesc(text);
		}
		else if (Array.IndexOf(RANCH_ENTRIES, id) != -1)
		{
			PopulateRanchDesc(text);
		}
		else
		{
			PopulateGenericDesc(text);
		}
		if (listItems.ContainsKey(listingId))
		{
			Toggle toggle = listItems[listingId];
			if (toggle != null && EventSystem.current.currentSelectedGameObject != toggle.gameObject)
			{
				toggle.Select();
			}
			toggle.isOn = true;
		}
		StartCoroutine(DelayedResetScroller(descScroller));
	}

	private void PopulateVacpackDesc(string lowerName)
	{
		vacpackDescPanel.gameObject.SetActive(value: true);
		string text = "m.instructions.gamepad.";
		if (InputDirector.UsingGamepad() && pediaBundle.Exists(text + lowerName))
		{
			instructionsText.text = pediaBundle.Get(text + lowerName);
		}
		else
		{
			instructionsText.text = pediaBundle.Get("m.instructions." + lowerName);
		}
		vacDescText.text = pediaBundle.Get("m.desc." + lowerName);
	}

	private void PopulateSlimesDesc(string lowerName)
	{
		slimesDescPanel.gameObject.SetActive(value: true);
		dietText.text = pediaBundle.Get("m.diet." + lowerName);
		favoriteText.text = pediaBundle.Get("m.favorite." + lowerName);
		biologyText.text = pediaBundle.Get("m.slimeology." + lowerName);
		risksText.text = pediaBundle.Get("m.risks." + lowerName);
		plortText.text = pediaBundle.Get("m.plortonomics." + lowerName);
	}

	private void PopulateResourcesDesc(string lowerName)
	{
		resourcesDescPanel.gameObject.SetActive(value: true);
		resourceTypeText.text = pediaBundle.Get("m.resource_type." + lowerName);
		if (pediaBundle.Exists("l.favored_by." + lowerName))
		{
			favoredByLabel.text = pediaBundle.Get("l.favored_by." + lowerName);
		}
		else
		{
			favoredByLabel.text = uiBundle.Get("l.favored_by");
		}
		favoredByText.text = pediaBundle.Get("m.favored_by." + lowerName);
		bool flag = pediaBundle.Exists("m.how_to_use." + lowerName);
		if (flag)
		{
			howToUseText.text = pediaBundle.Get("m.how_to_use." + lowerName);
		}
		howToUseArea.SetActive(flag);
		resourceDescText.text = pediaBundle.Get("m.desc." + lowerName);
	}

	private void PopulateRanchDesc(string lowerName)
	{
		ranchDescPanel.gameObject.SetActive(value: true);
		for (int num = upgradesPanel.transform.childCount - 1; num >= 0; num--)
		{
			Destroyer.Destroy(upgradesPanel.GetChild(num).gameObject, "PediaUI.PopulateRanchDesc");
		}
		foreach (LandPlot.Upgrade value in Enum.GetValues(typeof(LandPlot.Upgrade)))
		{
			string text = Enum.GetName(typeof(LandPlot.Upgrade), value).ToLowerInvariant();
			string key = "m.upgrade.name." + lowerName + "." + text;
			if (pediaBundle.Exists(key))
			{
				GameObject obj = new GameObject("UpgradeNameText");
				obj.AddComponent<TextMeshProUGUI>().text = pediaBundle.Get(key);
				obj.AddComponent<MeshTextStyler>().SetStyle("LargeBold");
				obj.transform.SetParent(upgradesPanel, worldPositionStays: false);
				GameObject obj2 = new GameObject("UpgradeDescText");
				TextMeshProUGUI textMeshProUGUI = obj2.AddComponent<TextMeshProUGUI>();
				string key2 = "m.upgrade.desc." + lowerName + "." + text;
				textMeshProUGUI.text = pediaBundle.Get(key2);
				obj2.AddComponent<MeshTextStyler>().SetStyle("Default");
				obj2.transform.SetParent(upgradesPanel, worldPositionStays: false);
			}
		}
		ranchDescText.text = pediaBundle.Get("m.desc." + lowerName);
	}

	private void PopulateGenericDesc(string lowerName)
	{
		genericDescPanel.gameObject.SetActive(value: true);
		longDescText.text = pediaBundle.Get("m.desc." + lowerName);
	}

	private void SelectTabForId(PediaDirector.Id id)
	{
		if (id != PediaDirector.Id.LOCKED)
		{
			if (Array.IndexOf(TUTORIALS_ENTRIES, id) != -1 || id == PediaDirector.Id.TUTORIALS)
			{
				vacpackTab.isOn = true;
				BuildListing(TUTORIALS_ENTRIES);
			}
			else if (Array.IndexOf(SLIMES_ENTRIES, id) != -1 || id == PediaDirector.Id.SLIMES)
			{
				slimesTab.isOn = true;
				BuildListing(SLIMES_ENTRIES);
			}
			else if (Array.IndexOf(RESOURCES_ENTRIES, id) != -1 || id == PediaDirector.Id.RESOURCES)
			{
				resourcesTab.isOn = true;
				BuildListing(RESOURCES_ENTRIES);
			}
			else if (Array.IndexOf(RANCH_ENTRIES, id) != -1 || id == PediaDirector.Id.RANCH)
			{
				ranchTab.isOn = true;
				BuildListing(RANCH_ENTRIES);
			}
			else if (Array.IndexOf(WORLD_ENTRIES, id) != -1 || id == PediaDirector.Id.WORLD)
			{
				worldTab.isOn = true;
				BuildListing(WORLD_ENTRIES);
			}
			else if (Array.IndexOf(SCIENCE_ENTRIES, id) != -1 || id == PediaDirector.Id.SCIENCE)
			{
				scienceTab.isOn = true;
				BuildListing(SCIENCE_ENTRIES);
			}
			else
			{
				Log.Debug("Could not find tab for pedia ID, skipping.", "id", id);
			}
		}
		tabs.RecalcSelected();
	}

	private void BuildListing(PediaDirector.Id[] ids)
	{
		for (int i = 0; i < listingPanel.childCount; i++)
		{
			Destroyer.Destroy(listingPanel.GetChild(i).gameObject, "PediaUI.BuildListing");
		}
		listItems.Clear();
		ToggleGroup component = listingPanel.GetComponent<ToggleGroup>();
		bool flag = true;
		foreach (PediaDirector.Id id in ids)
		{
			if (!PediaDirector.HIDDEN_ENTRIES.Contains(id) || pediaDir.IsUnlocked(id))
			{
				PediaDirector.IdEntry entry = pediaDir.Get(id);
				GameObject gameObject = CreateListing(pediaDir.pediaListingPrefab, entry, id);
				gameObject.transform.SetParent(listingPanel, worldPositionStays: false);
				if (flag && gameObject.activeSelf)
				{
					flag = false;
					gameObject.AddComponent<InitSelected>();
				}
				gameObject.GetComponent<Toggle>().group = component;
				listItems[id] = gameObject.GetComponent<Toggle>();
			}
		}
		StartCoroutine(DelayedResetScroller(listingScroller));
	}

	private IEnumerator DelayedResetScroller(ScrollRect scroller)
	{
		yield return new WaitForEndOfFrame();
		scroller.verticalNormalizedPosition = 1f;
	}

	private GameObject CreateListing(GameObject prefab, PediaDirector.IdEntry entry, PediaDirector.Id listingId)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		TMP_Text component = gameObject.transform.Find("NameText").GetComponent<TMP_Text>();
		Image component2 = gameObject.transform.Find("Image").GetComponent<Image>();
		if (entry == null)
		{
			Debug.Log("Missing Pedia entry, using fallback icons and text.");
		}
		string text = ((entry == null) ? "*UNKNOWN*" : Enum.GetName(typeof(PediaDirector.Id), entry.id).ToLowerInvariant());
		component.text = pediaBundle.Xlate("t." + text);
		component2.sprite = ((entry == null) ? pediaDir.unknownIcon : entry.icon);
		PediaListingUI listingUI = gameObject.GetComponent<PediaListingUI>();
		listingUI.id = entry?.id ?? PediaDirector.Id.PINK_SLIME;
		OnSelectDelegator.Create(gameObject, delegate
		{
			SelectEntry(listingUI.id, selectTab: false, listingId);
		});
		return gameObject;
	}

	protected override bool Closeable()
	{
		return true;
	}
}
