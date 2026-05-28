using UnityEngine;

public class SiloSlotUI : StorageSlotUI
{
	[Tooltip("Silo slot index.")]
	public int slotIdx;

	private SiloStorage storage;

	public override void Awake()
	{
		base.Awake();
		storage = GetComponentInParent<SiloStorage>();
	}

	protected override Identifiable.Id GetCurrentId()
	{
		return storage.GetSlotIdentifiable(slotIdx);
	}

	protected override int GetCurrentCount()
	{
		return storage.GetSlotCount(slotIdx);
	}
}
