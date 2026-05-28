using UnityEngine;

public class PlortInstability : SRBehaviour
{
	public float lifetimeHours = 0.5f;

	public float explodePower = 400f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 10f;

	public float maxPlayerDamage = 30f;

	public GameObject explodeFX;

	private double destroyTime;

	private TimeDirector timeDir;

	public void Awake()
	{
		if (!SRSingleton<SceneContext>.Instance.ModDirector.PlortsUnstable())
		{
			Destroyer.Destroy(this, "PlortInstability.Awake");
		}
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		destroyTime = timeDir.HoursFromNowOrStart(lifetimeHours);
	}

	public void Update()
	{
		if (timeDir.HasReached(destroyTime))
		{
			Object.Instantiate(explodeFX, base.transform.position, base.transform.rotation);
			Destroyer.DestroyActor(base.gameObject, "PlortInstability.Update");
			PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage);
		}
	}
}
