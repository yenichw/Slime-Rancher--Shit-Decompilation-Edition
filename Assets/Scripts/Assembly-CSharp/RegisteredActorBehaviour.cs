public abstract class RegisteredActorBehaviour : SRBehaviour
{
	protected bool hasStarted;

	public virtual void Start()
	{
		hasStarted = true;
	}

	public virtual void OnEnable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.ActorRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.ActorRegistry.Register(this);
		}
	}

	public virtual void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.ActorRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.ActorRegistry.Deregister(this);
		}
	}

	public virtual void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.ActorRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.ActorRegistry.Deregister(this);
		}
	}
}
