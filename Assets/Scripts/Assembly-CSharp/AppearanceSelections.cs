using System.Collections.Generic;
using System.Linq;

public class AppearanceSelections
{
	public delegate void OnAppearanceUnlockedForSlimeDelegate(SlimeDefinition slime, SlimeAppearance appearance);

	public delegate void OnAppearanceSelectedForSlimeDelegate(SlimeDefinition slime, SlimeAppearance appearance);

	public delegate void OnAppearanceLockedForSlimeDelegate(SlimeDefinition slime, SlimeAppearance appearance);

	private readonly Dictionary<Identifiable.Id, HashSet<SlimeAppearance>> unlocks = new Dictionary<Identifiable.Id, HashSet<SlimeAppearance>>();

	private readonly Dictionary<Identifiable.Id, SlimeAppearance> selections = new Dictionary<Identifiable.Id, SlimeAppearance>();

	private readonly HashSet<SlimeAppearance> allSelectedAppearances = new HashSet<SlimeAppearance>(SlimeAppearanceEqualityComparer.Default);

	public event OnAppearanceUnlockedForSlimeDelegate onAppearanceUnlocked = delegate
	{
	};

	public event OnAppearanceSelectedForSlimeDelegate onAppearanceSelected = delegate
	{
	};

	public event OnAppearanceLockedForSlimeDelegate onAppearanceLocked = delegate
	{
	};

	public void UnlockAppearanceForSlime(SlimeDefinition slime, SlimeAppearance appearance)
	{
		if (!slime.Appearances.Contains(appearance))
		{
			Log.Error($"Trying to unlock appearance {appearance.name} not attached to a slime definition {slime.Name}.");
			return;
		}
		GetOrCreateUnlockSetForSlime(slime).Add(appearance);
		this.onAppearanceUnlocked(slime, appearance);
	}

	public void LockAppearanceForSlime(SlimeDefinition slime, SlimeAppearance appearance)
	{
		if (unlocks.TryGetValue(slime.IdentifiableId, out var value))
		{
			value.Remove(appearance);
		}
		this.onAppearanceLocked(slime, appearance);
	}

	public void SelectAppearanceForSlime(SlimeDefinition slime, SlimeAppearance appearance)
	{
		SlimeAppearance selectedAppearance = GetSelectedAppearance(slime);
		if (selectedAppearance != null)
		{
			allSelectedAppearances.Remove(selectedAppearance);
		}
		allSelectedAppearances.Add(appearance);
		selections[slime.IdentifiableId] = appearance;
		this.onAppearanceSelected(slime, appearance);
	}

	public SlimeAppearance GetSelectedAppearance(SlimeDefinition slime)
	{
		if (!selections.TryGetValue(slime.IdentifiableId, out var value))
		{
			return null;
		}
		return value;
	}

	public List<SlimeAppearance> GetUnlockedAppearances(SlimeDefinition slime)
	{
		return GetOrCreateUnlockSetForSlime(slime).ToList();
	}

	public HashSet<SlimeAppearance> GetAllSelectedAppearances()
	{
		return allSelectedAppearances;
	}

	private HashSet<SlimeAppearance> GetOrCreateUnlockSetForSlime(SlimeDefinition slime)
	{
		if (!unlocks.TryGetValue(slime.IdentifiableId, out var value))
		{
			value = new HashSet<SlimeAppearance>();
			unlocks[slime.IdentifiableId] = value;
		}
		return value;
	}
}
