using UnityEngine;

public class PlayerIgniteReact : MonoBehaviour, Ignitable
{
	public int damagePerIgnite = 10;

	public float repeatTime = 1f;

	private PlayerDamageable damageable;

	private double nextTime;

	public void Awake()
	{
		damageable = GetComponent<PlayerDamageable>();
	}

	public void Ignite(GameObject igniter)
	{
		if ((double)Time.time >= nextTime)
		{
			TryToDamage(igniter);
		}
	}

	private void TryToDamage(GameObject igniter)
	{
		if (damageable.Damage(damagePerIgnite, base.gameObject))
		{
			DeathHandler.Kill(base.gameObject, DeathHandler.Source.SLIME_IGNITE, igniter, "PlayerIgniteReact.TryToDamage");
		}
		nextTime = Time.time + repeatTime;
	}
}
