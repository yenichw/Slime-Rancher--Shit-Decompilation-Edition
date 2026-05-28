using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EchoNet : SRBehaviour, GadgetModel.Participant
{
	public float minSpawnsPerHour = 0.2f;

	public float maxSpawnsPerHour = 0.3333f;

	public Transform[] spawnNodes;

	public GameObject activeVersion;

	public GameObject inactiveVersion;

	private CellDirector cellDir;

	private ZoneDirector zoneDir;

	private LookupDirector lookupDir;

	private TimeDirector timeDir;

	private EchoNetModel model;

	private const float PRESENT_DIST = 0.001f;

	private const float SQR_PRESENT_DIST = 1.0000001E-06f;

	public void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void Start()
	{
		zoneDir = GetComponentInParent<ZoneDirector>();
		cellDir = GetComponentInParent<CellDirector>();
		MaybeSpawnEchoes();
	}

	public void InitModel(GadgetModel model)
	{
		ResetSpawnTime((EchoNetModel)model);
	}

	public void SetModel(GadgetModel model)
	{
		this.model = (EchoNetModel)model;
		if (zoneDir != null)
		{
			MaybeSpawnEchoes();
		}
	}

	public void OnEnable()
	{
		if (!(zoneDir == null))
		{
			MaybeSpawnEchoes();
		}
	}

	private void MaybeSpawnEchoes()
	{
		bool flag = zoneDir.GetAllAuxItems().Count > 0;
		activeVersion.SetActive(flag);
		inactiveVersion.SetActive(!flag);
		double num = timeDir.WorldTime() - model.lastSpawnTime;
		ResetSpawnTime(model);
		float num2 = (int)Math.Round((double)Randoms.SHARED.GetInRange(minSpawnsPerHour, maxSpawnsPerHour) * num * 0.00027777778450399637);
		ICollection<Identifiable.Id> allAuxItems = zoneDir.GetAllAuxItems();
		if (!(num2 > 0f) || allAuxItems.Count <= 0)
		{
			return;
		}
		List<GameObject> result = new List<GameObject>();
		foreach (Identifiable.Id item in allAuxItems)
		{
			cellDir.Get(item, ref result);
		}
		List<Transform> list = new List<Transform>(spawnNodes);
		Transform[] array = spawnNodes;
		foreach (Transform transform in array)
		{
			foreach (GameObject item2 in result)
			{
				if ((item2.transform.position - transform.position).sqrMagnitude < 1.0000001E-06f)
				{
					list.Remove(transform);
					break;
				}
			}
		}
		for (int j = 0; (float)j < num2; j++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			Transform node = Randoms.SHARED.Pluck(list, null);
			SpawnAt(node);
		}
	}

	private void SpawnAt(Transform node)
	{
		Identifiable.Id id = zoneDir.PickAuxItem();
		SRBehaviour.InstantiateActor(lookupDir.GetPrefab(id), zoneDir.regionSetId, node.position, node.rotation);
	}

	private void ResetSpawnTime(EchoNetModel model)
	{
		model.lastSpawnTime = timeDir.WorldTime();
	}
}
