public class GlitchKeepUpright : KeepUpright
{
	private Vacuumable vacuumable;

	public void Awake()
	{
		vacuumable = GetComponent<Vacuumable>();
	}

	public override void RegistryFixedUpdate()
	{
		if (!vacuumable.isCaptive())
		{
			base.RegistryFixedUpdate();
		}
	}
}
