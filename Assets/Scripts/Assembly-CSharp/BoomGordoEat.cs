using UnityEngine;

public class BoomGordoEat : GordoEat, BoomMaterialAnimator.BoomMaterialInformer
{
	public float explodePower = 600f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 15f;

	public float maxPlayerDamage = 45f;

	protected override void WillStartBurst()
	{
		base.WillStartBurst();
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: true);
	}

	protected override void DidCompleteBurst()
	{
		base.DidCompleteBurst();
		PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage, base.gameObject);
		GetComponentsInChildren<ExplodeIndicatorMarker>(includeInactive: true)[0].SetActive(active: false);
	}

	public float GetReadiness()
	{
		return Mathf.Lerp(0.2f, 1f, GetPercentageFed());
	}

	public float GetRecoveriness()
	{
		return 0f;
	}
}
