using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

public class Ammo
{
	public class Slot
	{
		public readonly Identifiable.Id id;

		public int count;

		public SlimeEmotionData emotions;

		public Slot(Identifiable.Id id, int count)
		{
			this.id = id;
			this.count = count;
		}

		public void AverageIn(SlimeEmotions emotions)
		{
			if (this.emotions == null)
			{
				this.emotions = new SlimeEmotionData(emotions);
			}
			else
			{
				this.emotions.AverageIn(emotions, 1f / (float)count);
			}
		}
	}

	[Serializable]
	public class AmmoData
	{
		public Identifiable.Id id;

		public int count;

		public SlimeEmotionData emotionData;

		public AmmoData(Identifiable.Id id, int count, SlimeEmotionData emotionData)
		{
			this.id = id;
			this.count = count;
			this.emotionData = emotionData;
		}

		public override bool Equals(object o)
		{
			if (!(o is AmmoData))
			{
				return false;
			}
			AmmoData ammoData = (AmmoData)o;
			if (id == ammoData.id && count == ammoData.count)
			{
				if (emotionData != null)
				{
					return emotionData.Equals(ammoData.emotionData);
				}
				return ammoData.emotionData == null;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode() ^ count.GetHashCode() ^ ((emotionData != null) ? emotionData.GetHashCode() : 0);
		}
	}

	private AmmoModel ammoModel;

	private int selectedAmmoIdx;

	private readonly HashSet<Identifiable.Id> potentialAmmo;

	private int numSlots;

	private int initUsableSlots;

	private Func<Identifiable.Id, int, int> initSlotMaxCountFunction;

	private Predicate<Identifiable.Id>[] slotPreds;

	private TimeDirector timeDir;

	private LookupDirector lookupDir;

	private double waterIsMagicUntil = double.NegativeInfinity;

	private const float MAGIC_WATER_LIFETIME = 0.5f;

	private Slot[] Slots => ammoModel.slots;

	public Ammo(HashSet<Identifiable.Id> potentialAmmo, int numSlots, int usableSlots, Predicate<Identifiable.Id>[] slotPreds, Func<Identifiable.Id, int, int> slotMaxCountFunction)
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		this.potentialAmmo = potentialAmmo;
		this.numSlots = numSlots;
		this.slotPreds = slotPreds;
		initUsableSlots = usableSlots;
		initSlotMaxCountFunction = slotMaxCountFunction;
	}

	public void InitModel(AmmoModel model)
	{
		model.Reset(numSlots, initUsableSlots, initSlotMaxCountFunction);
	}

	public void SetModel(AmmoModel model)
	{
		ValidateAndAdjustSlots(ref model.slots);
		ammoModel = model;
	}

	public Predicate<Identifiable.Id> GetSlotPredicate(int index)
	{
		return slotPreds[index];
	}

	public bool SetAmmoSlot(int idx)
	{
		if (selectedAmmoIdx != idx && idx < ammoModel.usableSlots)
		{
			selectedAmmoIdx = idx;
			return true;
		}
		return false;
	}

	public void NextAmmoSlot()
	{
		selectedAmmoIdx = (selectedAmmoIdx + 1) % ammoModel.usableSlots;
	}

	public void PrevAmmoSlot()
	{
		selectedAmmoIdx = (selectedAmmoIdx + ammoModel.usableSlots - 1) % ammoModel.usableSlots;
	}

	public Identifiable.Id GetSlotName(int slotIdx)
	{
		if (Slots[slotIdx] == null)
		{
			return Identifiable.Id.NONE;
		}
		return AdjustId(Slots[slotIdx].id);
	}

	public SlimeEmotionData GetSlimeEmotionData(int slotIdx)
	{
		if (Slots[slotIdx] == null)
		{
			return new SlimeEmotionData();
		}
		return Slots[slotIdx].emotions;
	}

