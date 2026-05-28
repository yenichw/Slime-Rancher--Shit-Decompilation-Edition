public class ScienceUIActivator : UIActivator
{
	public override bool CanActivate()
	{
		return SRSingleton<SceneContext>.Instance.ProgressDirector.HasProgress(ProgressDirector.ProgressType.UNLOCK_LAB);
	}
}
