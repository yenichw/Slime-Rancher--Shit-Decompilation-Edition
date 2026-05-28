public class DecorizerSlotUI : StorageSlotUI
{
	private DecorizerStorage storage;

	public override void Awake()
	{
		base.Awake();
		storage = GetComponentInParent<DecorizerStorage>();
	}

	protected override Identifiable.Id GetCurrentId()
	{
		return storage.selected;
	}

	protected override int GetCurrentCount()
	{
		return storage.GetCount();
	}
}
