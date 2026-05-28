using UnityEngine;

public class ExplodeOnTouching : SRBehaviour
{
	public float explodePower = 600f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 15f;

	public float maxPlayerDamage = 45f;

	public bool ignites;

	public GameObject explodeFX;

	public void OnCollisionEnter(Collision col)
	{
		DestroyOnTouching component = col.gameObject.GetComponent<DestroyOnTouching>();
		if (component == null || component.wateringUnits <= 0f)
		{
			Explode();
		}
	}

	public void Explode()
	{
		PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage, ignites);
		if (explodeFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(explodeFX, base.transform.position, base.transform.rotation);
		}
		RequestDestroy("ExplodeOnTouching.Explode");
	}
}
