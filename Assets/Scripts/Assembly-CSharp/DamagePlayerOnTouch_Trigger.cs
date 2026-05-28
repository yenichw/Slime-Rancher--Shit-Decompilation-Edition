using UnityEngine;

public class DamagePlayerOnTouch_Trigger : RegisteredActorBehaviour, RegistryUpdateable
{
	[Tooltip("Amount of damage applied each tick.")]
	public int damagePerTick;

	[Tooltip("Amount of time in between ticks. (in-game minutes)")]
	public float cooldownPerTick;

	private TimeDirector timeDirector;

	protected double nextTime;

	protected GameObject damageGameObject;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (damageGameObject == null && PhysicsUtil.IsPlayerMainCollider(collider))
		{
			damageGameObject = collider.gameObject;
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (damageGameObject == collider.gameObject)
		{
			damageGameObject = null;
		}
	}

	public virtual void RegistryUpdate()
	{
		if (damageGameObject != null && timeDirector.HasReached(nextTime))
		{
			if (damageGameObject.GetInterfaceComponent<Damageable>().Damage(damagePerTick, base.gameObject))
			{
				DeathHandler.Kill(damageGameObject, DeathHandler.Source.SLIME_DAMAGE_PLAYER_ON_TOUCH, base.gameObject, "DamagePlayerOnTouch_Trigger.RegistryUpdate");
			}
			nextTime = timeDirector.HoursFromNow(cooldownPerTick * (1f / 60f));
		}
	}
}
