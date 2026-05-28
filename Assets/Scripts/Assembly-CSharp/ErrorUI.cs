public class ErrorUI : SRBehaviour
{
	public void Close()
	{
		Destroyer.Destroy(base.gameObject, "ErrorUI.Close");
	}
}
