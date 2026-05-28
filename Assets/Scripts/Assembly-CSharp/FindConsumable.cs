using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public abstract class FindConsumable : SlimeSubbehaviour
{
	public float minSearchRad = 5f;

	public float maxSearchRad = 30f;

	public float facingStability = 1f;

	public float facingSpeed = 5f;

	public float pursuitSpeedFactor = 1f;

	public float minDist;

	protected Dictionary<Identifiable.Id, DriveCalculator> searchIds;

	protected SlimeAudio slimeAudio;

	protected RegionMember member;

	protected LookupDirector lookupDir;

	protected float startTime;

	private const float SCOOT_CYCLE_TIME = 1f;

	private const float SCOOT_CYCLE_FACTOR = (float)Math.PI * 2f;

	private List<GameObject> nearbyGameObjects = new List<GameObject>();

	private static List<GameObjectActorModelIdentifiableIndex.Entry> localStatic_entries = new List<GameObjectActorModelIdentifiableIndex.Entry>();

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
		member = GetComponent<RegionMember>();
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		startTime = Time.time;
	}

	public override void Start()
	{
		base.Start();
		UpdateSearchIds();
	}

	public void UpdateSearchIds()
	{
		searchIds = GetSearchIds();
	}

	protected virtual Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		return GetComponent<SlimeEat>().GetAllEats();
	}

	protected void RotateTowards(Vector3 dirToTarget)
	{
		RotateTowards(dirToTarget, facingSpeed, facingStability);
	}

	protected GameObject FindNearestConsumable(IList<GameObjectActorModelIdentifiableIndex.Entry> gameObjects, out float drive)
	{
		GameObject result = null;
		float num = 1f / (maxSearchRad * maxSearchRad);
		drive = 0f;
		float num2 = minDist * minDist;
		Vector3 position = base.transform.position;
		for (int i = 0; i < gameObjects.Count; i++)
		{
			GameObjectActorModelIdentifiableIndex.Entry entry = gameObjects[i];
			GameObject gameObject = entry.GameObject;
			if (!(gameObject != base.gameObject) || !searchIds.TryGetValue(entry.Id, out var _) || !Identifiable.IsEdible(gameObject))
			{
				continue;
			}
			float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(gameObject) - position).sqrMagnitude;
			if (!(sqrMagnitude < num2))
			{
				float num3 = searchIds[entry.Id].Drive(emotions, entry.Id);
				float num4 = num3 / sqrMagnitude;
				if (num4 > num)
				{
					result = gameObject;
					num = num4;
					drive = Mathf.Clamp(num3, 0f, 1f);
				}
			}
		}
		return result;
	}

	protected GameObject FindNearestConsumable(out float drive)
	{
		localStatic_entries.Clear();
		CellDirector.Get(searchIds.Keys.AsEnumerable(), member, localStatic_entries);
		return FindNearestConsumable(localStatic_entries, out drive);
	}

	protected GameObject FindNearestConsumableOld(out float drive)
	{
		GameObject result = null;
		float num = 1f / (maxSearchRad * maxSearchRad);
		drive = 0f;
		float num2 = minDist * minDist;
		foreach (KeyValuePair<Identifiable.Id, DriveCalculator> searchId in searchIds)
		{
			nearbyGameObjects.Clear();
			CellDirector.Get(searchId.Key, member, nearbyGameObjects);
			Vector3 position = base.transform.position;
			for (int i = 0; i < nearbyGameObjects.Count; i++)
			{
				GameObject gameObject = nearbyGameObjects[i];
				if (!(gameObject != base.gameObject) || !Identifiable.IsEdible(gameObject))
				{
					continue;
				}
				float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(gameObject) - position).sqrMagnitude;
				if (!(sqrMagnitude < num2))
				{
					float num3 = searchId.Value.Drive(emotions, searchId.Key);
					float num4 = num3 / sqrMagnitude;
					if (num4 > num)
					{
						result = gameObject;
						num = num4;
						drive = Mathf.Clamp(num3, 0f, 1f);
					}
				}
			}
		}
		nearbyGameObjects.Clear();
		return result;
	}

	protected void MoveTowards(Vector3 targetPos, bool shouldJump, ref float nextJumpAvail, float jumpStrength)
	{
		if (!IsGrounded())
		{
			return;
		}
		Vector3 vector = targetPos - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		Vector3 normalized = vector.normalized;
		RotateTowards(normalized);
		if (shouldJump)
		{
			if (Time.fixedTime >= nextJumpAvail)
			{
				float num = Mathf.Min(1f, Mathf.Sqrt(sqrMagnitude) / 30f);
				slimeBody.AddForce((normalized * num + Vector3.up).normalized * jumpStrength * slimeBody.mass, ForceMode.Impulse);
				slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
				slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
				nextJumpAvail = Time.time + 1f;
			}
		}
		else if (sqrMagnitude <= 9f)
		{
			slimeBody.AddForce(normalized * (480f * pursuitSpeedFactor * slimeBody.mass * Time.fixedDeltaTime));
		}
		else
		{
			float num2 = ScootCycleSpeed();
			slimeBody.AddForce(normalized * (150f * slimeBody.mass * pursuitSpeedFactor * Time.fixedDeltaTime * num2));
			Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
			slimeBody.AddForceAtPosition(normalized * (270f * slimeBody.mass * Time.fixedDeltaTime * num2), position);
		}
	}

	protected float ScootCycleSpeed()
	{
		return Mathf.Sin((Time.time - startTime) * ((float)Math.PI * 2f)) + 1f;
	}
}
