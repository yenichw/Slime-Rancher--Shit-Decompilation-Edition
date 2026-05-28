public class GlitchTarrNodeSpawner : DirectedSlimeSpawner
{
	private GlitchTarrNode node;

	private GameModeConfig config;

	public override void Awake()
	{
		base.Awake();
		config = SRSingleton<SceneContext>.Instance.GameModeConfig;
		node = GetRequiredComponent<GlitchTarrNode>();
	}

	public override bool CanSpawn(float? forHour = null)
	{
		if (node.GetState() == GlitchTarrNode.State.ACTIVE && !config.GetModeSettings().preventHostiles)
		{
			return base.CanSpawn(forHour);
		}
		return false;
	}

	protected override void Register(CellDirector director)
	{
		(director as GlitchCellDirector).Register(this);
	}
}
