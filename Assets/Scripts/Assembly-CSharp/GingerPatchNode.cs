using System.Collections.Generic;
using UnityEngine;

public class GingerPatchNode : IdHandler
{
	public GameObject bed;

	public FixedJoint spawnJoint;

	public GameObject gingerPrefab;

	public GameObject pulledFx;

	public GameObject disappearFx;

	public static List<GingerPatchNode> allGingerPatches = new List<GingerPatchNode>();

	private Transform jointTransform;

	private ZoneDirector zoneDirector;

	public void Awake()
	{
		jointTransform = spawnJoint.transform;
		allGingerPatches.Add(this);
		zoneDirector = GetComponentInParent<ZoneDirector>();
		zoneDirector.Register(this);
	}

	public void OnDestroy()
	{
		allGingerPatches.Remove(this);
	}

	public void Start()
	{
		if (spawnJoint.connectedBody == null)
		{
			HidePatch();
		}
	}

	public void Grow()
	{
		Grow(SRBehaviour.InstantiateActor(gingerPrefab, zoneDirector.regionSetId, spawnJoint.transform.position, spawnJoint.transform.rotation));
	}

	public void Grow(GameObject ginger)
	{
		bed.SetActive(value: true);
		spawnJoint.gameObject.SetActive(value: true);
		if (spawnJoint.connectedBody != null)
		{
			Destroyer.DestroyActor(spawnJoint.connectedBody.gameObject, "GingerPatchNode.Grow");
		}
		ginger.GetComponent<ResourceCycle>().Attach(spawnJoint, null, Harvested);
	}

	public void Harvested()
	{
		HidePatch();
		if (spawnJoint.connectedBody != null)
		{
			Destroyer.DestroyActor(spawnJoint.connectedBody.gameObject, "GingerPatchNode.Harvested");
			spawnJoint.connectedBody = null;
		}
		SRBehaviour.SpawnAndPlayFX(pulledFx, jointTransform.position, jointTransform.rotation);
	}

	public void HidePatchAndReset()
	{
		HidePatch();
		if (spawnJoint.connectedBody != null)
		{
			Destroyer.DestroyActor(spawnJoint.connectedBody.gameObject, "GingerPatchNode.Reset");
			spawnJoint.connectedBody = null;
			SRBehaviour.SpawnAndPlayFX(disappearFx, jointTransform.position, jointTransform.rotation);
		}
	}

	private void HidePatch()
	{
		bed.SetActive(value: false);
		spawnJoint.gameObject.SetActive(value: false);
	}

	protected override string IdPrefix()
	{
		return "gingerPatch";
	}
}
