using System.Collections.Generic;
using UnityEngine;

public class ClearAreaFeralsOnHit : MonoBehaviour
{
	public float radius = 20f;

	public float minTimeBetween = 1f;

	public SECTR_AudioCue hitCue;

	private float nextTime;

	private WaitForChargeup waiter;

	public void Awake()
	{
		waiter = GetComponentInParent<WaitForChargeup>();
	}

	public void OnCollisionEnter(Collision col)
	{
		MaybeHandleCollision();
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		MaybeHandleCollision();
	}

	private void MaybeHandleCollision()
	{
		if (!waiter.IsWaiting() && Time.time >= nextTime)
		{
			HandleCollision();
			nextTime = Time.time + minTimeBetween;
		}
	}

	private void HandleCollision()
	{
		SphereOverlapTrigger.CreateGameObject(base.transform.position, radius, delegate(IEnumerable<Collider> colliders)
		{
			foreach (Collider collider in colliders)
			{
				SlimeFeral component = collider.GetComponent<SlimeFeral>();
				if (component != null)
				{
					component.ClearFeral(deagitate: true);
				}
			}
		});
		if (hitCue != null)
		{
			SECTR_AudioSystem.Play(hitCue, base.transform.position, loop: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}
}
