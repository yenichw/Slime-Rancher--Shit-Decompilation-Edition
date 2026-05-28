using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class RanchDirector : MonoBehaviour, RanchModel.Participant
{
	public enum PaletteType
	{
		RANCH_TECH = 0,
		HOUSE = 1,
		VAC = 2,
		OGDEN_TECH = 3,
		OGDEN_HOUSE = 4,
		MOCHI_TECH = 5,
		MOCHI_HOUSE = 6,
		VALLEY = 7,
		VIKTOR_TECH = 8,
		VIKTOR_HOUSE = 9
	}

	public enum Palette
	{
		DEFAULT = 0,
		PALETTE01 = 1,
		PALETTE02 = 2,
		PALETTE03 = 3,
		PALETTE04 = 4,
		PALETTE05 = 5,
		PALETTE06 = 6,
		PALETTE07 = 7,
		PALETTE08 = 8,
		PALETTE09 = 9,
		PALETTE10 = 10,
		PALETTE11 = 11,
		PALETTE12 = 12,
		PALETTE13 = 13,
		PALETTE14 = 14,
		PALETTE15 = 15,
		PALETTE16 = 16,
		PALETTE17 = 17,
		PALETTE18 = 18,
		PALETTE19 = 19,
		PALETTE20 = 20,
		PALETTE21 = 21,
		PALETTE22 = 22,
		PALETTE23 = 23,
		PALETTE24 = 24,
		PALETTE25 = 25,
		PALETTE26 = 26,
		PALETTE27 = 27,
		PALETTE28 = 28,
		PALETTE29 = 29,
		PALETTE30 = 30,
		OGDEN = 1000,
		MOCHI = 1001,
		VIKTOR = 1002
	}

	[Serializable]
	public class PaletteEntry
	{
		public Palette palette;

		public Sprite icon;

		public int order;

		public int requiresPartnerLevel;

		public ProgressDirector.ProgressType requiresProgressType = ProgressDirector.ProgressType.NONE;

		public int requiresProgressCount;

		public Color redDark;

		public Color redLight;

		public Color greenDark;

		public Color greenLight;

		public Color blueDark;

		public Color blueLight;

		public Color blackDark;

		public Color blackLight;

		public Color magentaDark;

		public Color magentaLight;

		public Color yellowDark;

		public Color yellowLight;

		public Color cyanDark;

		public Color cyanLight;

		public Color whiteDark;

		public Color whiteLight;
	}

	private class Comparer : SRComparer<PaletteEntry>
	{
		public static Comparer<PaletteEntry> DEFAULT = from p in new Comparer()
			orderby p.palette == Palette.DEFAULT descending
			orderby p.requiresPartnerLevel > 0 descending
			orderby p.requiresPartnerLevel
			orderby p.order
			select p;
	}

	public Material[] ranchMats;

	public Material[] houseMats;

	public Material[] vacMats;

	private const float PARTNER_UNLOCK_TIME = 259200f;

	public PaletteEntry[] palettes;

	private Dictionary<Palette, PaletteEntry> paletteDict = new Dictionary<Palette, PaletteEntry>();

	private Dictionary<Material, Material> standardMatDict = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> ogdenMatDict = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> mochiMatDict = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> valleyMatDict = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> viktorMatDict = new Dictionary<Material, Material>();

	private List<PaletteEntry> orderedPalettes;

	private Dictionary<PaletteType, Material[]> recolorMats = new Dictionary<PaletteType, Material[]>();

	private List<Material> dynamicVacRecolorMats = new List<Material>();

	private ProgressDirector progressDir;

	private RanchModel model;

	public static string PARTNER_MAIL_KEY = "partner_rewards";

	public void Awake()
	{
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		paletteDict = palettes.ToDictionary((PaletteEntry e) => e.palette, (PaletteEntry e) => e);
		recolorMats[PaletteType.VALLEY] = new Material[ranchMats.Length + houseMats.Length];
		recolorMats[PaletteType.RANCH_TECH] = new Material[ranchMats.Length];
		recolorMats[PaletteType.OGDEN_TECH] = new Material[ranchMats.Length];
		recolorMats[PaletteType.MOCHI_TECH] = new Material[ranchMats.Length];
		recolorMats[PaletteType.VIKTOR_TECH] = new Material[ranchMats.Length];
		for (int i = 0; i < ranchMats.Length; i++)
		{
			Material material = ranchMats[i];
			standardMatDict[material] = (recolorMats[PaletteType.RANCH_TECH][i] = new Material(material));
			ogdenMatDict[material] = (recolorMats[PaletteType.OGDEN_TECH][i] = new Material(material));
			mochiMatDict[material] = (recolorMats[PaletteType.MOCHI_TECH][i] = new Material(material));
			valleyMatDict[material] = (recolorMats[PaletteType.VALLEY][i] = new Material(material));
			viktorMatDict[material] = (recolorMats[PaletteType.VIKTOR_TECH][i] = new Material(material));
		}
		recolorMats[PaletteType.HOUSE] = new Material[houseMats.Length];
		recolorMats[PaletteType.OGDEN_HOUSE] = new Material[houseMats.Length];
		recolorMats[PaletteType.MOCHI_HOUSE] = new Material[houseMats.Length];
		recolorMats[PaletteType.VIKTOR_HOUSE] = new Material[houseMats.Length];
		for (int j = 0; j < houseMats.Length; j++)
		{
			Material material2 = houseMats[j];
			standardMatDict[material2] = (recolorMats[PaletteType.HOUSE][j] = new Material(material2));
			ogdenMatDict[material2] = (recolorMats[PaletteType.OGDEN_HOUSE][j] = new Material(material2));
			mochiMatDict[material2] = (recolorMats[PaletteType.MOCHI_HOUSE][j] = new Material(material2));
			valleyMatDict[material2] = (recolorMats[PaletteType.VALLEY][j + ranchMats.Length] = new Material(material2));
			viktorMatDict[material2] = (recolorMats[PaletteType.VIKTOR_HOUSE][j] = new Material(material2));
		}
		recolorMats[PaletteType.VAC] = new Material[vacMats.Length];
		for (int k = 0; k < vacMats.Length; k++)
		{
			Material material3 = vacMats[k];
			Material material4 = new Material(material3);
			recolorMats[PaletteType.VAC][k] = material4;
			standardMatDict[material3] = material4;
		}
	}

	public void InitModel(RanchModel model)
	{
		ResetDefaults(model);
	}

	public void SetModel(RanchModel model)
	{
		this.model = model;
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterRanch(this);
	}

	public void OnDestroy()
	{
		foreach (Material value in standardMatDict.Values)
		{
			Destroyer.Destroy(value, "RanchDirector.OnDestroy(1)");
		}
		foreach (Material value2 in ogdenMatDict.Values)
		{
			Destroyer.Destroy(value2, "RanchDirector.OnDestroy(2)");
		}
		foreach (Material value3 in mochiMatDict.Values)
		{
			Destroyer.Destroy(value3, "RanchDirector.OnDestroy(3)");
		}
		foreach (Material value4 in valleyMatDict.Values)
		{
			Destroyer.Destroy(value4, "RanchDirector.OnDestroy(4)");
		}
		foreach (Material value5 in viktorMatDict.Values)
		{
			Destroyer.Destroy(value5, "RanchDirector.OnDestroy(5)");
		}
	}

	public void RegisterPalette(PaletteEntry entry)
	{
		paletteDict[entry.palette] = entry;
		orderedPalettes = null;
	}

	public bool IsPartnerUnlocked()
	{
		return progressDir.HasProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER_UNLOCK);
	}

	private void ResetDefaults(RanchModel model)
	{
		model.SelectPalette(PaletteType.HOUSE, Palette.DEFAULT);
		model.SelectPalette(PaletteType.RANCH_TECH, Palette.DEFAULT);
		model.SelectPalette(PaletteType.OGDEN_HOUSE, Palette.OGDEN);
		model.SelectPalette(PaletteType.OGDEN_TECH, Palette.OGDEN);
		model.SelectPalette(PaletteType.MOCHI_HOUSE, Palette.MOCHI);
		model.SelectPalette(PaletteType.MOCHI_TECH, Palette.MOCHI);
		model.SelectPalette(PaletteType.VAC, Palette.DEFAULT);
		model.SelectPalette(PaletteType.VALLEY, Palette.MOCHI);
		model.SelectPalette(PaletteType.VIKTOR_TECH, Palette.VIKTOR);
		model.SelectPalette(PaletteType.VIKTOR_HOUSE, Palette.VIKTOR);
	}

	public void SetColorsForPalette(PaletteType type, Palette palette)
	{
		PaletteEntry entry = paletteDict[palette];
		Material[] recolorMaterials = GetRecolorMaterials(type);
		foreach (Material mat in recolorMaterials)
		{
			SetColors(mat, entry);
		}
		if (type != PaletteType.VAC)
		{
			return;
		}
		foreach (Material dynamicVacRecolorMat in dynamicVacRecolorMats)
		{
			SetColors(dynamicVacRecolorMat, entry);
		}
	}

	private void SetColors(Material mat, PaletteEntry entry)
	{
		mat.SetColor("_Color00", entry.redDark);
		mat.SetColor("_Color01", entry.redLight);
		mat.SetColor("_Color10", entry.greenDark);
		mat.SetColor("_Color11", entry.greenLight);
		mat.SetColor("_Color20", entry.blueDark);
		mat.SetColor("_Color21", entry.blueLight);
		mat.SetColor("_Color30", entry.blackDark);
		mat.SetColor("_Color31", entry.blackLight);
		mat.SetColor("_Color40", entry.magentaDark);
		mat.SetColor("_Color41", entry.magentaLight);
		mat.SetColor("_Color50", entry.yellowDark);
		mat.SetColor("_Color51", entry.yellowLight);
		mat.SetColor("_Color60", entry.cyanDark);
		mat.SetColor("_Color61", entry.cyanLight);
		mat.SetColor("_Color70", entry.whiteDark);
		mat.SetColor("_Color71", entry.whiteLight);
	}

	private Material[] GetRecolorMaterials(PaletteType type)
	{
		return recolorMats[type];
	}

	private Dictionary<Material, Material> GetZoneDict(ZoneDirector.Zone zone)
	{
		switch (zone)
		{
		case ZoneDirector.Zone.WILDS:
		case ZoneDirector.Zone.OGDEN_RANCH:
			return ogdenMatDict;
		case ZoneDirector.Zone.MOCHI_RANCH:
			return mochiMatDict;
		case ZoneDirector.Zone.VALLEY:
			return valleyMatDict;
		case ZoneDirector.Zone.SLIMULATIONS:
		case ZoneDirector.Zone.VIKTOR_LAB:
			return viktorMatDict;
		default:
			return standardMatDict;
		}
	}

	public Material GetRecolorMaterial(Material mat, ZoneDirector.Zone zone)
	{
		if (GetZoneDict(zone).ContainsKey(mat))
		{
			return GetZoneDict(zone)[mat];
		}
		return null;
	}

	public void RegisterVacRecolorMat(Material mat)
	{
		dynamicVacRecolorMats.Add(mat);
		if (base.enabled && model != null)
		{
			SetColors(mat, paletteDict[model.selectedPalettes[PaletteType.VAC]]);
		}
	}

	public void UnregisterVacRecolorMat(Material mat)
	{
		dynamicVacRecolorMats.Remove(mat);
	}

	public bool IsSelectedPalette(Palette palette, PaletteType paletteType)
	{
		return model.selectedPalettes[paletteType] == palette;
	}

	public bool HasPalette(Palette palette)
	{
		PaletteEntry paletteEntry = paletteDict[palette];
		if (progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER) >= paletteEntry.requiresPartnerLevel)
		{
			if (paletteEntry.requiresProgressCount > 0)
			{
				return progressDir.GetProgress(paletteEntry.requiresProgressType) >= paletteEntry.requiresProgressCount;
			}
			return true;
		}
		return false;
	}

	public List<PaletteEntry> GetOrderedPalettes()
	{
		if (orderedPalettes == null)
		{
			orderedPalettes = paletteDict.Values.ToList();
			orderedPalettes.Sort(Comparer.DEFAULT);
		}
		return orderedPalettes;
	}

	public void NoteSelected(PaletteType type, Palette palette)
	{
		SetColorsForPalette(type, palette);
	}
}
