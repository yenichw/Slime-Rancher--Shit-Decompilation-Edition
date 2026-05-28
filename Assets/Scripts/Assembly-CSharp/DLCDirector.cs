using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DLCPackage;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

public class DLCDirector
{
	public delegate void OnPackageInstalledDelegate(Id package);

	private class PackageLoader
	{
		public readonly DLCPackageMetadata package;

		public readonly DLCContentMetadata[] content;

		public PackageLoader(Id id)
		{
			string path = Path.Combine("DLC", id.ToString().ToLowerInvariant());
			package = Resources.Load<DLCPackageMetadata>(Path.Combine(path, "package"));
			content = Resources.LoadAll<DLCContentMetadata>(Path.Combine(path, "package_metadata"));
			if (package == null)
			{
				throw new Exception($"Failed to load DLC package. [id={id}]");
			}
			if (content == null)
			{
				throw new Exception($"Failed to load DLC package contents. [id={id}]");
			}
		}
	}

	public static HashSet<string> SECRET_STYLE_TREASURE_PODS = new HashSet<string>
	{
		"pod1067506426", "pod0573382639", "pod0528457170", "pod0209361044", "pod2089428629", "pod0797820130", "pod0793461898", "pod1736587205", "pod1843001748", "pod1327729579",
		"pod1761631840", "pod0942230423", "pod1507800227", "pod0403498756", "pod0732526653", "pod1897070320", "pod1003003618", "pod0084486208", "pod0463402699", "pod1284546475"
	};

	private DLCProvider provider;

	private Dictionary<Id, PackageLoader> packageLoadersDict = new Dictionary<Id, PackageLoader>(IdComparer.Instance);

	public IEnumerable<Id> Installed => from package in GetSupportedPackages()
		where GetPackageState(package) >= State.INSTALLED
		select package;

	public event OnPackageInstalledDelegate onPackageInstalled = delegate
	{
	};

	public bool SetProvider(DLCProvider provider)
	{
		if (this.provider != null)
		{
			Log.Error("Attempting to replace existing DLC provider.");
			return false;
		}
		this.provider = provider;
		return true;
	}

	public IEnumerable<Id> GetSupportedPackages()
	{
		if (provider == null)
		{
			yield break;
		}
		foreach (Id item in provider.GetSupported())
		{
			yield return item;
		}
	}

	public bool HasReached(Id id, State state)
	{
		if (provider != null && provider.GetSupported().Contains(id))
		{
			return provider.GetState(id) >= state;
		}
		return false;
	}

	public State GetPackageState(Id id)
	{
		if (provider == null)
		{
			return State.UNDEFINED;
		}
		return provider.GetState(id);
	}

	public void ShowPackageInStore(Id id)
	{
		if (provider != null)
		{
			provider.ShowInStore(id);
		}
	}

