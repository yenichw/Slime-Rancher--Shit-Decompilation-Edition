public class LabAccessDoor : AccessDoor
{
	private PediaDirector pediaDir;

	private bool firstUpdate = true;

	public override void Awake()
	{
		base.Awake();
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
	}

	public override void Update()
	{
		base.Update();
		if (firstUpdate)
		{
			MaybeRecountProgress();
			firstUpdate = false;
		}
	}

	public override bool MaybeRecountProgress()
	{
		if (base.MaybeRecountProgress())
		{
			pediaDir.UnlockScience();
			return true;
		}
		return false;
	}
}
