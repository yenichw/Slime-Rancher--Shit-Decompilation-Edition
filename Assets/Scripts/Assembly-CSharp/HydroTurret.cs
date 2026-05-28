using System;
using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class HydroTurret : SRBehaviour
{
	private enum ShootResult
	{
		PENDING = 0,
		SHOT = 1,
		FAIL = 2
	}

	public Identifiable.Id liquidId = Identifiable.Id.WATER_LIQUID;

	public Transform spawnLoc;

	public FilteredTrackCollisions tracker;

	public Transform toRotate;

	public SECTR_AudioCue shootCue;

	public SECTR_AudioCue rotateCue;

	[Tooltip("Delay in game mins between shots")]
	public float shootDelay = 2f;

	[Tooltip("Delay in game mins before we can shoot if we need to retarget.")]
	public float retargetDelay = 0.5f;

	[Tooltip("Delay in game mins before we can shoot if another turret on our gadget shot.")]
	public float localShotDelay = 0.2f;

	private TimeDirector timeDir;

	private Region region;

	private GameObject liquidPrefab;

	private double nextShootTime;

	private GameObject currTarget;

	private WaitForChargeup waiter;

	private List<HydroTurret> otherTurrets = new List<HydroTurret>();

	private const float SHOOT_SCALE_UP_TIME = 0.1f;

	private const float SHOOT_SCALE_FACTOR = 0.2f;

	private const float MAX_ROT_PER_SEC = 180f;

	private const float MAX_WALL_CHECK_DIST = 5f;

	public void Awake()
	{
		region = GetComponentInParent<Region>();
		waiter = GetComponent<WaitForChargeup>();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		tracker.SetFilter(IsTarr);
		liquidPrefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(liquidId);
		HydroTurret[] components = GetComponents<HydroTurret>();
		foreach (HydroTurret hydroTurret in components)
		{
			if (hydroTurret != this)
			{
				otherTurrets.Add(hydroTurret);
			}
		}
	}

	private void DelayForLocalShot()
	{
		nextShootTime = Math.Max(nextShootTime, timeDir.HoursFromNow(localShotDelay * (1f / 60f)));
	}

	private bool IsTarr(GameObject gameObj)
	{
		return Identifiable.IsTarr(Identifiable.GetId(gameObj));
	}

	public void Update()
	{
		if (waiter.IsWaiting() || !timeDir.HasReached(nextShootTime))
		{
			return;
		}
		HashSet<GameObject> hashSet = tracker.CurrColliders();
		if (hashSet.Count > 0)
		{
			if (currTarget == null || !hashSet.Contains(currTarget))
			{
				currTarget = Randoms.SHARED.Pick(hashSet, null);
			}
			switch (TryToShootAt(currTarget))
			{
			case ShootResult.SHOT:
				currTarget = null;
				nextShootTime = timeDir.HoursFromNow(shootDelay * (1f / 60f));
				{
					foreach (HydroTurret otherTurret in otherTurrets)
					{
						otherTurret.DelayForLocalShot();
					}
					break;
				}
			case ShootResult.FAIL:
				nextShootTime = timeDir.HoursFromNow(retargetDelay * (1f / 60f));
				break;
			}
		}
		else
		{
			nextShootTime = timeDir.HoursFromNow(retargetDelay * (1f / 60f));
		}
	}

	private ShootResult TryToShootAt(GameObject target)
	{
		Vector3 vector = target.transform.position - toRotate.position;
		float num = 25f;
		float num2 = Mathf.Abs(Physics.gravity.y);
		Vector3 vector2 = vector;
		vector2.y = 0f;
		float magnitude = vector2.magnitude;
		float y = vector.y;
		float num3 = num * num;
		float num4 = num3 * num3 - num2 * (num2 * magnitude * magnitude + 2f * y * num3);
		if (num4 < 0f)
		{
			return ShootResult.FAIL;
		}
		float y2 = num * num - Mathf.Sqrt(num4);
		float num5 = num2 * magnitude;
		vector2.Normalize();
		Vector3 vector3 = new Vector3(vector2.x * num5, y2, vector2.z * num5).normalized * num;
		if (WouldHitWall(vector3, Mathf.Min(vector.magnitude, 5f)))
		{
			return ShootResult.FAIL;
		}
		if (!RotateTowards(vector3))
		{
			return ShootResult.PENDING;
		}
		GameObject obj = SRBehaviour.InstantiateActor(liquidPrefab, region.setId, spawnLoc.position, spawnLoc.rotation);
		obj.GetComponent<Rigidbody>().velocity = vector3;
		float x = obj.transform.localScale.x;
		float fromValue = x * 0.2f;
		obj.transform.DOScale(x, 0.1f).From(fromValue).SetEase(Ease.Linear);
		if (shootCue != null)
		{
			SECTR_AudioSystem.Play(shootCue, spawnLoc.position, loop: false);
		}
		return ShootResult.SHOT;
	}

	private bool RotateTowards(Vector3 dir)
	{
		Quaternion quaternion = Quaternion.LookRotation(dir, Vector3.up);
		toRotate.rotation = Quaternion.RotateTowards(toRotate.rotation, quaternion, 180f * Time.deltaTime);
		return Quaternion.Angle(toRotate.rotation, quaternion) < Mathf.Epsilon;
	}

	private bool WouldHitWall(Vector3 dir, float maxDist)
	{
		LayerMask layerMask = 268435457;
		return Physics.Raycast(toRotate.position, dir, maxDist, layerMask, QueryTriggerInteraction.Ignore);
	}
}
