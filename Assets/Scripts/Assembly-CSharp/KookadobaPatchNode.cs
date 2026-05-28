using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class KookadobaPatchNode : SRBehaviour, KookadobaNodeModel.Participant
{
	public GameObject bed;

	public FixedJoint spawnJoint;

	public GameObject kookadobaPrefab;

	public GameObject pulledFx;

	public GameObject disappearFx;

	private Transform jointTransform;

	private ZoneDirector zoneDirector;

	public void Awake()
	{
		jointTransform = spawnJoint.transform;
		zoneDirector = GetComponentInParent<ZoneDirector>();
		zoneDirector.Register(this);
		SRSingleton<SceneContext>.Instance.GameModel.RegisterKookadobaNode(this);
	}

	public void Start()
	{
		if (spawnJoint.connectedBody == null)
		{
			HidePatch();
		}
	}

	public void InitModel(KookadobaNodeModel model)
	{
		model.pos = base.transform.position;
	}

	public void SetModel(KookadobaNodeModel model)
	{
	}

	public void Grow()
	{
		if (spawnJoint.connectedBody != null)
		{
			ResourceCycle component = spawnJoint.connectedBody.GetComponent<ResourceCycle>();
			if (component != null)
			{
				component.UpdateToNow();
			}
			if (spawnJoint.connectedBody != null)
			{
				return;
			}
		}
		Grow(SRBehaviour.InstantiateActor(kookadobaPrefab, zoneDirector.regionSetId, spawnJoint.transform.position, spawnJoint.transform.rotation));
	}

	public void Grow(GameObject kookadoba)
	{
		bed.SetActive(value: true);
		spawnJoint.gameObject.SetActive(value: true);
		if (spawnJoint.connectedBody != null)
		{
			Destroyer.DestroyActor(spawnJoint.connectedBody.gameObject, "KookadobaPatchNode.Grow");
		}
		kookadoba.GetComponent<ResourceCycle>().Attach(spawnJoint, null, Harvested);
	}

	public void Harvested()
	{
		HidePatch();
		if (spawnJoint.connectedBody != null)
		{
			Destroyer.Destroy(spawnJoint.connectedBody.gameObject, "KookadobaPatchNode.Harvested");
			spawnJoint.connectedBody = null;
		}
		if (base.gameObject.activeInHierarchy)
		{
			SRBehaviour.SpawnAndPlayFX(pulledFx, jointTransform.position, jointTransform.rotation);
		}
	}

	private void HidePatch()
	{
		bed.SetActive(value: false);
		spawnJoint.gameObject.SetActive(value: false);
	}
}
