using System;
using System.Collections.Generic;
using UnityEngine;

public class GardenCatcher : SRBehaviour
{
	[Serializable]
	public class PlantSlot
	{
		public Identifiable.Id id;

		public GameObject plantedPrefab;

		public GameObject deluxePlantedPrefab;
	}

	public PlantSlot[] plantable;

	public LandPlot activator;

	public GameObject acceptFX;

	public SECTR_AudioCue treeScaleUpCue;

	public SECTR_AudioCue treeScaleDownCue;

	private Dictionary<Identifiable.Id, GameObject> plantableDict = new Dictionary<Identifiable.Id, GameObject>(Identifiable.idComparer);

	private Dictionary<Identifiable.Id, GameObject> deluxeDict = new Dictionary<Identifiable.Id, GameObject>(Identifiable.idComparer);

	private TutorialDirector tutDir;

	public void Awake()
	{
		PlantSlot[] array = plantable;
		foreach (PlantSlot plantSlot in array)
		{
			plantableDict[plantSlot.id] = plantSlot.plantedPrefab;
			deluxeDict[plantSlot.id] = plantSlot.deluxePlantedPrefab;
		}
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable.Id id = Identifiable.GetId(col.gameObject);
		if (CanAccept(id))
		{
			Plant(id, isReplacement: false);
			tutDir.OnPlanted();
			if (acceptFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(acceptFX, base.transform.position, base.transform.rotation);
			}
			Destroyer.DestroyActor(col.gameObject, "GardenCatcher.OnTriggerEnter");
		}
	}

	public GameObject Plant(Identifiable.Id cropId, bool isReplacement)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(activator.HasUpgrade(LandPlot.Upgrade.DELUXE_GARDEN) ? deluxeDict[cropId] : plantableDict[cropId], activator.transform.position, activator.transform.rotation);
		if (Identifiable.FRUIT_CLASS.Contains(cropId))
		{
			activator.Attach(gameObject, immediate: false, isReplacement, treeScaleUpCue);
		}
		else
		{
			activator.Attach(gameObject, immediate: false, isReplacement);
		}
		return gameObject;
	}

	public bool CanAccept(Identifiable.Id id)
	{
		if (activator.HasAttached())
		{
			return false;
		}
		return plantableDict.ContainsKey(id);
	}
}
