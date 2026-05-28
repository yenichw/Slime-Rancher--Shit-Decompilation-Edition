using System;
using System.Collections.Generic;
using UnityEngine;

public class ModDirector : MonoBehaviour
{
	public delegate void ModsListener();

	public enum Mod
	{
		VAMPIRIC_CHICKENS = 0,
		NIGHT_TARR_SLIMES = 1,
		RANDOM_PLORTS = 2,
		INCREASED_SLIME_SPAWNS = 3,
		SLIME_HUNGER = 4,
		UNSTABLE_PLORTS = 5
	}

	private ModsListener modsListeners;

	[Tooltip("Chance of a tarr slime spawning in place of a normal slime at night during the mod.")]
	public float nightTarrChance = 0.05f;

	[Tooltip("Chance of a slime producing a random plort instead of its normal one during the mod.")]
	public float randomPlortChance = 0.1f;

	[Tooltip("Factor by which we increase the number of slimes spawned during the mod.")]
	public float increasedSlimeSpawnsFactor = 1.3f;

	[Tooltip("Factor by which slimes' hunger increases during the mod.")]
	public float hungerFactor = 2f;

	[Tooltip("Any mods we should activate immediately on startup, for testing")]
	public Mod[] initMods;

	private List<Mod> activeMods = new List<Mod>();

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		Mod[] array = initMods;
		foreach (Mod item in array)
		{
			activeMods.Add(item);
		}
	}

	public void InitForLevel()
	{
		NotifyModsChanged();
	}

	public void ActivateMod(Mod mod)
	{
		if (!activeMods.Contains(mod))
		{
			activeMods.Add(mod);
			NotifyModsChanged();
		}
	}

	public void DeactivateMod(Mod mod)
	{
		if (activeMods.Contains(mod))
		{
			activeMods.Remove(mod);
			NotifyModsChanged();
		}
	}

	public bool IsModActive(Mod mod)
	{
		return false;
	}

	public void RegisterModsListener(ModsListener listener)
	{
		modsListeners = (ModsListener)Delegate.Combine(modsListeners, listener);
		listener();
	}

	public void UnregisterModsListener(ModsListener listener)
	{
		modsListeners = (ModsListener)Delegate.Remove(modsListeners, listener);
	}

	public float SlimeCountFactor()
	{
		return 1f;
	}

	public float SlimeHungerFactor()
	{
		return 1f;
	}

	public float ChanceOfTarrSpawn()
	{
		return 0f;
	}

	private bool IsNight()
	{
		float num = timeDir.CurrDayFraction();
		if (!(num < 0.25f))
		{
			return num > 0.75f;
		}
		return true;
	}

	public float ChanceRandomPlort()
	{
		return 0f;
	}

	public bool PlortsUnstable()
	{
		return false;
	}

	public float PlortPriceFactor(Identifiable.Id plortId)
	{
		return 1f;
	}

	public bool VampiricChickens()
	{
		return false;
	}

	private void NotifyModsChanged()
	{
		if (modsListeners != null)
		{
			modsListeners();
		}
	}
}
