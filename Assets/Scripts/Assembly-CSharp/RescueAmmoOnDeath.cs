using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class RescueAmmoOnDeath : SRBehaviour
{
	public Transform[] spawnNodes;

	private Queue<Identifiable.Id> rescueIds;

	private PlayerDeathHandler deathHandler;

	private LookupDirector lookupDir;

	private ProgressDirector progressDir;

	private Region region;

	private const float PCT_TO_RESCUE = 0.5f;

	private const int MAX_TO_RESCUE_PER_SLOT = 6;

	private const float RESCUE_ITEM_SPAWN_FORCE = 20f;

	private const float RESCUE_ITEM_SPAWN_TORQUE = 20f;

	public void Start()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		deathHandler = SRSingleton<SceneContext>.Instance.Player.GetComponent<PlayerDeathHandler>();
		PlayerDeathHandler playerDeathHandler = deathHandler;
		playerDeathHandler.onAmmoWillClear = (PlayerDeathHandler.OnAmmoWillClear)Delegate.Combine(playerDeathHandler.onAmmoWillClear, new PlayerDeathHandler.OnAmmoWillClear(OnAmmoWillClear));
		region = GetComponentInParent<Region>();
		rescueIds = new Queue<Identifiable.Id>();
	}

	public void OnDestroy()
	{
		if (deathHandler != null)
		{
			PlayerDeathHandler playerDeathHandler = deathHandler;
			playerDeathHandler.onAmmoWillClear = (PlayerDeathHandler.OnAmmoWillClear)Delegate.Remove(playerDeathHandler.onAmmoWillClear, new PlayerDeathHandler.OnAmmoWillClear(OnAmmoWillClear));
		}
	}

	private void OnAmmoWillClear(PlayerState.AmmoMode mode, Ammo ammo, PlayerDeathHandler.DeathType deathType)
	{
		if (!progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_DOCKS) || deathType != 0 || mode != 0)
		{
			return;
		}
		int usableSlotCount = ammo.GetUsableSlotCount();
		Identifiable.Id[] array = new Identifiable.Id[usableSlotCount];
		int[] array2 = new int[usableSlotCount];
		for (int i = 0; i < usableSlotCount; i++)
		{
			Identifiable.Id slotName = ammo.GetSlotName(i);
			if (CanRescue(slotName))
			{
				array[i] = slotName;
			}
			else
			{
				array[i] = Identifiable.Id.NONE;
			}
			array2[i] = Math.Min(6, RandomlyRound((float)ammo.GetSlotCount(i) * 0.5f));
		}
		for (int j = 0; j < spawnNodes.Length; j++)
		{
			int @int = Randoms.SHARED.GetInt(usableSlotCount);
			if (array[@int] != 0 && array2[@int] > 0)
			{
				array2[@int]--;
				if (rescueIds.Count >= spawnNodes.Length)
				{
					rescueIds.Dequeue();
				}
				rescueIds.Enqueue(array[@int]);
			}
		}
		if (base.enabled)
		{
			SpawnRescuedItems();
		}
	}

	private bool CanRescue(Identifiable.Id id)
	{
		if (!Identifiable.IsSlime(id) && !Identifiable.IsFashion(id) && id != Identifiable.Id.PUDDLE_PLORT && id != Identifiable.Id.FIRE_PLORT && id != Identifiable.Id.QUICKSILVER_PLORT && id != Identifiable.Id.WATER_LIQUID)
		{
			return id != Identifiable.Id.MAGIC_WATER_LIQUID;
		}
		return false;
	}

	private int RandomlyRound(float val)
	{
		int num = Mathf.FloorToInt(val);
		float p = val - (float)num;
		return num + (Randoms.SHARED.GetProbability(p) ? 1 : 0);
	}

	private void SpawnRescuedItems()
	{
		List<Transform> list = new List<Transform>(spawnNodes);
		while (list.Count > 0 && rescueIds.Count > 0)
		{
			Transform transform = Randoms.SHARED.Pluck(list, null);
			Identifiable.Id id = rescueIds.Dequeue();
			Rigidbody component = SRBehaviour.InstantiateActor(lookupDir.GetPrefab(id), region.setId, transform.position, transform.rotation).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.AddForce((Vector3.up + UnityEngine.Random.onUnitSphere) * 20f);
				component.AddTorque(UnityEngine.Random.onUnitSphere * 20f);
			}
		}
	}
}
