public class CollidableActorBehaviour : RegisteredActorBehaviour
{
	private CollisionAggregator collisionBehaviour;

	public virtual void Awake()
	{
		collisionBehaviour = GetComponent<CollisionAggregator>();
	}

	public override void Start()
	{
		base.Start();
		if (collisionBehaviour != null && base.enabled)
		{
			collisionBehaviour.Register(this);
		}
	}

	public override void OnEnable()
	{
		if (collisionBehaviour != null)
		{
			collisionBehaviour.Register(this);
		}
		base.OnEnable();
	}

	public override void OnDisable()
	{
		if (collisionBehaviour != null)
		{
			collisionBehaviour.Deregister(this);
		}
		base.OnDisable();
	}
}
