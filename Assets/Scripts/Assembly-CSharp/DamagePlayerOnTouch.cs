using UnityEngine;

public class DamagePlayerOnTouch : SRBehaviour, ControllerCollisionListener
{
	public int damagePerTouch = 10;

	public float repeatTime = 1f;

	private bool blocked;

	private float nextTime;

	private const float INIT_NO_DAMAGE_WINDOW = 0.1f;

	public void Awake()
	{
		ResetDamageAmnesty();
	}

	public void ResetDamageAmnesty()
	{
		nextTime = Time.time + 0.1f;
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		if (Time.time >= nextTime)
		{
			TryToDamage(gameObj);
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (Time.time >= nextTime && col.gameObject == SRSingleton<SceneContext>.Instance.Player)
		{
			TryToDamage(col.gameObject);
		}
	}

	public void SetBlocked(bool blocked)
	{
		this.blocked = blocked;
	}

	private void TryToDamage(GameObject gameObj)
	{
		if (!blocked && gameObj.GetInterfaceComponent<Damageable>().Damage(damagePerTouch, base.gameObject))
		{
			DeathHandler.Kill(gameObj, DeathHandler.Source.SLIME_DAMAGE_PLAYER_ON_TOUCH, base.gameObject, "DamagePlayerOnTouch.TryToDamage");
		}
		nextTime = Time.time + repeatTime;
	}
}
