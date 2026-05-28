using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class LandPlot : SRBehaviour, LandPlotModel.Participant
{
	public enum Id
	{
		NONE = 0,
		EMPTY = 1,
		CORRAL = 2,
		COOP = 3,
		GARDEN = 4,
		SILO = 5,
		POND = 6,
		INCINERATOR = 7
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public bool Equals(Id id1, Id id2)
		{
			return id1 == id2;
		}

		public int GetHashCode(Id obj)
		{
			return (int)obj;
		}
	}

	public enum Upgrade
	{
		NONE = 0,
		WALLS = 1,
		MUSIC_BOX = 2,
		STORAGE2 = 3,
		STORAGE3 = 4,
		STORAGE4 = 5,
		SOIL = 6,
		SPRINKLER = 7,
		SCARESLIME = 8,
		FEEDER = 9,
		VITAMIZER = 10,
		AIR_NET = 11,
		PLORT_COLLECTOR = 12,
		SOLAR_SHIELD = 13,
		ASH_TROUGH = 14,
		MIRACLE_MIX = 15,
		DELUXE_GARDEN = 16,
		DELUXE_COOP = 17
	}

	public class UpgradeComparer : IEqualityComparer<Upgrade>
	{
		public bool Equals(Upgrade a, Upgrade b)
		{
			return a == b;
		}

		public int GetHashCode(Upgrade obj)
		{
			return (int)obj;
		}
	}

	public static IdComparer idComparer = new IdComparer();

	public static UpgradeComparer upgradeComparer = new UpgradeComparer();

	public Id typeId;

	public const float attachScaleUpTime = 5f;

	private LandPlotModel model;

	private GameObject attached;

	private DroneNetwork droneNetwork;

	private AchievementsDirector achievementsDirector;

	public void InitModel(LandPlotModel model)
	{
		model.typeId = typeId;
	}

	public void SetModel(LandPlotModel model)
	{
		this.model = model;
		ApplyUpgrades(model.upgrades);
		if (model.attachedId != 0)
		{
			GameObject toAttach = Object.Instantiate(SRSingleton<GameContext>.Instance.LookupDirector.GetResourcePrefab(model.attachedId), base.transform.position, base.transform.rotation);
			Attach(toAttach, immediate: true, isReplacement: false);
		}
	}

	public void Awake()
	{
		achievementsDirector = SRSingleton<SceneContext>.Instance.AchievementsDirector;
	}

	public void Start()
	{
		droneNetwork = GetComponentInParent<DroneNetwork>();
		droneNetwork.Register(this);
	}

	public void OnDestroy()
	{
		if (droneNetwork != null)
		{
			droneNetwork.Deregister(this);
			droneNetwork = null;
		}
	}

	public void Attach(GameObject toAttach, bool immediate, bool isReplacement, SECTR_AudioCue scaleUpCue = null)
	{
		toAttach.transform.SetParent(base.transform, worldPositionStays: true);
		attached = toAttach;
		SpawnResource component = attached.GetComponent<SpawnResource>();
		model.attachedId = ((!(component == null)) ? component.id : SpawnResource.Id.NONE);
		model.attachedResourceId = component.GetPrimarySpawnId();
		if (!immediate)
		{
			SECTR_AudioSystem.Play(scaleUpCue, toAttach.transform, Vector3.zero, loop: false);
			TweenScaleUpItem(toAttach, isReplacement);
		}
		if (typeId == Id.GARDEN)
		{
			Identifiable.Id attachedCropId = GetAttachedCropId();
			if (Identifiable.IsFruit(attachedCropId))
			{
				achievementsDirector.CheckAchievement(AchievementsDirector.Achievement.FRUIT_TREE_TYPES);
			}
			else if (Identifiable.IsVeggie(attachedCropId))
			{
				achievementsDirector.CheckAchievement(AchievementsDirector.Achievement.VEGGIE_PATCH_TYPES);
			}
		}
	}

	private void TweenScaleUpItem(GameObject toAttach, bool isReplacement)
	{
		SpawnResource[] spawners = toAttach.GetComponentsInChildren<SpawnResource>(includeInactive: true);
		ScaleMarker[] componentsInChildren = toAttach.GetComponentsInChildren<ScaleMarker>();
		foreach (ScaleMarker scaleMarker in componentsInChildren)
		{
			if (!isReplacement || !scaleMarker.doNotScaleAsReplacement)
			{
				SpawnResource[] array = spawners;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].RegisterSpawnBlocker();
				}
				TweenUtil.ScaleIn(scaleMarker.gameObject, 5f).OnComplete(delegate
				{
					TweenScaleUpItem_OnTweenComplete(spawners);
				});
			}
		}
	}

	private void TweenScaleUpItem_OnTweenComplete(SpawnResource[] spawners)
	{
		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].DeregisterSpawnBlocker();
		}
	}

	public bool HasAttached()
	{
		return attached != null;
	}

	public void DestroyAttached()
	{
		Destroyer.Destroy(attached, "LandPlot.DestroyAttached");
		attached = null;
		model.attachedId = SpawnResource.Id.NONE;
	}

	public void AddUpgrade(Upgrade upgrade)
	{
		model.upgrades.Add(upgrade);
		achievementsDirector.CheckAchievement(AchievementsDirector.Achievement.RANCH_UPGRADED_STORAGE);
		achievementsDirector.AddToStat(AchievementsDirector.GameIntStat.UPGRADES_PURCHASED, 1);
		ApplyUpgrades(upgrade.ToEnumerable());
	}

	public bool HasUpgrade(Upgrade upgrade)
	{
		return model.HasUpgrade(upgrade);
	}

	private void ApplyUpgrades(IEnumerable<Upgrade> upgrades)
	{
		PlotUpgrader[] components = GetComponents<PlotUpgrader>();
		foreach (PlotUpgrader plotUpgrader in components)
		{
			foreach (Upgrade upgrade in upgrades)
			{
				plotUpgrader.Apply(upgrade);
			}
		}
		if (droneNetwork != null)
		{
			droneNetwork.OnUpgradesChanged(this);
		}
	}

	public Identifiable.Id GetAttachedCropId()
	{
		if (attached != null)
		{
			return attached.GetComponent<SpawnResource>().GetPrimarySpawnId();
		}
		return Identifiable.Id.NONE;
	}

	public double GetAttachedDeathTime()
	{
		if (attached != null)
		{
			DestroyAfterTime component = attached.GetComponent<DestroyAfterTime>();
			if (component != null)
			{
				return component.GetDeathTime();
			}
		}
		return 0.0;
	}
}