	public int GetCount(Identifiable.Id id)
	{
		for (int i = 0; i < GetUsableSlotCount(); i++)
		{
			if (Slots[i] != null && Slots[i].id == id)
			{
				return Slots[i].count;
			}
		}
		return 0;
	}

	public int GetSlotCount(int slotIdx)
	{
		if (Slots[slotIdx] == null)
		{
			return 0;
		}
		return Slots[slotIdx].count;
	}

	public int GetSelectedAmmoIdx()
	{
		return selectedAmmoIdx;
	}

	public int? GetAmmoIdx(Identifiable.Id id)
	{
		for (int i = 0; i < GetUsableSlotCount(); i++)
		{
			if (Slots[i] != null && Slots[i].id == id)
			{
				return i;
			}
		}
		return null;
	}

	public Identifiable.Id GetSelectedId()
	{
		Slot slot = Slots[selectedAmmoIdx];
		if (slot != null)
		{
			return AdjustId(slot.id);
		}
		return Identifiable.Id.NONE;
	}

	public void RegisterPotentialAmmo(GameObject prefab)
	{
		potentialAmmo.Add(Identifiable.GetId(prefab));
	}

	public int GetSlotMaxCount(int index)
	{
		return GetSlotMaxCount(GetSlotName(index), index);
	}

	public int GetSlotMaxCount(Identifiable.Id id, int index)
	{
		return ammoModel.GetSlotMaxCount(id, index);
	}

	private Identifiable.Id AdjustId(Identifiable.Id id)
	{
		if (Identifiable.IsWater(id) && !timeDir.HasReached(waterIsMagicUntil))
		{
			return Identifiable.Id.MAGIC_WATER_LIQUID;
		}
		return id;
	}

	public GameObject GetSelectedStored()
	{
		Slot slot = Slots[selectedAmmoIdx];
		if (slot == null)
		{
			return null;
		}
		return lookupDir.GetPrefab(AdjustId(slot.id));
	}

	public Dictionary<SlimeEmotions.Emotion, float> GetSelectedEmotions()
	{
		return Slots[selectedAmmoIdx].emotions;
	}

	public bool HasSelectedAmmo()
	{
		if (Slots[selectedAmmoIdx] != null)
		{
			return Slots[selectedAmmoIdx].count > 0;
		}
		return false;
	}

	public void DecrementSelectedAmmo(int amount = 1)
	{
		if (Slots[selectedAmmoIdx] != null && (!Identifiable.IsWater(Slots[selectedAmmoIdx].id) || timeDir.HasReached(waterIsMagicUntil)))
		{
			Slots[selectedAmmoIdx].count = Math.Max(Slots[selectedAmmoIdx].count - amount, 0);
			if (Slots[selectedAmmoIdx].count == 0)
			{
				Slots[selectedAmmoIdx] = null;
			}
		}
	}

	public void Clear()
	{
		Clear((int i) => true);
	}

