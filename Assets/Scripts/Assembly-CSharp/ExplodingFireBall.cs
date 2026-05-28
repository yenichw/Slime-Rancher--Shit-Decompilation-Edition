using UnityEngine;

public class ExplodingFireBall : FireBall
{
	public float explodePower = 600f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 15f;

	public float maxPlayerDamage = 45f;

	public GameObject explodeFX;

	protected override void OnExpire()
	{
		Explode();
	}

	public void Explode()
	{
		if (!defused)
		{
			PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage, ignites: true);
			if (explodeFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(explodeFX, base.transform.position, base.transform.rotation);
			}
		}
	}
}
