using System;

public class DroneAmmo : Ammo
{
	public const int MAX_COUNT = 50;

	public DroneAmmo()
		: base(SRSingleton<SceneContext>.Instance.PlayerState.GetPotentialAmmo(), 1, 1, new Predicate<Identifiable.Id>[1], (Identifiable.Id id, int index) => 50)
	{
	}

	public Identifiable.Id Pop()
	{
		Identifiable.Id slotName = GetSlotName();
		Decrement(slotName);
		return slotName;
	}

	public Identifiable.Id GetSlotName()
	{
		return GetSlotName(0);
	}

	public bool MaybeAddToSlot(Identifiable.Id id)
	{
		return MaybeAddToSpecificSlot(id, null, 0);
	}

	public new bool IsEmpty()
	{
		return GetSlotCount() <= 0;
	}

	public bool IsFull()
	{
		return GetSlotCount() >= GetSlotMaxCount();
	}

	public int GetSlotCount()
	{
		return GetSlotCount(0);
	}

	public int GetSlotMaxCount()
	{
		return GetSlotMaxCount(0);
	}

	public new bool CouldAddToSlot(Identifiable.Id id)
	{
		return CouldAddToSlot(id, 0, overflow: false);
	}

	public bool Any()
	{
		return GetSlotCount() > 0;
	}

	public bool None()
	{
		return GetSlotCount() <= 0;
	}
}
