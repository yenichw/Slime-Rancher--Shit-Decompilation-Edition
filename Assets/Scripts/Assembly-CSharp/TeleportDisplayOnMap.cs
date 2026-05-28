public class TeleportDisplayOnMap : DisplayOnMap
{
	private TeleportSource teleportSrc;

	public override void Awake()
	{
		base.Awake();
		teleportSrc = GetComponentInChildren<TeleportSource>();
	}

	public override bool ShowOnMap()
	{
		if (base.ShowOnMap())
		{
			return SRSingleton<SceneContext>.Instance.TeleportNetwork.IsLinkFullyActive(teleportSrc);
		}
		return false;
	}
}
