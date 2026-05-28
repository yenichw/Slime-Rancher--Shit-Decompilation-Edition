using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeKey : SRBehaviour
{
	public static List<SlimeKey> allKeys = new List<SlimeKey>();

	public GameObject pickupFX;

	private RegionMember regionMember;

	public void Awake()
	{
		allKeys.Add(this);
		regionMember = GetComponent<RegionMember>();
	}

	public bool IsKeyInZone(ZoneDirector.Zone zoneId)
	{
		return regionMember.IsInZone(zoneId);
	}

	public void OnDestroy()
	{
		allKeys.Remove(this);
		regionMember = null;
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == SRSingleton<SceneContext>.Instance.Player)
		{
			SRSingleton<SceneContext>.Instance.PlayerState.AddKey();
			if (pickupFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(pickupFX, base.transform.position, base.transform.rotation);
			}
			Destroyer.DestroyActor(base.gameObject, "SlimeKey.OnTriggerEnter");
		}
	}
}
