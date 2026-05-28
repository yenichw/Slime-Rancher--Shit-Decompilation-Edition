using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class LemonPhasor : SRBehaviour
{
	private SpawnResource spawnResource;

	private Region region;

	public GameObject lemonPrefab;

	public GameObject spawnLemonFx;

	public GameObject phaseoutFruitFx;

	private HashSet<GameObject> handledFruit = new HashSet<GameObject>();

	public void Awake()
	{
		region = GetComponentInParent<Region>();
		spawnResource = GetComponent<SpawnResource>();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable component = col.gameObject.GetComponent<Identifiable>();
		if (component == null || !Identifiable.FRUIT_CLASS.Contains(component.id) || component.id == Identifiable.Id.LEMON_FRUIT || handledFruit.Contains(col.gameObject))
		{
			return;
		}
		Joint joint = spawnResource.PickRipeResourceJoint();
		if (!(joint == null))
		{
			handledFruit.Add(col.gameObject);
			((ProduceModel)SRSingleton<SceneContext>.Instance.GameModel.GetActorModel(component.GetActorId())).state = ResourceCycle.State.ROTTEN;
			Destroyer.DestroyActor(joint.connectedBody.gameObject, "LemonPhasor.OnTriggerEnter#1");
			GameObject gameObject = SRBehaviour.InstantiateActor(lemonPrefab, region.setId, joint.transform.position, joint.transform.rotation);
			ProduceModel obj = (ProduceModel)SRSingleton<SceneContext>.Instance.GameModel.GetActorModel(component.GetActorId());
			ResourceCycle component2 = gameObject.GetComponent<ResourceCycle>();
			obj.state = ResourceCycle.State.EDIBLE;
			component2.Eject(gameObject.GetComponent<Rigidbody>());
			if (spawnLemonFx != null)
			{
				SRBehaviour.SpawnAndPlayFX(spawnLemonFx, joint.transform.position, joint.transform.rotation);
			}
			if (phaseoutFruitFx != null)
			{
				SRBehaviour.SpawnAndPlayFX(phaseoutFruitFx, col.transform.position, col.transform.rotation);
			}
			Destroyer.DestroyActor(col.gameObject, "LemonPhasor.OnTriggerEnter#2");
		}
	}

	public void LateUpdate()
	{
		handledFruit.Clear();
	}
}
