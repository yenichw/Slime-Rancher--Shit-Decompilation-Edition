using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Slime Appearance Director")]
public class SlimeAppearanceDirector : ScriptableObject, AppearancesModel.Participant
{
	public delegate void OnSlimeAppearanceChangedDelegate(SlimeDefinition slime, SlimeAppearance appearance);

	private class AppearanceDefinitionPair
	{
		public SlimeAppearance appearance;

		public SlimeDefinition definition;
	}

	public SlimeDefinitions SlimeDefinitions;

	[Tooltip("The icon to show for slime appearances that don't have an icon defined.")]
	public Sprite missingIcon;

	[Tooltip("SlimeAppearancePopupUI prefab reference.")]
	public GameObject appearancePopupUI;

	[Tooltip("The default animator controller to use for slimes.")]
	public RuntimeAnimatorController defaultAnimatorController;

	private readonly Dictionary<SlimeAppearance, List<AppearanceDefinitionPair>> appearancesByDependentAppearance = new Dictionary<SlimeAppearance, List<AppearanceDefinitionPair>>(SlimeAppearance.DefaultComparer);

	private AppearanceSelections appearanceSelections = new AppearanceSelections();

	public event OnSlimeAppearanceChangedDelegate onSlimeAppearanceChanged = delegate
	{
	};

	public void OnEnable()
	{
		SlimeDefinition[] slimes = SlimeDefinitions.Slimes;
		foreach (SlimeDefinition slimeDefinition in slimes)
		{
			foreach (SlimeAppearance appearance in slimeDefinition.Appearances)
			{
				RegisterDependentAppearances(slimeDefinition, appearance);
			}
		}
		RefreshDefaultChosenSlimes();
	}

	public void RegisterDependentAppearances(SlimeDefinition definition, SlimeAppearance appearance)
	{
		if (appearance == null)
		{
			Log.Error("Found an unassigned appearance in a slime definition.", "SlimeDefinition", definition.name);
			return;
		}
		SlimeAppearance[] dependentAppearances = appearance.DependentAppearances;
		foreach (SlimeAppearance slimeAppearance in dependentAppearances)
		{
			if (slimeAppearance == null)
			{
				Log.Error("Found an unassigned dependent appearance in a slime appearance.", "SlimeAppearance", appearance);
				continue;
			}
			if (!appearancesByDependentAppearance.TryGetValue(slimeAppearance, out var value))
			{
				value = new List<AppearanceDefinitionPair>();
				appearancesByDependentAppearance.Add(slimeAppearance, value);
			}
			value.Add(new AppearanceDefinitionPair
			{
				appearance = appearance,
				definition = definition
			});
		}
	}

	public void RefreshDefaultChosenSlimes()
	{
		SlimeDefinition[] slimes = SlimeDefinitions.Slimes;
		foreach (SlimeDefinition slimeDefinition in slimes)
		{
			SlimeAppearance slimeAppearance = slimeDefinition.Appearances.First();
			UnlockAppearance(slimeDefinition, slimeAppearance);
			UpdateChosenSlimeAppearance(slimeDefinition, slimeAppearance);
		}
	}

	public SlimeAppearance GetChosenSlimeAppearance(Identifiable.Id id)
	{
		return GetChosenSlimeAppearance(SlimeDefinitions.GetSlimeByIdentifiableId(id));
	}

	public SlimeAppearance GetChosenSlimeAppearance(SlimeDefinition slimeDefinition)
	{
		return appearanceSelections.GetSelectedAppearance(slimeDefinition);
	}

	public void UpdateChosenSlimeAppearance(SlimeDefinition definition, SlimeAppearance newChosenAppearance)
	{
		SetChosenSlimeAppearance(definition, newChosenAppearance);
		if (!appearancesByDependentAppearance.TryGetValue(newChosenAppearance, out var value))
		{
			return;
		}
		foreach (AppearanceDefinitionPair item in value)
		{
			if (AreDependentAppearancesChosen(item.appearance))
			{
				SetChosenSlimeAppearance(item.definition, item.appearance);
			}
		}
	}

	public Sprite GetCurrentSlimeIcon(Identifiable.Id slimeId)
	{
		Sprite icon = GetChosenSlimeAppearance(slimeId).Icon;
		if (!(icon != null))
		{
			return missingIcon;
		}
		return icon;
	}

	private bool AreDependentAppearancesChosen(SlimeAppearance appearance)
	{
		return appearance.DependentAppearances.All((SlimeAppearance a) => appearanceSelections.GetAllSelectedAppearances().Contains(a));
	}

	private void SetChosenSlimeAppearance(SlimeDefinition slimeDefinition, SlimeAppearance newAppearance)
	{
		appearanceSelections.SelectAppearanceForSlime(slimeDefinition, newAppearance);
		this.onSlimeAppearanceChanged(slimeDefinition, newAppearance);
	}

	public void UnlockAppearance(SlimeDefinition slimeDefinition, SlimeAppearance appearance)
	{
		appearanceSelections.UnlockAppearanceForSlime(slimeDefinition, appearance);
	}

	public void LockAppearance(SlimeDefinition slimeDefinition, SlimeAppearance appearance)
	{
		appearanceSelections.LockAppearanceForSlime(slimeDefinition, appearance);
	}

	public bool IsAppearanceUnlocked(SlimeDefinition slimeDefinition, SlimeAppearance appearance)
	{
		return appearanceSelections.GetUnlockedAppearances(slimeDefinition).Contains(appearance);
	}

	public IEnumerable<SlimeAppearance> GetUnlockedAppearances(SlimeDefinition slimeDefinition)
	{
		return appearanceSelections.GetUnlockedAppearances(slimeDefinition);
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterAppearances(this);
		RefreshDefaultChosenSlimes();
	}

	public void InitModel(AppearancesModel model)
	{
		foreach (SlimeDefinition item in SlimeDefinitions.Slimes.Where((SlimeDefinition slime) => !slime.IsLargo))
		{
			SlimeAppearance appearanceForSet = item.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC);
			if (appearanceForSet == null)
			{
				throw new Exception("No classic appearance available for slime " + item.Name);
			}
			model.AppearanceSelections.UnlockAppearanceForSlime(item, appearanceForSet);
			model.AppearanceSelections.SelectAppearanceForSlime(item, appearanceForSet);
		}
	}

	public void SetModel(AppearancesModel model)
	{
		appearanceSelections = model.AppearanceSelections;
		foreach (Identifiable.Id item in model.unlocks.Keys.ToList())
		{
			SlimeDefinition slime = SlimeDefinitions.GetSlimeByIdentifiableId(item);
			foreach (SlimeAppearance item2 in model.unlocks[item].Select((SlimeAppearance.AppearanceSaveSet saveSet) => slime.GetAppearanceForSet(saveSet)).ToList())
			{
				UnlockAppearance(slime, item2);
			}
		}
		foreach (Identifiable.Id item3 in model.selections.Keys.ToList())
		{
			SlimeDefinition slimeByIdentifiableId = SlimeDefinitions.GetSlimeByIdentifiableId(item3);
			UpdateChosenSlimeAppearance(slimeByIdentifiableId, slimeByIdentifiableId.GetAppearanceForSet(model.selections[item3]));
		}
	}
}
