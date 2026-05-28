public class GlitchStorageUI : StorageSlotUI
{
	private GlitchStorage storage;

	public override void Awake()
	{
		base.Awake();
		storage = GetComponentInParent<GlitchStorage>();
	}

	protected override Identifiable.Id GetCurrentId()
	{
		return storage.selected;
	}

	protected override int GetCurrentCount()
	{
		return storage.count;
	}
}
