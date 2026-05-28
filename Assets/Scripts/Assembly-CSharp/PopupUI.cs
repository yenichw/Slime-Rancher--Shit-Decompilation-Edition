public abstract class PopupUI<T> : SRBehaviour
{
	protected T idEntry;

	public virtual void Init(T idEntry)
	{
		this.idEntry = idEntry;
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundleAvailable);
	}

	public abstract void OnBundleAvailable(MessageDirector msgDir);

	public virtual void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundleAvailable);
		}
	}
}
