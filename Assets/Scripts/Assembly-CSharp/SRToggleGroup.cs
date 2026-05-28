using UnityEngine.UI;

public class SRToggleGroup : ToggleGroup
{
	private SRToggle slime1992_previousEnabled;

	private bool slime1992_requiresHotfix;

	protected override void Awake()
	{
		base.Awake();
		slime1992_requiresHotfix = !base.allowSwitchOff;
	}

	public void OnToggleEnable(SRToggle instance)
	{
		if (slime1992_requiresHotfix && instance == slime1992_previousEnabled)
		{
			slime1992_previousEnabled.SetIsOnWithoutNotify(value: true);
			slime1992_previousEnabled = null;
			base.allowSwitchOff = false;
		}
	}

	public void OnToggleWillDisable(SRToggle instance)
	{
		if (slime1992_requiresHotfix && instance != null && instance.isOn)
		{
			base.allowSwitchOff = true;
			slime1992_previousEnabled = instance;
			slime1992_previousEnabled.SetIsOnWithoutNotify(value: false);
		}
	}
}
