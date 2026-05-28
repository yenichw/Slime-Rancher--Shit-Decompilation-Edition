public class GordoDisplayOnMap : DisplayOnMap
{
	private GordoEat gordoEat;

	public override void Awake()
	{
		base.Awake();
		gordoEat = GetComponent<GordoEat>();
	}

	public override bool ShowOnMap()
	{
		if (base.ShowOnMap())
		{
			int num = gordoEat.GetEatenCount();
			if (SRSingleton<SceneContext>.Instance.GameModel.currGameMode == PlayerState.GameMode.TIME_LIMIT_V2)
			{
				GordoNearBurstOnGameMode component = base.gameObject.GetComponent<GordoNearBurstOnGameMode>();
				num -= (int)((component == null) ? 0 : (gordoEat.GetTargetCount() - component.remaining));
			}
			return num > 0;
		}
		return false;
	}
}
