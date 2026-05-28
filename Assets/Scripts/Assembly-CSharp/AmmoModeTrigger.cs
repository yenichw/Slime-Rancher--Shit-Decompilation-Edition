using UnityEngine;

public class AmmoModeTrigger : MonoBehaviour
{
	[Tooltip("Ammo mode to set on entering the trigger.")]
	public PlayerState.AmmoMode onEnter;

	[Tooltip("Ammo mode to set on exiting the trigger.")]
	public PlayerState.AmmoMode onExit;

	private PlayerState playerState;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider))
		{
			playerState.SetAmmoMode(onEnter);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider))
		{
			playerState.SetAmmoMode(onExit);
		}
	}
}
