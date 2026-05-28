using System.Collections.Generic;
using UnityEngine;

public class GatherIdentifiableItems : FindConsumable
{
	public enum ItemClass
	{
		FRUIT = 0,
		VEGGIE = 1
	}

	[Tooltip("The item types we will gather.")]
	public ItemClass[] itemClasses;

	public float maxJump = 6f;

	public float pauseBetweenGathers = 10f;

	public float minGatherDist = 5f;

	private GameObject target;

	private Vector3? gatherToPos;

	private FixedJoint joint;

	private float nextLeapAvail;

	private float giveUpOnGatherTime;

	private float disallowSelectionUntil;

	private bool isAttached;

	private static HashSet<GameObject> allGathering = new HashSet<GameObject>();

	private const float GIVE_UP_GATHER_TIME = 10f;

	private const float CLOSE_ENOUGH = 1f;

	private const float CLOSE_ENOUGH_SQR = 1f;

	private const float GATHER_RAD = 3f;

	private const float GATHER_RAD_SQR = 9f;

	private List<GameObject> nearbyGameObjectsLst = new List<GameObject>();

	public override void Awake()
	{
		base.Awake();
	}

	public override float Relevancy(bool isGrounded)
	{
		Release();
		if (Time.time >= disallowSelectionUntil)
		{
			target = FindNearestConsumable(out var _);
		}
		if (!(target == null))
		{
			return Randoms.SHARED.GetInRange(0.3f, 0.5f);
		}
		return 0f;
	}

	public override void Selected()
	{
		giveUpOnGatherTime = Time.time + 10f;
	}

	public override void Action()
	{
		if (target == null || (joint == null && isAttached) || Time.time > giveUpOnGatherTime || vacuumable.isHeld())
		{
			Release();
		}
		else if (joint == null)
		{
			Vector3 gotoPos = SlimeSubbehaviour.GetGotoPos(target);
			bool flag = IsBlocked(target);
			MoveTowards(gotoPos, flag, ref nextLeapAvail, maxJump);
			if (!((gotoPos - base.transform.position).sqrMagnitude <= 1f * base.transform.localScale.z * base.transform.localScale.z))
			{
				return;
			}
			if (!flag)
			{
				Vector3? vector = (gatherToPos = GetGatherToPosition());
				if (vector.HasValue && allGathering.Add(target))
				{
					joint = SlimeUtil.AttachToMouth(base.gameObject, target);
					giveUpOnGatherTime = Time.time + 10f;
					isAttached = true;
					slimeAudio.Play(slimeAudio.slimeSounds.gatherCue);
					return;
				}
			}
			Release();
		}
		else if ((gatherToPos.Value - base.transform.position).sqrMagnitude <= 9f)
		{
			Release();
		}
		else
		{
			Rigidbody component = GetComponent<Rigidbody>();
			MoveTowards(gatherToPos.Value, shouldJump: true, ref nextLeapAvail, maxJump * (target.GetComponent<Rigidbody>().mass + component.mass) / component.mass);
			RotateTowards(gatherToPos.Value - base.transform.position);
		}
	}

	private Vector3? GetGatherToPosition()
	{
		Identifiable component = target.GetComponent<Identifiable>();
		GameObject gameObject = ((component == null) ? null : FindItemNotOfType(component.id, maxSearchRad));
		if (!(gameObject == null))
		{
			return SlimeSubbehaviour.GetGotoPos(gameObject);
		}
		return null;
	}

	private void Release()
	{
		Destroyer.Destroy(joint, "GatherIdentifiableItems.Release");
		joint = null;
		if (isAttached)
		{
			allGathering.Remove(target);
		}
		target = null;
		isAttached = false;
	}

	public override bool CanRethink()
	{
		return !isAttached;
	}

	public override void Deselected()
	{
		base.Deselected();
		Release();
		disallowSelectionUntil = Time.time + pauseBetweenGathers;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Release();
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		Dictionary<Identifiable.Id, DriveCalculator> dictionary = new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer);
		ItemClass[] array = itemClasses;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Identifiable.Id searchId in GetSearchIds(array[i]))
			{
				dictionary[searchId] = new DriveCalculator(SlimeEmotions.Emotion.NONE, 0f, 0f);
			}
		}
		return dictionary;
	}

	private static ICollection<Identifiable.Id> GetSearchIds(ItemClass itemClass)
	{
		switch (itemClass)
		{
		case ItemClass.FRUIT:
			return Identifiable.FRUIT_CLASS;
		case ItemClass.VEGGIE:
			return Identifiable.VEGGIE_CLASS;
		default:
			return new HashSet<Identifiable.Id>();
		}
	}

	protected GameObject FindItemNotOfType(Identifiable.Id ineligibleId, float maxDist)
	{
		float num = maxDist * maxDist;
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<Identifiable.Id, DriveCalculator> searchId in searchIds)
		{
			if (searchId.Key == ineligibleId)
			{
				continue;
			}
			nearbyGameObjectsLst.Clear();
			CellDirector.Get(searchId.Key, member, nearbyGameObjectsLst);
			Vector3 position = base.transform.position;
			float num2 = minGatherDist * minGatherDist;
			for (int i = 0; i < nearbyGameObjectsLst.Count; i++)
			{
				GameObject gameObject = nearbyGameObjectsLst[i];
				if (Identifiable.IsEdible(gameObject))
				{
					float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(gameObject) - position).sqrMagnitude;
					if (sqrMagnitude <= num && sqrMagnitude >= num2)
					{
						list.Add(gameObject);
					}
				}
			}
		}
		nearbyGameObjectsLst.Clear();
		return Randoms.SHARED.Pick(list, null);
	}
}
