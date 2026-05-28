using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class SiloStorage : MonoBehaviour, LandPlotModel.Participant
{
	public enum StorageType
	{
		NON_SLIMES = 0,
		PLORT = 1,
		FOOD = 2,
		CRAFTING = 3,
		ELDER = 4
	}

	public class StorageTypeComparer : IEqualityComparer<StorageType>
	{
		public static StorageTypeComparer Instance = new StorageTypeComparer();

		public bool Equals(StorageType a, StorageType b)
		{
			return a == b;
		}

		public int GetHashCode(StorageType a)
		{
			return (int)a;
		}
	}

	public StorageType type;

	public int numSlots = 4;

	public int maxAmmo = 100;

	protected LandPlotModel model;

	protected Ammo ammo;

	private const int SILO_FULL_ACHIEVE_SLOTS = 12;

	private const int SILO_FULL_ACHIEVE_AMOUNT = 50;

	public Ammo LocalAmmo => ammo;

	public virtual void Awake()
	{
		InitAmmo();
	}

	private void InitAmmo()
	{
		if (ammo == null)
		{
			ammo = new Ammo(type.GetContents(), numSlots, numSlots, new Predicate<Identifiable.Id>[numSlots], (Identifiable.Id id, int index) => maxAmmo);
		}
	}

	public void InitModel(LandPlotModel model)
	{
		InitAmmo();
		model.siloAmmo[type] = new AmmoModel();
		LocalAmmo.InitModel(model.siloAmmo[type]);
	}

	public void SetModel(LandPlotModel model)
	{
		this.model = model;
		LocalAmmo.SetModel(model.siloAmmo[type]);
	}

	public virtual Ammo GetRelevantAmmo()
	{
		return ammo;
	}

	public Identifiable.Id GetSlotIdentifiable(int slotIdx)
	{
		return GetRelevantAmmo().GetSlotName(slotIdx);
	}

	public int GetSlotCount(int slotIdx)
	{
		return GetRelevantAmmo().GetSlotCount(slotIdx);
	}

	public bool MaybeAddIdentifiable(Identifiable.Id id)
	{
		bool result = GetRelevantAmmo().MaybeAddToSlot(id, null);
		OnAdded();
		return result;
	}

	public bool MaybeAddIdentifiable(Identifiable.Id id, int slotIdx, int count = 1, bool overflow = false)
	{
		bool result = GetRelevantAmmo().MaybeAddToSpecificSlot(id, null, slotIdx, count, overflow);
		OnAdded();
		return result;
	}

	public bool CanAccept(Identifiable.Id id)
	{
		return GetRelevantAmmo().CouldAddToSlot(id);
	}

	public bool CanAccept(Identifiable.Id id, int slotIdx, bool overflow)
	{
		return GetRelevantAmmo().CouldAddToSlot(id, slotIdx, overflow);
	}

	private void OnAdded()
	{
		if (GetRelevantAmmo().HasFullSlots(12, 50))
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.FILLED_SILO, 1);
		}
	}
}
