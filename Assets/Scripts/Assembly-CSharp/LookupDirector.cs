using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LookupDirector : SRBehaviour
{
	[SerializeField]
	private SlimeAppearanceDirector slimeAppearanceDirector;

	[SerializeField]
	private PrefabList identifiablePrefabs;

	[SerializeField]
	private PrefabList plotPrefabs;

	[SerializeField]
	private PrefabList resourceSpawnerPrefabs;

	[SerializeField]
	private GadgetDefinitionList gadgetDefinitions;

	[SerializeField]
	private VacItemDefinitionList vacItemDefinitions;

	[SerializeField]
	private LiquidDefinitionList liquidDefinitions;

	[SerializeField]
	private UpgradeDefinitionList upgradeDefinitions;

	[SerializeField]
	private PrefabList gordoEntries;

	[SerializeField]
	private ToyDefinitionList toyDefinitions;

	private readonly List<GadgetDefinition> gadgetDefinitionsDynamic = new List<GadgetDefinition>();

	private readonly List<VacItemDefinition> vacItemDefinitionsDynamic = new List<VacItemDefinition>();

	private Dictionary<Identifiable.Id, GameObject> identifiablePrefabDict = new Dictionary<Identifiable.Id, GameObject>(Identifiable.idComparer);

	private Dictionary<LandPlot.Id, GameObject> plotPrefabDict = new Dictionary<LandPlot.Id, GameObject>(LandPlot.idComparer);

	private Dictionary<SpawnResource.Id, GameObject> resourcePrefabDict = new Dictionary<SpawnResource.Id, GameObject>(SpawnResource.idComparer);

	private Dictionary<Gadget.Id, GadgetDefinition> gadgetDefinitionDict = new Dictionary<Gadget.Id, GadgetDefinition>(Gadget.idComparer);

	private Dictionary<Identifiable.Id, VacItemDefinition> vacItemDict = new Dictionary<Identifiable.Id, VacItemDefinition>(Identifiable.idComparer);

	private Dictionary<Identifiable.Id, LiquidDefinition> liquidDict = new Dictionary<Identifiable.Id, LiquidDefinition>(Identifiable.idComparer);

	private Dictionary<PlayerState.Upgrade, UpgradeDefinition> upgradeDefinitionDict = new Dictionary<PlayerState.Upgrade, UpgradeDefinition>(PlayerState.upgradeComparer);

	private Dictionary<Identifiable.Id, GameObject> gordoDict = new Dictionary<Identifiable.Id, GameObject>(Identifiable.idComparer);

	private Dictionary<Identifiable.Id, ToyDefinition> toyDict = new Dictionary<Identifiable.Id, ToyDefinition>(Identifiable.idComparer);

	public IEnumerable<GameObject> PlotPrefabs => plotPrefabs;

	public IEnumerable<GadgetDefinition> GadgetDefinitions => gadgetDefinitions.Concat(gadgetDefinitionsDynamic);

	public IEnumerable<VacItemDefinition> VacItemDefinitions => vacItemDefinitions.Concat(vacItemDefinitionsDynamic);

	public IEnumerable<GameObject> GordoEntries => gordoEntries;

	public void Awake()
	{
		identifiablePrefabDict.Clear();
		foreach (GameObject identifiablePrefab in identifiablePrefabs)
		{
			if (!(identifiablePrefab == null))
			{
				Identifiable component = identifiablePrefab.GetComponent<Identifiable>();
				if (identifiablePrefabDict.ContainsKey(component.id))
				{
					Log.Error("LookupDirector Duplicate Identifiable ID: " + component.id);
				}
				identifiablePrefabDict[component.id] = identifiablePrefab;
			}
		}
		plotPrefabDict.Clear();
		foreach (GameObject plotPrefab in plotPrefabs)
		{
			if (!(plotPrefab == null))
			{
				LandPlot component2 = plotPrefab.GetComponent<LandPlot>();
				if (plotPrefabDict.ContainsKey(component2.typeId))
				{
					Log.Error("LookupDirector Duplicate Plot ID: " + component2.typeId);
				}
				plotPrefabDict[component2.typeId] = plotPrefab;
			}
		}
		resourcePrefabDict.Clear();
		foreach (GameObject resourceSpawnerPrefab in resourceSpawnerPrefabs)
		{
			if (!(resourceSpawnerPrefab == null))
			{
				SpawnResource component3 = resourceSpawnerPrefab.GetComponent<SpawnResource>();
				if (resourcePrefabDict.ContainsKey(component3.id))
				{
					Log.Error("LookupDirector Duplicate Resource ID: " + component3.id);
				}
				resourcePrefabDict[component3.id] = resourceSpawnerPrefab;
			}
		}
		gadgetDefinitionDict.Clear();
		foreach (GadgetDefinition gadgetDefinition in gadgetDefinitions)
		{
			if (!(gadgetDefinition.prefab == null))
			{
				Gadget component4 = gadgetDefinition.prefab.GetComponent<Gadget>();
				if (gadgetDefinitionDict.ContainsKey(component4.id))
				{
					Log.Error("LookupDirector Duplicate Gadget ID: " + component4.id);
				}
				if (gadgetDefinition.id != component4.id)
				{
					Log.Error("LookupDirector Mismatch Gadget.", "entryId", gadgetDefinition.id, "gadgetId", component4.id);
				}
				gadgetDefinitionDict[component4.id] = gadgetDefinition;
			}
		}
		vacItemDict.Clear();
		foreach (VacItemDefinition vacItemDefinition in vacItemDefinitions)
		{
			if (vacItemDict.ContainsKey(vacItemDefinition.Id))
			{
				Log.Error("LookupDirector Duplicate Vac Item Definition ID: " + vacItemDefinition.Id);
			}
			vacItemDict[vacItemDefinition.Id] = vacItemDefinition;
		}
		liquidDict.Clear();
		foreach (LiquidDefinition liquidDefinition in liquidDefinitions)
		{
			if (liquidDict.ContainsKey(liquidDefinition.Id))
			{
				Log.Error("LookupDirector Duplicate Liquid ID: " + liquidDefinition.Id);
			}
			liquidDict[liquidDefinition.Id] = liquidDefinition;
		}
		upgradeDefinitionDict.Clear();
		foreach (UpgradeDefinition upgradeDefinition in upgradeDefinitions)
		{
			if (upgradeDefinitionDict.ContainsKey(upgradeDefinition.Upgrade))
			{
				Log.Error("LookupDirector Duplicate Upgrade ID: " + upgradeDefinition.Upgrade);
			}
			upgradeDefinitionDict[upgradeDefinition.Upgrade] = upgradeDefinition;
		}
		gordoDict.Clear();
		foreach (GameObject gordoEntry in gordoEntries)
		{
			GordoIdentifiable component5 = gordoEntry.GetComponent<GordoIdentifiable>();
			if (gordoDict.ContainsKey(component5.id))
			{
				Log.Error("LookupDirector Duplicate Gordo ID: " + component5.id);
			}
			else
			{
				gordoDict.Add(component5.id, gordoEntry);
			}
		}
		toyDict.Clear();
		foreach (ToyDefinition toyDefinition in toyDefinitions)
		{
			toyDict.Add(toyDefinition.ToyId, toyDefinition);
		}
	}

	public GameObject GetPrefab(Identifiable.Id id)
	{
		if (!identifiablePrefabDict.ContainsKey(id))
		{
			Log.Error(string.Concat("Missing prefab: ", id, " hasIdsCount: ", identifiablePrefabDict.Count));
			return null;
		}
		if (identifiablePrefabDict[id] == null)
		{
			Log.Error("No prefab wired up for identifiable", "id", id);
			return null;
		}
		return identifiablePrefabDict[id];
	}

	public GameObject GetPlotPrefab(LandPlot.Id id)
	{
		return plotPrefabDict[id];
	}

	public GameObject GetResourcePrefab(SpawnResource.Id id)
	{
		try
		{
			return resourcePrefabDict[id];
		}
		catch (KeyNotFoundException innerException)
		{
			throw new KeyNotFoundException($"Failed to find spawn resource entry: {id}", innerException);
		}
	}

	public GadgetDefinition GetGadgetDefinition(Gadget.Id id)
	{
		try
		{
			return gadgetDefinitionDict[id];
		}
		catch (KeyNotFoundException innerException)
		{
			throw new KeyNotFoundException($"Failed to find gadget definition: {id}", innerException);
		}
	}

	public bool HasGadgetDefinition(Gadget.Id id)
	{
		return gadgetDefinitionDict.ContainsKey(id);
	}

	public void RegisterFashion(GameObject prefab, VacItemDefinition vac, GadgetDefinition gadget)
	{
		identifiablePrefabDict[vac.Id] = prefab;
		vacItemDict[vac.Id] = vac;
		vacItemDefinitionsDynamic.RemoveAll((VacItemDefinition e) => e.Id == vac.Id);
		vacItemDefinitionsDynamic.Add(vac);
		gadgetDefinitionDict[gadget.id] = gadget;
		gadgetDefinitionsDynamic.RemoveAll((GadgetDefinition e) => e.id == gadget.id);
		gadgetDefinitionsDynamic.Add(gadget);
	}

	public UpgradeDefinition GetUpgradeDefinition(PlayerState.Upgrade upgrade)
	{
		return upgradeDefinitionDict[upgrade];
	}

	public ToyDefinition GetToyDefinition(Identifiable.Id id)
	{
		return toyDict[id];
	}

	public void RegisterToy(ToyDefinition entry, GameObject prefab)
	{
		identifiablePrefabDict[entry.ToyId] = prefab;
		toyDict[entry.ToyId] = entry;
	}

	public Color GetColor(Identifiable.Id id)
	{
		if (vacItemDict.TryGetValue(id, out var value))
		{
			return value.Color;
		}
		return Color.clear;
	}

	public Sprite GetIcon(Identifiable.Id id)
	{
		try
		{
			if (Identifiable.IsSlime(id))
			{
				return slimeAppearanceDirector.GetCurrentSlimeIcon(id);
			}
			if (id != 0)
			{
				return vacItemDict[id].Icon;
			}
		}
		catch (KeyNotFoundException ex)
		{
			Log.Error("Failed to find Identifiable Id when looking up icon.", "Id", id.ToString(), "Exception", ex);
		}
		return null;
	}

	public GameObject GetLiquidIncomingFX(Identifiable.Id id)
	{
		if (liquidDict.ContainsKey(id))
		{
			return liquidDict[id].InFx;
		}
		return null;
	}

	public GameObject GetLiquidVacFailFX(Identifiable.Id id)
	{
		if (liquidDict.ContainsKey(id))
		{
			return liquidDict[id].VacFailFx;
		}
		return null;
	}

	public GameObject GetGordo(Identifiable.Id id)
	{
		if (gordoDict.ContainsKey(id))
		{
			return gordoDict[id];
		}
		return null;
	}
}
