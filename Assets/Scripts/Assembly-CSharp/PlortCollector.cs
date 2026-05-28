using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PlortCollector : SRBehaviour, LandPlotModel.Participant
{
	private class JointReference
	{
		public Identifiable.Id id;

		public Vacuumable vacuumable;

		public SpringJoint joint;

		public void Destroy()
		{
			Destroyer.Destroy(joint, "PlortCollector.JointReference.Destroy");
			if (vacuumable != null)
			{
				vacuumable.release();
				vacuumable = null;
			}
		}
	}

	[Tooltip("The area within which we collect plorts.")]
	public TrackCollisions collectionArea;

	[Tooltip("Time between collections in hours.")]
	public float collectPeriod = 1f;

	[Tooltip("Animator to animate while collecting any plorts.")]
	public Animator collectAnim;

	[Tooltip("Effect to play on collecting an individual plort.")]
	public GameObject collectFX;

	[Tooltip("Where to pull the plorts to")]
	public Transform collectPt;

	private SiloStorage storage;

	private SECTR_AudioSource vacAudio;

	private TimeDirector timeDir;

	private Region region;

	private List<JointReference> joints = new List<JointReference>();

	private double endCollectAt;

	private double forceCollectUntil;

	private const float COLLECT_DIST = 1f;

	private const float COLLECT_DIST_SQR = 1f;

	private const float COLLECT_SPEED = 5f;

	private const float MIN_COLLECT_TIME = 1f / 12f;

	private const float MAX_COLLECT_TIME = 1f / 6f;

	private int animCycloneActiveId;

	private LandPlotModel model;

	public void Awake()
	{
		region = GetComponentInParent<Region>();
		storage = GetComponentInParent<SiloStorage>();
		vacAudio = GetComponent<SECTR_AudioSource>();
		collectAnim = GetComponentInChildren<Animator>();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		animCycloneActiveId = Animator.StringToHash("CycloneActive");
	}

	public void InitModel(LandPlotModel model)
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		model.collectorNextTime = timeDir.HoursFromNowOrStart(collectPeriod);
	}

	public void SetModel(LandPlotModel model)
	{
		this.model = model;
	}

	public void Update()
	{
		if (region.Hibernated)
		{
			return;
		}
		if (joints.Count > 0 && timeDir.HasReached(endCollectAt))
		{
			foreach (JointReference joint in joints)
			{
				joint.Destroy();
			}
			joints.Clear();
		}
		else if (timeDir.HasReached(model.collectorNextTime))
		{
			DoCollection();
		}
		if (joints.Count > 0)
		{
			List<JointReference> list = new List<JointReference>();
			foreach (JointReference joint2 in joints)
			{
				if (joint2.joint == null || joint2.joint.connectedBody == null)
				{
					list.Add(joint2);
				}
				else if (!storage.CanAccept(joint2.id))
				{
					list.Add(joint2);
				}
				else if ((joint2.joint.connectedBody.transform.position - collectPt.position).sqrMagnitude <= 1f)
				{
					if (storage.MaybeAddIdentifiable(joint2.id))
					{
						if (collectFX != null)
						{
							SRBehaviour.SpawnAndPlayFX(collectFX, joint2.joint.connectedBody.transform.position, joint2.joint.connectedBody.transform.rotation);
						}
						Destroyer.DestroyActor(joint2.joint.connectedBody.gameObject, "PlortCollector.Update");
					}
					list.Add(joint2);
				}
				else
				{
					joint2.joint.maxDistance = Mathf.Max(0f, joint2.joint.maxDistance - Time.deltaTime * 5f);
				}
			}
			foreach (JointReference item in list)
			{
				joints.Remove(item);
				item.Destroy();
			}
		}
		bool flag = joints.Count > 0 || !timeDir.HasReached(forceCollectUntil);
		if (collectAnim != null)
		{
			collectAnim.SetBool(animCycloneActiveId, flag);
		}
		if (flag && !vacAudio.IsPlaying)
		{
			vacAudio.Play();
		}
		else if (!flag && vacAudio.IsPlaying)
		{
			vacAudio.Stop(stopImmediately: false);
		}
	}

	public void StartCollection()
	{
		if (joints.Count == 0 && timeDir.HasReached(forceCollectUntil))
		{
			DoCollection();
		}
	}

	private void DoCollection()
	{
		model.collectorNextTime += 3600f * collectPeriod;
		foreach (GameObject item in collectionArea.CurrColliders())
		{
			Identifiable component = item.GetComponent<Identifiable>();
			if (component != null && storage.CanAccept(component.id))
			{
				Vacuumable component2 = item.GetComponent<Vacuumable>();
				if (component2 != null && !component2.isCaptive())
				{
					GameObject obj = new GameObject("CollectJoint");
					obj.AddComponent<Rigidbody>().isKinematic = true;
					obj.transform.SetParent(collectPt, worldPositionStays: false);
					obj.transform.localPosition = Vector3.zero;
					SpringJoint springJoint = obj.AddComponent<SpringJoint>();
					springJoint.spring = 1000f;
					springJoint.maxDistance = (item.transform.position - collectPt.position).magnitude;
					springJoint.autoConfigureConnectedAnchor = false;
					springJoint.connectedAnchor = Vector3.zero;
					SafeJointReference.AttachSafely(item, springJoint);
					springJoint.connectedBody.WakeUp();
					component2.capture(springJoint);
					joints.Add(new JointReference
					{
						vacuumable = component2,
						joint = springJoint,
						id = component.id
					});
				}
			}
		}
		forceCollectUntil = timeDir.HoursFromNow(1f / 12f);
		endCollectAt = timeDir.HoursFromNow(1f / 6f);
	}

	public void FastForward(List<Identifiable.Id> produceIds, List<Identifiable.Id> alreadyCollectedIds)
	{
		for (int num = produceIds.Count - 1; num >= 0; num--)
		{
			if (storage.MaybeAddIdentifiable(produceIds[num]))
			{
				alreadyCollectedIds.Add(produceIds[num]);
			}
		}
	}
}
