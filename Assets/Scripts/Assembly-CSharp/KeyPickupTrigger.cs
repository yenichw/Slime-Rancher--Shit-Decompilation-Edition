using UnityEngine;

public class KeyPickupTrigger : SRBehaviour
{
	public GameObject pickupFX;

	public void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == SRSingleton<SceneContext>.Instance.Player)
		{
			SRSingleton<SceneContext>.Instance.PlayerState.AddKey();
			if (pickupFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(pickupFX, base.transform.position, base.transform.rotation);
			}
			Destroyer.DestroyActor(base.gameObject, "KeyPickupTrigger.OnTriggerEnter");
		}
	}
}
