using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public abstract class GordoRewardsBase : SRBehaviour
{
	public GameObject slimePrefab;

	public GameObject slimeSpawnFXPrefab;

	private List<GameObject> activeRewards;

	private const float SPAWN_RAD = 1.2f;

	private const float SPAWN_VERT_OFFSET = 1.7f;

	private const float SPAWN_TORQUE = 10f;

	private static readonly Vector3 SPAWN_OFFSET;

	private static Vector3[] spawns;

	static GordoRewardsBase()
	{
		SPAWN_OFFSET = new Vector3(0f, 1.7f, 0f);
		spawns = new Vector3[13];
		spawns[0] = Vector3.zero;
		for (int i = 0; i < 6; i++)
		{
			float f = (float)Math.PI * 2f * (float)i / 6f;
			spawns[i + 1] = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		}
		for (int j = 0; j < 3; j++)
		{
			float f2 = (float)Math.PI * 2f * (float)j / 3f + (float)Math.PI / 6f;
			spawns[j + 7] = new Vector3(Mathf.Cos(f2) * 0.5f, 0.866f, Mathf.Sin(f2) * 0.5f);
		}
		for (int k = 0; k < 3; k++)
		{
			float f3 = (float)Math.PI * 2f * (float)k / 3f - (float)Math.PI / 6f;
			spawns[k + 10] = new Vector3(Mathf.Cos(f3) * 0.5f, -0.866f, Mathf.Sin(f3) * 0.5f);
		}
	}

	public void Start()
	{
		SetupActiveRewards();
	}

	public void SetupActiveRewards()
	{
		if (activeRewards == null)
		{
			activeRewards = new List<GameObject>(SelectActiveRewardPrefabs());
		}
	}

	public bool HasKeyReward()
	{
		if (activeRewards == null)
		{
			return false;
		}
		foreach (GameObject activeReward in activeRewards)
		{
			Identifiable component = activeReward.GetComponent<Identifiable>();
			if (component != null && component.id == Identifiable.Id.KEY)
			{
				return true;
			}
		}
		return false;
	}

	public void GiveRewards()
	{
		if (activeRewards == null)
		{
			Log.Error("Active rewards on gordo are null.", "gordo", base.name);
			return;
		}
		List<Identifiable.Id> allFashions = GetComponent<AttachFashions>().GetAllFashions();
		Identifiable component = base.gameObject.GetComponent<Identifiable>();
		Color[] colors = SlimeUtil.GetColors(base.gameObject, (component != null) ? component.id : Identifiable.Id.NONE, isGordo: true);
		Region componentInParent = GetComponentInParent<Region>();
		List<Vector3> list = new List<Vector3>(spawns.Skip(1));
		int num = 0;
		while (list.Count > 0)
		{
			GameObject original = ((num < activeRewards.Count) ? MaybeReplaceCratePrefab(activeRewards[num]) : slimePrefab);
			Vector3 vector = ((num == 0) ? spawns[0] : Randoms.SHARED.Pluck(list, Vector3.zero));
			Vector3 position = base.transform.position + vector * 1.2f + SPAWN_OFFSET;
			Quaternion rotation = ((num == 0) ? Quaternion.identity : Quaternion.LookRotation(vector, Vector3.up));
			GameObject gameObject = SRBehaviour.InstantiateActor(original, componentInParent.setId, position, rotation, nonActorOk: true);
			gameObject.GetComponent<Rigidbody>().AddTorque(Randoms.SHARED.GetInRange(-10f, 10f), Randoms.SHARED.GetInRange(-10f, 10f), Randoms.SHARED.GetInRange(-10f, 10f));
			AttachFashions component2 = gameObject.GetComponent<AttachFashions>();
			if (component2 != null)
			{
				component2.SetFashions(allFashions);
			}
			RecolorSlimeMaterial[] componentsInChildren = SRBehaviour.SpawnAndPlayFX(slimeSpawnFXPrefab, position, rotation).GetComponentsInChildren<RecolorSlimeMaterial>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetColors(colors[0], colors[1], colors[2]);
			}
			OnInstantiatedReward(gameObject);
			num++;
		}
	}

	private static GameObject MaybeReplaceCratePrefab(GameObject prefab)
	{
		if (!SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventGordos.Any() || !Identifiable.STANDARD_CRATE_CLASS.Contains(Identifiable.GetId(prefab)) || !Randoms.SHARED.GetProbability(HolidayModel.EventGordo.CRATE_CHANCE))
		{
			return prefab;
		}
		return SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(HolidayModel.EventGordo.CRATE);
	}

	protected abstract IEnumerable<GameObject> SelectActiveRewardPrefabs();

	protected virtual void OnInstantiatedReward(GameObject instance)
	{
	}
}
