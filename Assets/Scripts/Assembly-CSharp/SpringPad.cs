using UnityEngine;

public class SpringPad : MonoBehaviour, ControllerCollisionListener
{
	private static Vector3 UP_ACCEL = Vector3.up * 5f;

	private static Vector3 UP_PLAYER_FORCE = Vector3.up * 1.667f;

	public Animator anim;

	private int springAnimId;

	private double nextPlayerHitTime;

	private TimeDirector timeDir;

	private WaitForChargeup waiter;

	public void Awake()
	{
		springAnimId = Animator.StringToHash("spring");
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		waiter = GetComponentInParent<WaitForChargeup>();
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		if (!waiter.IsWaiting())
		{
			vp_FPController component = gameObj.GetComponent<vp_FPController>();
			if (component != null && timeDir.HasReached(nextPlayerHitTime))
			{
				component.AddSoftForce(UP_PLAYER_FORCE, 5f);
				anim.SetTrigger(springAnimId);
				nextPlayerHitTime = timeDir.HoursFromNow(1f / 60f);
			}
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (!waiter.IsWaiting() && col.gameObject.layer != 16)
		{
			Rigidbody rigidbody = col.rigidbody;
			if (rigidbody != null)
			{
				rigidbody.AddForce(UP_ACCEL, ForceMode.VelocityChange);
				anim.SetTrigger(springAnimId);
			}
		}
	}
}
