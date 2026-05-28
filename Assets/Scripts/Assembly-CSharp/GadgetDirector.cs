using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GadgetDirector : MonoBehaviour, GadgetsModel.Participant
{
	public delegate bool UnlockCondition();

	public class BlueprintLockData
	{
		public bool timedLock;

		public double lockedUntil;

		public BlueprintLockData(bool timedLock, double lockedUntil)
		{
			this.timedLock = timedLock;
			this.lockedUntil = lockedUntil;
		}

		public BlueprintLockData()
		{
		}
	}

	public class BlueprintLocker
	{
		public class ViktorProgress : BlueprintLocker
		{
			public ViktorProgress(GadgetDirector director, Gadget.Id id, int count)
				: base(director, id, () => SRSingleton<SceneContext>.Instance.ProgressDirector.GetProgress(ProgressDirector.ProgressType.VIKTOR_REWARDS) >= count, 0f)
			{
			}

			public override void Unlock()
			{
				base.Unlock();
				gadgetDir.AddBlueprint(id);
			}

			public override bool ShowBlueprintAvailablePopup()
			{
				return false;
			}
		}

		protected readonly GadgetDirector gadgetDir;

		protected readonly UnlockCondition unlockCondition;

		protected readonly float unlockDelayHrs;

		protected readonly Gadget.Id id;

		public BlueprintLocker(GadgetDirector gadgetDir, Gadget.Id id, UnlockCondition unlockCondition, float unlockDelayHrs)
		{
			this.gadgetDir = gadgetDir;
			this.id = id;
			this.unlockCondition = unlockCondition;
			this.unlockDelayHrs = unlockDelayHrs;
		}

		public bool CheckUnlockCondition()
		{
			if (!gadgetDir.model.IsTimedLock(id))
			{
				return unlockCondition();
			}
			return false;
		}

		public bool ReachedUnlockTime()
		{
			if (gadgetDir.model.IsTimedLock(id))
			{
				return gadgetDir.timeDir.HasReached(gadgetDir.model.GetLockedUntil(id));
			}
			return false;
		}

		public virtual void Unlock()
		{
			gadgetDir.model.UnlockAt(id, gadgetDir.timeDir.HoursFromNow(unlockDelayHrs));
		}

		public virtual bool ShowBlueprintAvailablePopup()
		{
			return true;
		}
	}

	private class AvailBlueprintPopupCreator : PopupDirector.PopupCreator
	{
		private Gadget.Id id;

		public AvailBlueprintPopupCreator(Gadget.Id id)
		{
			this.id = id;
		}

		public override void Create()
		{
			AvailBlueprintPopupUI.CreateAvailBlueprintPopup(SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(id));
		}

		public override bool Equals(object other)
		{
			if (other is AvailBlueprintPopupCreator)
			{
				return ((AvailBlueprintPopupCreator)other).id == id;
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

	public class PlacementError
	{
		public string message;

		public string button;
	}

	public GameObject gadgetPopupPrefab;

	public GameObject availBlueprintPopupPrefab;

	public GameObject waitForChargeupPrefab;

	public const int REFINERY_MAX = 999;

	private TimeDirector timeDir;

	private ProgressDirector progressDir;

	private PopupDirector popupDir;

	private TutorialDirector tutorialDir;

	private GadgetsModel model;

	private List<Gadget.Id> toRemove = new List<Gadget.Id>();

	public Dictionary<Gadget.Id, BlueprintLocker> blueprintLocks = new Dictionary<Gadget.Id, BlueprintLocker>(Gadget.idComparer);

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		popupDir = SRSingleton<SceneContext>.Instance.PopupDirector;
		tutorialDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGadgets(this);
	}

	public void InitModel(GadgetsModel model)
	{
		blueprintLocks.Clear();
		model.Reset();
		InitBlueprintLocks();
		foreach (Gadget.Id key in blueprintLocks.Keys)
		{
			model.blueprintLockData[key] = new BlueprintLockData(timedLock: false, double.PositiveInfinity);
		}
	}

	public void SetModel(GadgetsModel model)
	{
		this.model = model;
		ClearUnnecessaryBlueprintLockers();
		CheckAllBlueprintLockers();
	}

	private void ClearUnnecessaryBlueprintLockers()
	{
		foreach (Gadget.Id availBlueprint in model.availBlueprints)
		{
			model.blueprintLockData.Remove(availBlueprint);
			blueprintLocks.Remove(availBlueprint);
		}
	}

	private void InitBlueprintLocks()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().blueprintsEnabled)
		{
			blueprintLocks[Gadget.Id.EXTRACTOR_DRILL_ADVANCED] = CreateBasicLock(Gadget.Id.EXTRACTOR_DRILL_ADVANCED, Gadget.Id.NONE, ProgressDirector.ProgressType.UNLOCK_LAB, 126f);
			blueprintLocks[Gadget.Id.EXTRACTOR_PUMP_ADVANCED] = CreateBasicLock(Gadget.Id.EXTRACTOR_PUMP_ADVANCED, Gadget.Id.EXTRACTOR_PUMP_NOVICE, 120);
			blueprintLocks[Gadget.Id.EXTRACTOR_APIARY_ADVANCED] = CreateBasicLock(Gadget.Id.EXTRACTOR_APIARY_ADVANCED, Gadget.Id.EXTRACTOR_APIARY_NOVICE, 120);
			blueprintLocks[Gadget.Id.TAMING_BELL] = CreateBasicLock(Gadget.Id.TAMING_BELL, new Gadget.Id[1] { Gadget.Id.MED_STATION }, new ProgressDirector.ProgressType[2]
			{
				ProgressDirector.ProgressType.UNLOCK_MOSS,
				ProgressDirector.ProgressType.UNLOCK_QUARRY
			}, 72f);
			blueprintLocks[Gadget.Id.REFINERY_LINK] = CreateBasicLock(Gadget.Id.REFINERY_LINK, new Gadget.Id[3]
			{
				Gadget.Id.EXTRACTOR_DRILL_ADVANCED,
				Gadget.Id.EXTRACTOR_PUMP_ADVANCED,
				Gadget.Id.EXTRACTOR_APIARY_ADVANCED
			}, null, 24f);
			blueprintLocks[Gadget.Id.TELEPORTER_BLUE] = CreateBasicLock(Gadget.Id.TELEPORTER_BLUE, Gadget.Id.TELEPORTER_PINK, ProgressDirector.ProgressType.UNLOCK_QUARRY, 48f);
			blueprintLocks[Gadget.Id.TELEPORTER_GREY] = CreateBasicLock(Gadget.Id.TELEPORTER_GREY, Gadget.Id.TELEPORTER_PINK, ProgressDirector.ProgressType.UNLOCK_MOSS, 48f);
			blueprintLocks[Gadget.Id.TELEPORTER_BUTTERSCOTCH] = CreateBasicLock(Gadget.Id.TELEPORTER_BUTTERSCOTCH, Gadget.Id.TELEPORTER_PINK, ProgressDirector.ProgressType.UNLOCK_DESERT, 0.5f);
			blueprintLocks[Gadget.Id.WARP_DEPOT_BLUE] = CreateBasicLock(Gadget.Id.WARP_DEPOT_BLUE, Gadget.Id.WARP_DEPOT_PINK, ProgressDirector.ProgressType.UNLOCK_QUARRY, 48f);
			blueprintLocks[Gadget.Id.WARP_DEPOT_GREY] = CreateBasicLock(Gadget.Id.WARP_DEPOT_GREY, Gadget.Id.WARP_DEPOT_PINK, ProgressDirector.ProgressType.UNLOCK_MOSS, 48f);
			blueprintLocks[Gadget.Id.WARP_DEPOT_BUTTERSCOTCH] = CreateBasicLock(Gadget.Id.WARP_DEPOT_BUTTERSCOTCH, Gadget.Id.WARP_DEPOT_PINK, ProgressDirector.ProgressType.UNLOCK_DESERT, 0.5f);
			blueprintLocks[Gadget.Id.LAMP_BLUE] = CreateBasicLock(Gadget.Id.LAMP_BLUE, Gadget.Id.LAMP_PINK, 48);
			blueprintLocks[Gadget.Id.LAMP_GREY] = CreateBasicLock(Gadget.Id.LAMP_GREY, Gadget.Id.LAMP_BLUE, 48);
			blueprintLocks[Gadget.Id.FASHION_POD_CLIP_ON] = CreateBasicLock(Gadget.Id.FASHION_POD_CLIP_ON, Gadget.Id.SLIME_STAGE, 48);
			blueprintLocks[Gadget.Id.FASHION_POD_GOOGLY] = CreateBasicLock(Gadget.Id.FASHION_POD_GOOGLY, Gadget.Id.SLIME_HOOP, 72);
			blueprintLocks[Gadget.Id.FASHION_POD_REMOVER] = CreateAnyFashionLock(Gadget.Id.FASHION_POD_REMOVER, 24f);
			blueprintLocks[Gadget.Id.CHICKEN_CLONER] = new BlueprintLocker.ViktorProgress(this, Gadget.Id.CHICKEN_CLONER, 1);
			blueprintLocks[Gadget.Id.DRONE_ADVANCED] = new BlueprintLocker.ViktorProgress(this, Gadget.Id.DRONE_ADVANCED, 2);
		}
	}

	public BlueprintLocker CreateBasicLock(Gadget.Id id, Gadget.Id waitForBlueprint, int delayHrs)
	{
		return CreateBasicLock(id, (waitForBlueprint == Gadget.Id.NONE) ? null : new Gadget.Id[1] { waitForBlueprint }, null, delayHrs);
	}

	public BlueprintLocker CreateBasicLock(Gadget.Id id, Gadget.Id waitForBlueprint, ProgressDirector.ProgressType waitForProgress, float delayHrs)
	{
		return CreateBasicLock(id, (waitForBlueprint == Gadget.Id.NONE) ? null : new Gadget.Id[1] { waitForBlueprint }, new ProgressDirector.ProgressType[1] { waitForProgress }, delayHrs);
	}

	public BlueprintLocker CreateBasicLock(Gadget.Id id, Gadget.Id[] waitForBlueprints, ProgressDirector.ProgressType[] waitForProgress, float delayHrs)
	{
		return new BlueprintLocker(this, id, () => ShouldUnlock(waitForBlueprints, waitForProgress), delayHrs);
	}

	public BlueprintLocker CreateAnyFashionLock(Gadget.Id id, float delayHrs)
	{
		return new BlueprintLocker(this, id, () => HasAnyFashionGadget(), delayHrs);
	}

	private bool HasAnyFashionGadget()
	{
		foreach (Gadget.Id aLL_FASHION in Gadget.ALL_FASHIONS)
		{
			if (HasBlueprint(aLL_FASHION))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldUnlock(Gadget.Id[] waitForBlueprints, ProgressDirector.ProgressType[] waitForProgress)
	{
		if (waitForBlueprints != null)
		{
			foreach (Gadget.Id blueprint in waitForBlueprints)
			{
				if (!HasBlueprint(blueprint))
				{
					return false;
				}
			}
		}
		if (waitForProgress != null)
		{
			foreach (ProgressDirector.ProgressType type in waitForProgress)
			{
				if (!progressDir.HasProgress(type))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void Update()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().blueprintsEnabled)
		{
			foreach (KeyValuePair<Gadget.Id, BlueprintLocker> blueprintLock in blueprintLocks)
			{
				if (!blueprintLock.Value.ReachedUnlockTime())
				{
					continue;
				}
				toRemove.Add(blueprintLock.Key);
				if (!model.availBlueprints.Contains(blueprintLock.Key))
				{
					model.availBlueprints.Add(blueprintLock.Key);
					if (blueprintLock.Value.ShowBlueprintAvailablePopup())
					{
						popupDir.QueueForPopup(new AvailBlueprintPopupCreator(blueprintLock.Key));
						popupDir.MaybePopupNext();
					}
				}
			}
			foreach (Gadget.Id item in toRemove)
			{
				blueprintLocks.Remove(item);
				model.blueprintLockData.Remove(item);
			}
			toRemove.Clear();
		}
		if (timeDir.OnPassedHour(5f) && SRSingleton<LockOnDeath>.Instance != null && SRSingleton<LockOnDeath>.Instance.Locked())
		{
			CheckGordoSnares();
		}
	}

	public void CheckGordoSnares()
	{
		foreach (GordoSnare allGordoSnare in GordoSnare.AllGordoSnares)
		{
			if (allGordoSnare.IsBaited())
			{
				allGordoSnare.SnareGordo();
			}
		}
	}

	public void OnProgress(ProgressDirector.ProgressType type)
	{
		CheckAllBlueprintLockers();
	}

	public bool AddBlueprint(Gadget.Id blueprint)
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().blueprintsEnabled && !model.blueprints.Contains(blueprint))
		{
			model.blueprints.Add(blueprint);
			CheckAllBlueprintLockers();
			return true;
		}
		return false;
	}

	public void CheckAllBlueprintLockers()
	{
		foreach (KeyValuePair<Gadget.Id, BlueprintLocker> blueprintLock in blueprintLocks)
		{
			if (blueprintLock.Value.CheckUnlockCondition())
			{
				blueprintLock.Value.Unlock();
			}
		}
	}

	public bool IsBlueprintUnlocked(Gadget.Id blueprint)
	{
		if (HasBlueprint(blueprint))
		{
			return false;
		}
		if (!model.availBlueprints.Contains(blueprint))
		{
			return model.registeredBlueprints.Contains(blueprint);
		}
		return true;
	}

	public bool HasBlueprint(Gadget.Id blueprint)
	{
		return model.blueprints.Contains(blueprint);
	}

	public bool CanAddGadget(GadgetDefinition gadgetDefinition)
	{
		int buyCountLimit = gadgetDefinition.buyCountLimit;
		List<Gadget.Id> list = new List<Gadget.Id> { gadgetDefinition.id };
		if (buyCountLimit > 0)
		{
			return GetGadgetCount(list) + GetPlacedCount(list) < buyCountLimit;
		}
		return true;
	}

	public bool CanPlaceGadget(GadgetSite site, Gadget.Id gadget)
	{
		return GetPlacementError(site, gadget) == null;
	}

	public PlacementError GetPlacementError(GadgetSite site, Gadget.Id gadget)
	{
		GadgetDefinition gadgetDefinition = SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(gadget);
		if (gadget == Gadget.Id.DRONE || gadget == Gadget.Id.DRONE_ADVANCED)
		{
			DroneNetwork droneNetwork = DroneNetwork.Find(site.gameObject);
			if (droneNetwork == null)
			{
				return new PlacementError
				{
					message = "w.not_on_ranch_drone",
					button = "b.place"
				};
			}
			if (droneNetwork.Drones.Count() >= gadgetDefinition.countLimit)
			{
				return new PlacementError
				{
					message = "w.limit_reached_drone",
					button = "b.limit_reached"
				};
			}
			return null;
		}
		if (gadgetDefinition.countLimit > 0)
		{
			List<Gadget.Id> gadgetsToCountIds = gadgetDefinition.GetGadgetsToCountIds();
			if (GetPlacedCount(gadgetsToCountIds) >= gadgetDefinition.countLimit)
			{
				string text = gadgetDefinition.id.ToString();
				if (text.StartsWith("EXTRACTOR_DRILL_"))
				{
					return new PlacementError
					{
						message = "w.limit_reached_drill",
						button = "b.limit_reached"
					};
				}
				if (text.StartsWith("EXTRACTOR_PUMP_"))
				{
					return new PlacementError
					{
						message = "w.limit_reached_pump",
						button = "b.limit_reached"
					};
				}
				if (text.StartsWith("EXTRACTOR_APIARY_"))
				{
					return new PlacementError
					{
						message = "w.limit_reached_apiary",
						button = "b.limit_reached"
					};
				}
				return new PlacementError
				{
					message = "w.limit_reached_drill",
					button = "b.limit_reached"
				};
			}
		}
		return null;
	}

	public void AddGadget(Gadget.Id gadget)
	{
		if (model.gadgets.ContainsKey(gadget))
		{
			Dictionary<Gadget.Id, int> gadgets = model.gadgets;
			int value = gadgets[gadget] + 1;
			gadgets[gadget] = value;
		}
		else
		{
			model.gadgets[gadget] = 1;
		}
	}

	public void SpendGadget(Gadget.Id gadget)
	{
		if (model.gadgets.ContainsKey(gadget))
		{
			if (model.gadgets[gadget] > 1)
			{
				Dictionary<Gadget.Id, int> gadgets = model.gadgets;
				int value = gadgets[gadget] - 1;
				gadgets[gadget] = value;
			}
			else
			{
				model.gadgets.Remove(gadget);
			}
		}
	}

	public bool RemoveGadget(Gadget.Id gadget)
	{
		if (model.gadgets.ContainsKey(gadget) && model.gadgets[gadget] >= 1)
		{
			Dictionary<Gadget.Id, int> gadgets = model.gadgets;
			int value = gadgets[gadget] - 1;
			gadgets[gadget] = value;
			if (model.gadgets[gadget] == 0)
			{
				model.gadgets.Remove(gadget);
			}
			return true;
		}
		return false;
	}

	public int GetGadgetCount(List<Gadget.Id> gadgets)
	{
		int num = 0;
		foreach (Gadget.Id gadget in gadgets)
		{
			num += GetGadgetCount(gadget);
		}
		return num;
	}

	public int GetGadgetCount(Gadget.Id gadget)
	{
		if (model.gadgets.ContainsKey(gadget))
		{
			return model.gadgets[gadget];
		}
		return 0;
	}

	public static bool IsRefineryResource(Identifiable.Id id)
	{
		if (!Identifiable.PLORT_CLASS.Contains(id))
		{
			return Identifiable.CRAFT_CLASS.Contains(id);
		}
		return true;
	}

	public int GetRefinerySpaceAvailable(Identifiable.Id id)
	{
		if (!IsRefineryResource(id))
		{
			return 0;
		}
		return Mathf.Max(0, 999 - GetRefineryCount(id));
	}

	public bool HasRefinerySpaceAvailable(Identifiable.Id id, int count = 1)
	{
		return GetRefinerySpaceAvailable(id) >= count;
	}

	public bool AddToRefinery(Identifiable.Id id)
	{
		return AddToRefinery(id, 1) > 0;
	}

	public int AddToRefinery(Identifiable.Id id, int count)
	{
		return AddToRefinery(id, count, overflow: false);
	}

	public int AddToRefinery(Identifiable.Id id, int count, bool overflow)
	{
		int num = (IsRefineryResource(id) ? (overflow ? count : Mathf.Min(count, GetRefinerySpaceAvailable(id))) : 0);
		if (num > 0)
		{
			model.craftMatCounts[id] = GetRefineryCount(id) + num;
			tutorialDir.OnRefineryAdded();
		}
		return num;
	}

	public bool HasInRefinery(GadgetDefinition.CraftCost[] costs)
	{
		foreach (GadgetDefinition.CraftCost craftCost in costs)
		{
			if (model.craftMatCounts.Get(craftCost.id) < craftCost.amount)
			{
				return false;
			}
		}
		return true;
	}

	public int GetRefineryCount(Identifiable.Id id)
	{
		return model.craftMatCounts.Get(id);
	}

	public bool TryToSpendFromRefinery(GadgetDefinition.CraftCost[] costs)
	{
		if (!HasInRefinery(costs))
		{
			return false;
		}
		foreach (GadgetDefinition.CraftCost craftCost in costs)
		{
			if (craftCost.amount > 0)
			{
				model.craftMatCounts[craftCost.id] -= craftCost.amount;
			}
		}
		return true;
	}

	public void IncrementPlacedGadgetCount(Gadget.Id id)
	{
		int num = model.placedGadgetCounts.Get(id);
		model.placedGadgetCounts[id] = num + 1;
	}

	public void DecrementPlacedGadgetCount(Gadget.Id id)
	{
		if (model.placedGadgetCounts.ContainsKey(id))
		{
			Dictionary<Gadget.Id, int> placedGadgetCounts = model.placedGadgetCounts;
			int value = placedGadgetCounts[id] - 1;
			placedGadgetCounts[id] = value;
		}
	}

	public int GetPlacedCount(List<Gadget.Id> gadgetCountIds)
	{
		int num = 0;
		foreach (Gadget.Id gadgetCountId in gadgetCountIds)
		{
			num += model.placedGadgetCounts.Get(gadgetCountId);
		}
		return num;
	}
}
