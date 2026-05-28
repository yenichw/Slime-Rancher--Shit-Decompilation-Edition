using UnityEngine;

public class CrystalSpikesLifecycle : SRBehaviour, LiquidConsumer
{
	[Tooltip("Lifetime of spikes in hours")]
	public float lifetime = 0.5f;

	public int damagePerHit = 10;

	public GameObject spawnFX;

	public GameObject destroyFX;

	private double destroyAt;

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		destroyAt = timeDir.HoursFromNowOrStart(lifetime);
		if (spawnFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(spawnFX, base.transform.position, base.transform.rotation);
		}
	}

	public void Update()
	{
		if (timeDir.HasReached(destroyAt))
		{
			bool flag = timeDir.HasReached(destroyAt + 3600.0);
			if (destroyFX != null && !flag)
			{
				SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
			}
			Destroyer.Destroy(base.gameObject, "CrystalSpikesLifecycle.Update");
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger)
		{
			Identifiable component = col.gameObject.GetComponent<Identifiable>();
			if (component != null && component.id == Identifiable.Id.PLAYER && col.gameObject.GetComponent<Damageable>().Damage(damagePerHit, base.gameObject))
			{
				DeathHandler.Kill(col.gameObject, DeathHandler.Source.SLIME_CRYSTAL_SPIKES, base.gameObject, "CrystalSpikesLifecycle.OnTriggerEnter");
			}
		}
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
			Destroyer.Destroy(base.gameObject, "CrystalSpikesLifecycle.AddLiquid");
		}
	}
}
