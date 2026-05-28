using UnityEngine;

public class FallDamager : SRBehaviour, EventHandlerRegistrable
{
	public float minImpactForDamage = 0.2f;

	public float damagePerImpact = 300f;

	private vp_FPPlayerEventHandler playerEvents;

	private Damageable damageable;

	public void Awake()
	{
		playerEvents = GetComponentInChildren<vp_FPPlayerEventHandler>();
		damageable = GetInterfaceComponent<Damageable>();
	}

	protected virtual void OnEnable()
	{
		if (playerEvents != null)
		{
			Register(playerEvents);
		}
	}

	protected virtual void OnDisable()
	{
		if (playerEvents != null)
		{
			Unregister(playerEvents);
		}
	}

	public virtual void OnMessage_FallImpact(float val)
	{
		if (val > minImpactForDamage)
		{
			float f = (val - minImpactForDamage) * damagePerImpact;
			if (damageable.Damage(Mathf.RoundToInt(f), null))
			{
				DeathHandler.Kill(base.gameObject, DeathHandler.Source.FALL_DAMAGE, null, "FallDamager.OnMessage_FallImpact");
			}
		}
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterMessage<float>("FallImpact", OnMessage_FallImpact);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterMessage<float>("FallImpact", OnMessage_FallImpact);
	}
}
