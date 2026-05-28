public class ConfirmUI : BaseUI
{
	public delegate void OnConfirm();

	public OnConfirm onConfirm;

	public void OK()
	{
		Destroyer.Destroy(base.gameObject, "ConfirmUI.OK");
		onConfirm();
	}

	public void Cancel()
	{
		Destroyer.Destroy(base.gameObject, "ConfirmUI.Cancel");
	}
}
