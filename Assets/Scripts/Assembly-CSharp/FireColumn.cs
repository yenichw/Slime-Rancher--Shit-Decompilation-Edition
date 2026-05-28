using System;
using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class FireColumn : SRBehaviour
{
	[Serializable]
	public class FireballEntry
	{
		public GameObject prefab;

		public float weight;

		public float minBallHeight;

		public float maxBallHeight;

		public float minBallEjectForce;

		public float maxBallEjectForce;
	}

	public FireballEntry[] fireballs;

	public Transform fireballParent;

	public GameObject fireEffectObj;

	public float minFireballDelay = 10f;

	public float maxFireballDelay = 20f;

	public float lifetimeHrs = 0.5f;

	public SECTR_AudioCue columnSpawnCue;

	public SECTR_AudioCue columnFireLoopCue;

	private SECTR_AudioCueInstance columnFireLookCueInstance;

	private bool fireActive;

	private double nextFireballTime;

	private double endOfLifeTime;

	private Dictionary<int, float> fireballEntryIdxWeightMap = new Dictionary<int, float>();

	private Dictionary<int, float> hibernatingFireballEntryIdxWeightMap = new Dictionary<int, float>();

	private TimeDirector timeDir;

	private Region region;

	private Collider columnCollider;

	private bool isInOasis;

	private bool deactivating;

	private const float FIZZLE_TIME = 2f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		region = GetComponentInParent<Region>();
		for (int i = 0; i < fireballs.Length; i++)
		{
			fireballEntryIdxWeightMap.Add(i, fireballs[i].weight);
			RegionMember component = fireballs[i].prefab.GetComponent<RegionMember>();
			if (component == null || !component.canHibernate)
			{
				hibernatingFireballEntryIdxWeightMap.Add(i, fireballs[i].weight);
			}
		}
		columnCollider = GetComponent<Collider>();
		columnCollider.enabled = false;
	}

	public void OnTriggerEnter(Collider col)
	{
		if (fireActive)
		{
			col.GetComponent<Ignitable>()?.Ignite(base.gameObject);
		}
	}

	public void ActivateFire()
	{
		if (base.isActiveAndEnabled && !fireActive && !deactivating)
		{
			SECTR_AudioSystem.Play(columnSpawnCue, base.transform.position, loop: false);
			columnFireLookCueInstance = SECTR_AudioSystem.Play(columnFireLoopCue, base.transform.position, loop: true);
			fireActive = true;
			fireEffectObj.SetActive(value: true);
			nextFireballTime = timeDir.WorldTime() + (double)Randoms.SHARED.GetInRange(0f, minFireballDelay);
			endOfLifeTime = timeDir.HoursFromNow(lifetimeHrs);
			columnCollider.enabled = true;
		}
	}

	public void DeactivateFire()
	{
		if (base.isActiveAndEnabled && fireActive)
		{
			deactivating = true;
			fireActive = false;
			StartCoroutine(FizzleThenDeactivateFireParticles());
			columnFireLookCueInstance.Stop(stopImmediately: true);
			columnCollider.enabled = false;
		}
	}

	public void OnDisable()
	{
		if (deactivating)
		{
			FinishDeactivate();
		}
	}

	private IEnumerator FizzleThenDeactivateFireParticles()
	{
		ParticleSystem[] componentsInChildren = fireEffectObj.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
		yield return new WaitForSeconds(2f);
		FinishDeactivate();
	}

	private void FinishDeactivate()
	{
		fireEffectObj.SetActive(value: false);
		ParticleSystem[] componentsInChildren = fireEffectObj.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
		deactivating = false;
	}

	public bool IsFireActive()
	{
		return fireActive;
	}

	public void Update()
	{
		if (fireActive)
		{
			if (timeDir.HasReached(nextFireballTime))
			{
				SpawnFireball();
				nextFireballTime = timeDir.WorldTime() + (double)Randoms.SHARED.GetInRange(minFireballDelay, maxFireballDelay);
			}
			if (timeDir.HasReached(endOfLifeTime))
			{
				DeactivateFire();
			}
		}
	}

	private void SpawnFireball()
	{
		Dictionary<int, float> dictionary = (IsHibernating() ? hibernatingFireballEntryIdxWeightMap : fireballEntryIdxWeightMap);
		if (dictionary.Count > 0)
		{
			int num = Randoms.SHARED.Pick(dictionary, -1);
			if (num >= 0)
			{
				FireballEntry fireballEntry = fireballs[num];
				GameObject gameObject = ((!(fireballEntry.prefab.GetComponent<Identifiable>() == null)) ? SRBehaviour.InstantiateActor(fireballEntry.prefab, region.setId, base.transform.position + base.transform.up * Randoms.SHARED.GetInRange(fireballEntry.minBallHeight, fireballEntry.maxBallHeight), Quaternion.identity) : SRBehaviour.InstantiatePooledDynamic(fireballEntry.prefab, base.transform.position + base.transform.up * Randoms.SHARED.GetInRange(fireballEntry.minBallHeight, fireballEntry.maxBallHeight), Quaternion.identity));
				gameObject.GetComponent<Rigidbody>().AddForce(UnityEngine.Random.onUnitSphere * Randoms.SHARED.GetInRange(fireballEntry.minBallEjectForce, fireballEntry.maxBallEjectForce));
			}
		}
	}

	public void NoteInOasis()
	{
		isInOasis = true;
	}

	public bool IsInOasis()
	{
		return isInOasis;
	}

	private bool IsHibernating()
	{
		if (region != null)
		{
			return region.Hibernated;
		}
		return false;
	}
}