	public bool IsPackageInstalledAndEnabled(Id id)
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().enableDLC)
		{
			return HasReached(id, State.INSTALLED);
		}
		return false;
	}

	public void InitForLevel()
	{
		if (provider != null && !Levels.isSpecial())
		{
			RegisterPackages();
		}
	}

	public IEnumerator RefreshPackagesAsync()
	{
		if (provider != null)
		{
			yield return provider.Refresh();
		}
	}

	public IEnumerator RegisterPackagesAsync()
	{
		yield return RefreshPackagesAsync();
		RegisterPackages();
	}

	public void RegisterPackages()
	{
		foreach (Id item in Installed)
		{
			DLCContentMetadata[] content = GetPackageLoader(item).content;
			for (int i = 0; i < content.Length; i++)
			{
				content[i].Register();
			}
			this.onPackageInstalled(item);
		}
	}

	public IEnumerable<DLCPackageMetadata> LoadPackageMetadatas()
	{
		return from id in GetSupportedPackages()
			select GetPackageLoader(id).package;
	}

	private PackageLoader GetPackageLoader(Id id)
	{
		if (packageLoadersDict.ContainsKey(id))
		{
			return packageLoadersDict[id];
		}
		return packageLoadersDict[id] = new PackageLoader(id);
	}

	public void Purge(GameV12 game)
	{
		Id[] array = (from Id p in Enum.GetValues(typeof(Id))
			where GetPackageState(p) < State.INSTALLED && PurgePackage(game, p)
			select p).ToArray();
		if (array.Any())
		{
			throw new DLCPurgedException(array);
		}
	}

	private bool PurgePackage(GameV12 game, Id package)
	{
		int num = 0;
		switch (package)
		{
		case Id.PLAYSET_PIRATE:
			num += PurgeChromaPack(game, RanchDirector.Palette.PALETTE27);
			num += PurgeFashion(game, Gadget.Id.FASHION_POD_PIRATEY, Identifiable.Id.PIRATEY_FASHION);
			num += PurgeToy(game, Identifiable.Id.TREASURE_CHEST_TOY);
			break;
		case Id.PLAYSET_HEROIC:
			num += PurgeChromaPack(game, RanchDirector.Palette.PALETTE28);
			num += PurgeFashion(game, Gadget.Id.FASHION_POD_HEROIC, Identifiable.Id.HEROIC_FASHION);
			num += PurgeToy(game, Identifiable.Id.BOP_GOBLIN_TOY);
			break;
		case Id.PLAYSET_SCIFI:
			num += PurgeChromaPack(game, RanchDirector.Palette.PALETTE29);
			num += PurgeFashion(game, Gadget.Id.FASHION_POD_SCIFI, Identifiable.Id.SCIFI_FASHION);
			num += PurgeToy(game, Identifiable.Id.ROBOT_TOY);
			break;
		case Id.SECRET_STYLE:
			foreach (KeyValuePair<Identifiable.Id, List<SlimeAppearance.AppearanceSaveSet>> unlock in game.appearances.unlocks)
			{
				num += unlock.Value.RemoveAll((SlimeAppearance.AppearanceSaveSet it) => it == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE);
				game.appearances.selections[unlock.Key] = SlimeAppearance.AppearanceSaveSet.CLASSIC;
			}
			foreach (KeyValuePair<string, TreasurePodV01> treasurePod in game.world.treasurePods)
			{
				if (SECRET_STYLE_TREASURE_PODS.Contains(treasurePod.Key) && treasurePod.Value.state != 0)
				{
					treasurePod.Value.state = TreasurePod.State.LOCKED;
					num++;
				}
			}
			break;
		default:
			throw new InvalidOperationException();
		}
		return num > 0;
	}

	private int PurgeChromaPack(GameV12 game, RanchDirector.Palette palette)
	{
		int num = 0;
		foreach (RanchDirector.PaletteType item in game.ranch.palettes.Keys.ToList())
		{
			if (game.ranch.palettes[item] == palette)
			{
				game.ranch.palettes[item] = RanchDirector.Palette.DEFAULT;
				num++;
			}
		}
		return num;
	}

	private int PurgeFashion(GameV12 game, Gadget.Id gadget, Identifiable.Id fashion)
	{
		int num = 0;
		foreach (GordoV01 value in game.world.gordos.Values)
		{
			num += value.fashions.RemoveAll((Identifiable.Id it) => it == fashion);
		}
		foreach (ActorDataV09 actor in game.actors)
		{
			num += actor.fashions.RemoveAll((Identifiable.Id it) => it == fashion);
		}
		foreach (string item in game.world.placedGadgets.Keys.ToList())
		{
			if (game.world.placedGadgets[item].gadgetId == gadget)
			{
				num += Convert.ToInt32(game.world.placedGadgets.Remove(item));
			}
		}
		foreach (PlacedGadgetV08 value2 in game.world.placedGadgets.Values)
		{
			num += value2.fashions.RemoveAll((Identifiable.Id it) => it == fashion);
			if (value2.drone != null)
			{
				num += value2.drone.drone.fashions.RemoveAll((Identifiable.Id it) => it == fashion);
			}
		}
		num += game.actors.RemoveAll((ActorDataV09 it) => it.typeId == (int)fashion);
		num += game.player.ammo[PlayerState.AmmoMode.DEFAULT].RemoveAll((AmmoDataV02 d) => d.id == fashion);
		num += game.player.blueprints.RemoveAll((Gadget.Id it) => it == gadget);
		num += game.player.availBlueprints.RemoveAll((Gadget.Id it) => it == gadget);
		num += Convert.ToInt32(game.player.blueprintLocks.Remove(gadget));
		return num + Convert.ToInt32(game.player.gadgets.Remove(gadget));
	}

	private int PurgeToy(GameV12 game, Identifiable.Id toy)
	{
		return game.actors.RemoveAll((ActorDataV09 it) => it.typeId == (int)toy);
	}
}
