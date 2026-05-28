public class Map : SRSingleton<Map>
{
	public MapUI mapUI;

	private TimeDirector timeDir;

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void Start()
	{
		mapUI.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		if (SRInput.Actions.openMap.WasPressed && !timeDir.IsFastForwarding())
		{
			if (!mapUI.gameObject.activeSelf)
			{
				OpenMap(ZoneDirector.Zone.NONE);
			}
		}
		else if (SRInput.PauseActions.closeMap.WasPressed && mapUI.gameObject.activeSelf)
		{
			CloseMap();
		}
	}

	public void OpenMap(ZoneDirector.Zone unlockedZone)
	{
		if (unlockedZone != ZoneDirector.Zone.NONE)
		{
			mapUI.AddZoneToReveal(unlockedZone);
		}
		mapUI.gameObject.SetActive(value: true);
		mapUI.OpenMap();
	}

	private void CloseMap()
	{
		mapUI.Close();
	}

	public void RegisterMarker(DisplayOnMap marker)
	{
		mapUI.RegisterObject(marker);
	}

	public void DeregisterMarker(DisplayOnMap marker)
	{
		mapUI.DeregisterObject(marker);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
