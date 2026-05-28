using UnityEngine.UI;

public class RadMeter : SRBehaviour
{
	public Image icon;

	private PlayerState player;

	private StatusBar energyBar;

	private bool wasVisible;

	private bool forceUpdate;

	private void Start()
	{
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		energyBar = GetComponent<StatusBar>();
		forceUpdate = true;
		Update();
	}

	private void Update()
	{
		int currRad = player.GetCurrRad();
		energyBar.currValue = currRad;
		bool flag = currRad > 0;
		if (flag != wasVisible || forceUpdate)
		{
			forceUpdate = false;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				base.transform.GetChild(i).gameObject.SetActive(flag);
			}
			icon.enabled = flag;
			wasVisible = flag;
		}
	}
}
