public class HealthMeter : SRBehaviour
{
	private PlayerState player;

	private StatusBar statusBar;

	public void Start()
	{
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		statusBar = GetComponent<StatusBar>();
		Update();
	}

	public void Update()
	{
		statusBar.currValue = player.GetCurrHealth();
		statusBar.maxValue = player.GetMaxHealth();
	}
}
