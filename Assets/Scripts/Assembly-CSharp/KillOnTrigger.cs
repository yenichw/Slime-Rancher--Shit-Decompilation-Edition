using UnityEngine;

public class KillOnTrigger : SRBehaviour
{
	public GameObject killFX;

	public GameObject playerKillFx;

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody component = collider.GetComponent<Rigidbody>();
		if (!collider.isTrigger && component != null && (!component.isKinematic || PhysicsUtil.IsPlayerMainCollider(collider)))
		{
			Debug.Log("Fallthrough destroying: " + collider.gameObject.name);
			DeathHandler.Kill(collider.gameObject, DeathHandler.Source.KILL_ON_TRIGGER, base.gameObject, "KillOnTrigger.OnTriggerEnter");
			if (PhysicsUtil.IsPlayerMainCollider(collider) && playerKillFx != null)
			{
				SRBehaviour.SpawnAndPlayFX(playerKillFx, collider.gameObject.transform.position, Quaternion.identity);
			}
			else if (killFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(killFX, collider.gameObject.transform.position, Quaternion.identity);
			}
		}
	}
}
