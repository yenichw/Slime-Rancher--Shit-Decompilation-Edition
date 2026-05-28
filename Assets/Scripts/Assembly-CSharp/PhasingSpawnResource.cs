using System;
using UnityEngine;
using UnityEngine.Events;

public class PhasingSpawnResource : PhaseableObject
{
	private SpawnResource spawnResource;

	private bool readyToPhase;

	public GameObject phaseOutFx;

	public GameObject phaseInFx;

	public void Awake()
	{
		spawnResource = GetComponent<SpawnResource>();
		SpawnResource obj = spawnResource;
		obj.onReachedSpawnTime = (UnityAction)Delegate.Combine(obj.onReachedSpawnTime, (UnityAction)delegate
		{
			readyToPhase = true;
		});
	}

	public override void PhaseIn()
	{
		spawnResource.RefreshSpawnJointObjectPositions();
		if (base.gameObject.activeInHierarchy)
		{
			SRBehaviour.SpawnAndPlayFX(phaseInFx, base.transform.position, base.transform.rotation);
		}
	}

	public override void PhaseOut()
	{
		if (base.gameObject.activeInHierarchy)
		{
			SRBehaviour.SpawnAndPlayFX(phaseOutFx, base.transform.position, base.transform.rotation);
		}
		readyToPhase = false;
	}

	public override bool ReadyToPhase()
	{
		return readyToPhase;
	}
}
