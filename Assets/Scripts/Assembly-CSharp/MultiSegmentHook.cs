using UnityEngine;

public class MultiSegmentHook : SRBehaviour, Attachment
{
	public TentacleHook segment1;

	public TentacleHook segment2;

	public GameObject bend;

	private Rigidbody bendBody;

	public void Init(GameObject source, GameObject target, Vector3 attachPoint, bool causeFear, float intermediateHeight)
	{
		bend.transform.position = source.transform.position + Vector3.up * intermediateHeight;
		bendBody = bend.GetComponent<Rigidbody>();
		segment1.Init(source, bend, Vector3.zero, causeFear, 0f);
		segment2.Init(bend, target, attachPoint, causeFear, 0f);
		segment1.SetPauseRetract(pauseRetract: true);
	}

	public void OnDestroy()
	{
		Destroyer.Destroy(bend, "MultiSegmentHook.OnDestroy");
	}

	public void Update()
	{
		if (bendBody != null && bendBody.isKinematic && (segment2 == null || segment2.parentJoint == null || segment2.parentJoint.maxDistance <= 0f))
		{
			SegmentOneRetract();
		}
		if (segment1 == null || segment1.parentJoint == null || segment1.parentJoint.maxDistance <= 0f)
		{
			Destroyer.Destroy(base.gameObject, "MultiSegmentHook.Update");
		}
	}

	public void SegmentOneRetract()
	{
		bendBody.isKinematic = false;
		segment1.SetPauseRetract(pauseRetract: false);
	}
}
