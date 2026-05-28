public class SetMouseEnabled : SRBehaviour
{
	public bool mouseEnabled = true;

	private void Start()
	{
		vp_Utility.LockCursor = !mouseEnabled;
	}
}
