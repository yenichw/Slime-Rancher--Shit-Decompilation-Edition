using System.Collections.Generic;
using UnityEngine;

public class DirectedAuxItemSpawner : SRBehaviour
{
	private CellDirector cellDir;

	private ZoneDirector zoneDirector;

	private const float PRESENT_DIST = 0.001f;

	private const float SQR_PRESENT_DIST = 1.0000001E-06f;

	public void Start()
	{
		cellDir = GetComponentInParent<CellDirector>();
		zoneDirector = GetComponentInParent<ZoneDirector>();
		zoneDirector.Register(this);
	}

	public void Spawn(GameObject prefab)
	{
		SRBehaviour.InstantiateActor(prefab, zoneDirector.regionSetId, base.transform.position, base.transform.rotation);
	}

	public void UnspawnIfPresent(IEnumerable<Identifiable.Id> ids)
	{
		List<GameObject> result = new List<GameObject>();
		foreach (Identifiable.Id id in ids)
		{
			cellDir.Get(id, ref result);
		}
		foreach (GameObject item in result)
		{
			if ((item.transform.position - base.transform.position).sqrMagnitude < 1.0000001E-06f)
			{
				Destroyer.DestroyActor(item, "DirectedAuxItemSpawner.UnspawnIfPresent");
			}
		}
	}
}
