using UnityEngine;

public class DestroyOnWater : SRBehaviour, LiquidConsumer
{
	public GameObject destroyFX;

	public bool destroyAsActor;

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			DestroyWithFX();
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		LiquidSource component = col.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId))
		{
			DestroyWithFX();
			return;
		}
		DestroyOnTouching component2 = col.GetComponent<DestroyOnTouching>();
		if (component2 != null && component2.wateringUnits > 0f)
		{
			DestroyWithFX();
		}
	}

	private void DestroyWithFX()
	{
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
		}
		if (destroyAsActor)
		{
			Destroyer.DestroyActor(base.gameObject, "DestroyOnWater.DestroyWithFX");
		}
		else
		{
			RequestDestroy("DestroyOnWater.DestroyWithFX");
		}
	}
}
