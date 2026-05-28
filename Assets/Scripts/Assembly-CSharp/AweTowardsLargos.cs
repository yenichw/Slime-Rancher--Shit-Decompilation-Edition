using System;
using System.Collections.Generic;
using UnityEngine;

public class AweTowardsLargos : FindConsumable
{
	private GameObject target;

	private static DriveCalculator largoDriveCalc = new DriveCalculator(SlimeEmotions.Emotion.NONE, 0f, 0f);

	private TimeDirector timeDir;

	private SlimeFaceAnimator sfAnimator;

	private double nextActivationTime;

	private float endTime;

	private static List<GameObjectActorModelIdentifiableIndex.Entry> localStaticLargoEntries = new List<GameObjectActorModelIdentifiableIndex.Entry>();

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		sfAnimator = GetComponent<SlimeFaceAnimator>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!isGrounded || !timeDir.HasReached(nextActivationTime))
		{
			return 0f;
		}
		localStaticLargoEntries.Clear();
		CellDirector.GetLargosNearMember(member, localStaticLargoEntries);
		target = FindNearestConsumable(localStaticLargoEntries, out var _);
		if (!(target == null))
		{
			return Randoms.SHARED.GetInRange(0.1f, 1f);
		}
		return 0f;
	}

	public override void Action()
	{
		if (target != null)
		{
			RotateTowards(SlimeSubbehaviour.GetGotoPos(target) - base.transform.position);
		}
	}

	public override void Selected()
	{
		sfAnimator.SetTrigger("triggerLongAwe");
		nextActivationTime = timeDir.HoursFromNow(1f);
		endTime = Time.time + 3f;
	}

	public override bool CanRethink()
	{
		return Time.time >= endTime;
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		SlimeVarietyModules component = GetComponent<SlimeVarietyModules>();
		Dictionary<Identifiable.Id, DriveCalculator> dictionary = new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer);
		foreach (Identifiable.Id item in Identifiable.LARGO_CLASS)
		{
			GameObject prefab = lookupDir.GetPrefab(item);
			if (prefab == null)
			{
				Log.Error("Null prefab!", "id", item);
			}
			SlimeVarietyModules component2 = prefab.GetComponent<SlimeVarietyModules>();
			if (!(component2 != null))
			{
				continue;
			}
			bool flag = false;
			GameObject[] slimeModules = component.slimeModules;
			foreach (GameObject value in slimeModules)
			{
				if (Array.IndexOf(component2.slimeModules, value) != -1)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dictionary[item] = largoDriveCalc;
			}
		}
		return dictionary;
	}
}