	public void Clear(Predicate<int> predicate)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (predicate(i))
			{
				Clear(i);
			}
		}
	}

	public void ClearSelected()
	{
		Clear(selectedAmmoIdx);
	}

	public void Clear(int index)
	{
		if (Identifiable.IsWater(GetSlotName(index)))
		{
			waterIsMagicUntil = double.NegativeInfinity;
		}
		Slots[index] = null;
	}

	public bool ReplaceWithQuicksilverAmmo(Identifiable.Id id, int count)
	{
		if (!potentialAmmo.Contains(id))
		{
			return false;
		}
		int? num = null;
		int? num2 = null;
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i] != null && Slots[i].id == id)
			{
				if (Slots[i].count < GetSlotMaxCount(id, i))
				{
					Slots[i] = new Slot(id, Mathf.Min(GetSlotMaxCount(id, i), Slots[i].count + count));
					return true;
				}
				return false;
			}
			if ((slotPreds[i] == null || slotPreds[i](id)) && (!num2.HasValue || GetSlotCount(i) < num2.Value))
			{
				num2 = GetSlotCount(i);
				num = i;
			}
		}
		if (num.HasValue)
		{
			Slots[num.Value] = new Slot(id, Mathf.Min(GetSlotMaxCount(id, num.Value), count));
			return true;
		}
		return false;
	}

	public bool Contains(Identifiable.Id id)
	{
		Slot[] slots = Slots;
		foreach (Slot slot in slots)
		{
			if (slot != null && slot.id == id)
			{
				return true;
			}
		}
		return false;
	}

	public void Decrement(Identifiable.Id id, int count = 1)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			Slot slot = Slots[i];
			if (slot != null && slot.id == id)
			{
				Decrement(i, count);
				return;
			}
		}
		throw new InvalidOperationException("Cannot decrement ammo we don't have: " + id);
	}

	public void Decrement(int index, int count = 1)
	{
		Slots[index].count -= count;
		if (Slots[index].count <= 0)
		{
			Slots[index] = null;
		}
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			Slot slot = Slots[i];
			if (slot != null && slot.count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool Any(Predicate<Identifiable.Id> predicate)
	{
		for (int i = 0; i < GetUsableSlotCount(); i++)
		{
			if (Slots[i] != null && Slots[i].count > 0 && predicate(Slots[i].id))
			{
				return true;
			}
		}
		return false;
	}

	public double GetRemainingWaterIsMagicMins()
	{
		double num = timeDir.HoursUntil(waterIsMagicUntil);
		if (num >= 0.0)
		{
			return num * 60.0;
		}
		return 0.0;
	}

	public float GetFullness(int index)
	{
		return Mathf.Min(1f, (float)GetSlotCount(index) / (float)GetSlotMaxCount(index));
	}

	public float GetSelectedFullness()
	{
		return GetFullness(selectedAmmoIdx);
	}

	public bool MaybeAddToSpecificSlot(Identifiable identifiable, int slotIdx)
	{
		return MaybeAddToSpecificSlot(identifiable.id, identifiable, slotIdx);
	}

	public bool MaybeAddToSpecificSlot(Identifiable.Id id, Identifiable identifiable, int slotIdx)
	{
		return MaybeAddToSpecificSlot(id, identifiable, slotIdx, GetAmountFilledPerVac(id, slotIdx));
	}

	public bool MaybeAddToSpecificSlot(Identifiable.Id id, Identifiable identifiable, int slotIdx, int count)
	{
		return MaybeAddToSpecificSlot(id, identifiable, slotIdx, count, overflow: false);
	}

	public bool MaybeAddToSpecificSlot(Identifiable.Id id, Identifiable identifiable, int slotIdx, int count, bool overflow)
	{
		if (Slots[slotIdx] == null)
		{
			if ((slotPreds[slotIdx] != null && !slotPreds[slotIdx](id)) || !potentialAmmo.Contains(id))
			{
				return false;
			}
			Slots[slotIdx] = new Slot(id, 0);
		}
		if (Slots[slotIdx].id == id)
		{
			if (!overflow && Slots[slotIdx].count + count > GetSlotMaxCount(id, slotIdx))
			{
				return false;
			}
			Slots[slotIdx].count += count;
			if (identifiable != null)
			{
				SlimeEmotions component = identifiable.GetComponent<SlimeEmotions>();
				if (component != null)
				{
					Slots[slotIdx].AverageIn(component);
				}
			}
			return true;
		}
		return false;
	}

	public bool CouldAddToSlot(Identifiable.Id id)
	{
		return CouldAddToSlot(id, 0, GetUsableSlotCount() - 1, overflow: false);
	}

	public bool CouldAddToSlot(Identifiable.Id id, int slotIdx, bool overflow)
	{
		return CouldAddToSlot(id, slotIdx, slotIdx, overflow);
	}

	private bool CouldAddToSlot(Identifiable.Id id, int indexMin, int indexMax, bool overflow)
	{
		if (id == Identifiable.Id.MAGIC_WATER_LIQUID)
		{
			id = Identifiable.Id.WATER_LIQUID;
		}
		if (!potentialAmmo.Contains(id))
		{
			return false;
		}
		for (int i = indexMin; i <= indexMax; i++)
		{
			if (Slots[i] == null)
			{
				if (slotPreds[i] == null || slotPreds[i](id))
				{
					return true;
				}
			}
			else if (Slots[i].id == id && (overflow || Slots[i].count < GetSlotMaxCount(id, i)))
			{
				return true;
			}
		}
		return false;
	}

	public bool MaybeAddToSlot(Identifiable.Id id, Identifiable identifiable)
	{
		bool flag = id == Identifiable.Id.MAGIC_WATER_LIQUID;
		if (flag)
		{
			id = Identifiable.Id.WATER_LIQUID;
		}
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < ammoModel.usableSlots; i++)
		{
			if (Slots[i] == null || Slots[i].id != id)
			{
				continue;
			}
			int slotMaxCount = GetSlotMaxCount(id, i);
			if (flag)
			{
				Slots[i].count = slotMaxCount;
				waterIsMagicUntil = timeDir.HoursFromNow(0.5f);
			}
			else if (Slots[i].count >= slotMaxCount)
			{
				flag3 = true;
			}
			else
			{
				Slots[i].count = Mathf.Min(slotMaxCount, Slots[i].count + GetAmountFilledPerVac(id, i));
				SlimeEmotions slimeEmotions = ((identifiable == null) ? null : identifiable.GetComponent<SlimeEmotions>());
				if (slimeEmotions != null)
				{
					Slots[i].AverageIn(slimeEmotions);
				}
			}
			flag2 = true;
			break;
		}
		if (!flag2)
		{
			for (int j = 0; j < ammoModel.usableSlots; j++)
			{
				if (flag2)
				{
					break;
				}
				if ((slotPreds[j] == null || slotPreds[j](id)) && Slots[j] == null && potentialAmmo.Contains(id))
				{
					SlimeEmotions slimeEmotions2 = ((identifiable == null) ? null : identifiable.GetComponent<SlimeEmotions>());
					if (flag)
					{
						Slots[j] = new Slot(id, GetSlotMaxCount(id, j));
						waterIsMagicUntil = timeDir.HoursFromNow(0.5f);
					}
					else
					{
						Slots[j] = new Slot(id, GetAmountFilledPerVac(id, j));
					}
					if (slimeEmotions2 != null)
					{
						Slots[j].AverageIn(slimeEmotions2);
					}
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			return !flag3;
		}
		return false;
	}

	private int GetAmountFilledPerVac(Identifiable.Id id, int index)
	{
		switch (id)
		{
		case Identifiable.Id.WATER_LIQUID:
			return 5;
		case Identifiable.Id.GLITCH_DEBUG_SPRAY_LIQUID:
			return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.debugSprayAmmoPerStation;
		default:
			return 1;
		}
	}

	public bool Replace(int index, Identifiable.Id id)
	{
		Slot slot = Slots[index];
		double num = waterIsMagicUntil;
		Clear(index);
		if (MaybeAddToSpecificSlot(id, null, index))
		{
			return true;
		}
		Slots[index] = slot;
		waterIsMagicUntil = num;
		return false;
	}

	public bool Replace(Identifiable.Id previous, Identifiable.Id next)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (GetSlotName(i) == previous)
			{
				Slots[i] = new Slot(next, Mathf.Min(GetSlotCount(i), GetSlotMaxCount(next, i)));
				if (Identifiable.IsSlime(Slots[i].id))
				{
					Slots[i].emotions = new SlimeEmotionData();
					Slots[i].emotions[SlimeEmotions.Emotion.AGITATION] = 0f;
					Slots[i].emotions[SlimeEmotions.Emotion.HUNGER] = 0.5f;
					Slots[i].emotions[SlimeEmotions.Emotion.FEAR] = 0f;
				}
				return true;
			}
		}
		return false;
	}

	public bool HasFullSlots(int numSlots, int fullToAmount)
	{
		if (ammoModel.usableSlots < numSlots)
		{
			return false;
		}
		for (int i = 0; i < ammoModel.usableSlots; i++)
		{
			if (GetSlotCount(i) < fullToAmount)
			{
				return false;
			}
		}
		return true;
	}

	public List<AmmoDataV02> ToSerializable()
	{
		return (from ii in Enumerable.Range(0, Slots.Length)
			select new AmmoDataV02
			{
				id = GetSlotName(ii),
				emotionData = new SlimeEmotionDataV02
				{
					emotionData = GetSlimeEmotionData(ii)
				},
				count = GetSlotCount(ii)
			}).ToList();
	}

	public void FromSerializable(List<AmmoDataV02> data)
	{
		if (data.Count > 0 && data.Count != Slots.Length)
		{
			Log.Warning("Unserializing ammo slot length differs, ignoring extra.");
		}
		for (int i = 0; i < Slots.Length && i < data.Count; i++)
		{
			if (data[i] == null || data[i].id == Identifiable.Id.NONE)
			{
				continue;
			}
			if (slotPreds[i] != null && !slotPreds[i](data[i].id))
			{
				Debug.Log("Unserializing ammo slot contained no-longer-legal ID, ignoring: " + data[i].id);
				continue;
			}
			if (!potentialAmmo.Contains(data[i].id))
			{
				Log.Warning("Unserializing ammo slot contained invalid ammo id: " + data[i].id);
				continue;
			}
			if (lookupDir.GetPrefab(data[i].id) == null)
			{
				Log.Warning("Found unknown ammo ID: " + data[i].id);
				continue;
			}
			Slots[i] = new Slot(data[i].id, data[i].count);
			Slots[i].emotions = new SlimeEmotionData();
			foreach (KeyValuePair<SlimeEmotions.Emotion, float> emotionDatum in data[i].emotionData.emotionData)
			{
				Slots[i].emotions.Add(emotionDatum.Key, emotionDatum.Value);
			}
		}
	}

	private void ValidateAndAdjustSlots(ref Slot[] slots)
	{
		if (slots.Length != slotPreds.Length)
		{
			Log.Warning("Unserializing ammo slot length different than expected, ignoring extra and/or padding.", "slots", slots.Length, "preds", slotPreds.Length);
			Slot[] array = slots;
			slots = new Slot[slotPreds.Length];
			for (int i = 0; i < slotPreds.Length; i++)
			{
				if (i < array.Length)
				{
					slots[i] = array[i];
				}
				else
				{
					slots[i] = null;
				}
			}
		}
		for (int j = 0; j < slotPreds.Length; j++)
		{
			if (slots[j] == null || slots[j].id == Identifiable.Id.NONE)
			{
				slots[j] = null;
			}
			else if (slotPreds[j] != null && !slotPreds[j](slots[j].id))
			{
				Debug.Log("Unserialized ammo slot contained no-longer-legal ID, ignoring: " + slots[j].id);
				slots[j] = null;
			}
			else if (!potentialAmmo.Contains(slots[j].id))
			{
				Log.Warning("Unserializing ammo slot contained invalid ammo id: " + slots[j].id);
				slots[j] = null;
			}
		}
	}

	public void IncreaseUsableSlots(int usableSlots)
	{
		ammoModel.IncreaseUsableSlots(usableSlots);
		Debug.Log("MST Increased slots: " + ammoModel.usableSlots + " set: " + usableSlots);
	}

	public int GetUsableSlotCount()
	{
		return ammoModel.usableSlots;
	}
}
