using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public abstract class BreakOnImpactBase : SRBehaviour
{
	public GameObject breakFX;

	private const float COLLISION_THRESHOLD = 14f;

	private Rigidbody body;

	private bool breaking;

	public virtual void Awake()
	{
		body = GetComponent<Rigidbody>();
	}

	public void OnCollisionEnter(Collision col)
	{
		if (!col.collider.isTrigger && !body.isKinematic)
		{
			float num = 0f;
			ContactPoint[] contacts = col.contacts;
			foreach (ContactPoint contactPoint in contacts)
			{
				num = Mathf.Max(num, Vector3.Dot(contactPoint.normal, col.relativeVelocity));
			}
			if (num > 14f)
			{
				BreakOpen();
			}
		}
	}

	private void BreakOpen()
	{
		if (breaking)
		{
			return;
		}
		breaking = true;
		SRBehaviour.SpawnAndPlayFX(breakFX, base.gameObject.transform.position, base.gameObject.transform.rotation);
		Destroyer.DestroyActor(base.gameObject, "BreakOnImpact.BreakOpen");
		RegionRegistry.RegionSetId setId = GetComponent<RegionMember>().setId;
		foreach (GameObject rewardPrefab in GetRewardPrefabs())
		{
			Vector3 position = base.transform.position + Random.insideUnitSphere;
			Rigidbody component = SRBehaviour.InstantiateActor(rewardPrefab, setId, position, Quaternion.identity, nonActorOk: true).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.AddTorque(Random.insideUnitSphere);
				component.AddForce(Random.insideUnitSphere);
			}
		}
	}

	protected abstract IEnumerable<GameObject> GetRewardPrefabs();
}
