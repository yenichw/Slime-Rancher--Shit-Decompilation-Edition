using UnityEngine.UI;

public class SRToggle : Toggle
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.group != null)
		{
			((SRToggleGroup)base.group).OnToggleEnable(this);
		}
	}

	protected override void OnDisable()
	{
		if (base.group != null)
		{
			((SRToggleGroup)base.group).OnToggleWillDisable(this);
		}
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		if (base.group != null)
		{
			((SRToggleGroup)base.group).OnToggleEnable(this);
		}
		base.OnDestroy();
	}
}
