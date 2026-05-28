using UnityEngine;

public class PediaUIActivator : UIActivator
{
	public PediaDirector.Id pediaId;

	public override GameObject Activate()
	{
		return SRSingleton<SceneContext>.Instance.PediaDirector.ShowPedia(pediaId);
	}
}
