using UnityEngine;

public class PediaAreaTrigger : SRBehaviour
{
	public PediaDirector.Id pediaId;

	public void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == SRSingleton<SceneContext>.Instance.Player && col is CharacterController)
		{
			SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(pediaId);
		}
	}
}
