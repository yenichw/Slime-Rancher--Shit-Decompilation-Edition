using InControl;

public class UWPControllerDisconnectPopupUI : BaseUI
{
	protected override bool Closeable()
	{
		return false;
	}

	public override void Update()
	{
		base.Update();
		if (InputManager.InputDetected())
		{
			Close();
		}
	}
}
