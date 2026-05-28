using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchRegionHelper : SRSingleton<GlitchRegionHelper>, AmbianceDirector.TimeOfDay, PlayerModel.Participant
{
	private class GlitchTarrNodeGroupComparer : SRComparer<GlitchTarrNode.Group>
	{
	}

	[Tooltip("Renderer to update the material on death in SLIMULATIONS region.")]
	public Renderer seaRenderer;

	private GameObject exitHudInstance;

	public GlitchImpostoDirector[] impostoDirectors { get; private set; }

	public CellDirector[] cellDirectors { get; private set; }

	public GlitchLiquidSource[] stations { get; private set; }

	public GlitchTarrNode[] nodes { get; private set; }

	public GlitchBreadcrumbNetwork breadcrumbs { get; private set; }

	public Dictionary<string, GlitchTeleportDestination> destinationsDict { get; private set; }

	public IEnumerable<GlitchTeleportDestination> destinations => destinationsDict.Values;

	public override void Awake()
	{
		base.Awake();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
		SRSingleton<SceneContext>.Instance.RegionRegistry.ManageWithRegionSet(base.gameObject, RegionRegistry.RegionSetId.SLIMULATIONS);
		ZoneDirector componentInParent = GetComponentInParent<ZoneDirector>();
		impostoDirectors = componentInParent.GetComponentsInChildren<GlitchImpostoDirector>(includeInactive: true);
		cellDirectors = componentInParent.GetComponentsInChildren<CellDirector>(includeInactive: true);
		stations = componentInParent.GetComponentsInChildren<GlitchLiquidSource>(includeInactive: true);
		nodes = componentInParent.GetComponentsInChildren<GlitchTarrNode>(includeInactive: true);
		breadcrumbs = componentInParent.GetRequiredComponentInChildren<GlitchBreadcrumbNetwork>(includeInactive: true);
		destinationsDict = componentInParent.GetComponentsInChildren<GlitchTeleportDestination>(includeInactive: true).ToDictionary((GlitchTeleportDestination d) => d.id, (GlitchTeleportDestination d) => d);
		breadcrumbs.OnGlitchRegionLoaded();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.RegionRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.RegionRegistry.ReleaseFromRegionSet(base.gameObject, RegionRegistry.RegionSetId.SLIMULATIONS);
		}
	}

	public void OnEnable()
	{
		SRSingleton<SceneContext>.Instance.AmbianceDirector.Register(this);
	}

	public void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.AmbianceDirector != null)
		{
			SRSingleton<SceneContext>.Instance.AmbianceDirector.Deregister(this);
		}
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
		if (current == RegionRegistry.RegionSetId.SLIMULATIONS)
		{
			TimeDirector timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
			GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
			PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
			SRSingleton<DynamicObjectContainer>.Instance.DestroyChildren(delegate(GameObject go)
			{
				RegionMember component2 = go.GetComponent<RegionMember>();
				return component2 != null && component2.IsInRegion(RegionRegistry.RegionSetId.SLIMULATIONS);
			}, "GlitchRegionHelper.RegionSetChanged");
			GlitchImpostoDirector[] array = impostoDirectors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ResetImpostos();
			}
			CellDirector[] array2 = cellDirectors;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].ForceCheckSpawn();
			}
			GlitchLiquidSource[] array3 = stations;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].ResetLiquidState();
			}
			List<GlitchTarrNode.Group> list = Enum.GetValues(typeof(GlitchTarrNode.Group)).Cast<GlitchTarrNode.Group>().ToList();
			list.Sort(new GlitchTarrNodeGroupComparer().OrderBy((GlitchTarrNode.Group it) => Randoms.SHARED.GetInt()));
			GlitchTarrNode[] array4 = nodes;
			foreach (GlitchTarrNode glitchTarrNode in array4)
			{
				glitchTarrNode.ResetNode(timeDirector.WorldTime() + (double)((glitch.tarrNodeActivationDelay + (float)(list.IndexOf(glitchTarrNode.activationGroup) + list.Count * glitchTarrNode.activationIndex) * glitch.tarrNodeActivationDelayPerNode) * 3600f));
			}
			foreach (GlitchTeleportDestination destination in destinations)
			{
				destination.Reset(null);
			}
			Randoms.SHARED.Pick(destinations.Where((GlitchTeleportDestination e) => e.isPotentialExitDestination), null).Reset(timeDirector.HoursFromNow(glitch.teleportActivationDelay.GetRandom()));
			playerState.Ammo.Replace(Identifiable.Id.GLITCH_BUG_REPORT, Identifiable.Id.GLITCH_SLIME);
		}
		if (previous != RegionRegistry.RegionSetId.SLIMULATIONS)
		{
			return;
		}
		PlayerState player = SRSingleton<SceneContext>.Instance.PlayerState;
		foreach (GlitchTeleportDestination destination2 in destinations)
		{
			destination2.Reset(0.0);
		}
		SRSingleton<DynamicObjectContainer>.Instance.DestroyChildren(delegate(GameObject go)
		{
			RegionMember component = go.GetComponent<RegionMember>();
			return component != null && component.IsInRegion(RegionRegistry.RegionSetId.SLIMULATIONS);
		}, "GlitchRegionHelper.RegionSetChanged");
		Destroyer.Destroy(exitHudInstance, "GlitchRegionHelper.RegionSetChanged");
		player.Ammo.Replace(Identifiable.Id.GLITCH_SLIME, Identifiable.Id.GLITCH_BUG_REPORT);
		player.Ammo.Clear((int ii) => player.Ammo.GetSlotName(ii) != Identifiable.Id.GLITCH_BUG_REPORT);
	}

	public void OnExitTeleporterBecameActive()
	{
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		Destroyer.Destroy(exitHudInstance, "GlitchRegionHelper.OnExitTeleporterBecameActive");
		exitHudInstance = UnityEngine.Object.Instantiate(glitch.teleportHudPrefab, SRSingleton<HudUI>.Instance.uiContainer.transform);
		exitHudInstance.StartCoroutine(OnExitTeleporterBecameActive_Coroutine(exitHudInstance));
	}

	private static IEnumerator OnExitTeleporterBecameActive_Coroutine(GameObject instance)
	{
		GlitchMetadata glitch = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		yield return new WaitForSeconds(glitch.teleportHudLifetime);
		instance.GetRequiredComponent<Animator>().SetBool("state_active", value: false);
	}

	public float GetCurrentDayFraction_Position()
	{
		return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.ambianceTimeOfDay;
	}

	public float GetCurrentDayFraction_Color()
	{
		return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.ambianceTimeOfDay;
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
	}

	public void TransformChanged(Vector3 position, Quaternion rotation)
	{
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> ammo)
	{
	}

	public void KeyAdded()
	{
	}
}
