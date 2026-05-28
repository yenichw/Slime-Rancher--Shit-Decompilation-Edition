using UnityEngine;

public static class DeathHandler
{
	public enum Source
	{
		UNDEFINED = 0,
		SLIME_ATTACK = 1,
		SLIME_ATTACK_PLAYER = 2,
		SLIME_CRYSTAL_SPIKES = 3,
		SLIME_DAMAGE_PLAYER_ON_TOUCH = 4,
		SLIME_EXPLODE = 5,
		SLIME_IGNITE = 6,
		SLIME_RAD = 7,
		CHICKEN_VAMPIRISM = 8,
		KILL_ON_TRIGGER = 9,
		EMERGENCY_RETURN = 10,
		FALL_DAMAGE = 11
	}

	public interface Interface
	{
		void OnDeath(Source source, GameObject sourceGameObject, string stackTrace);
	}

	public static void Kill(GameObject gameObject, Source source, GameObject sourceGameObject, string stackTrace)
	{
		Interface interfaceComponent = gameObject.GetInterfaceComponent<Interface>();
		if (interfaceComponent != null)
		{
			interfaceComponent.OnDeath(source, sourceGameObject, stackTrace);
		}
		else
		{
			Destroyer.DestroyActor(gameObject, stackTrace, okIfNonActor: true);
		}
	}
}
