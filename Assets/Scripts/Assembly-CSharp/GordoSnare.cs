using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GordoSnare : SRBehaviour, GadgetModel.Participant
{
	public static List<GordoSnare> AllGordoSnares = new List<GordoSnare>();

	private bool isSnared;

	public GameObject baitPosition;

	public GameObject bait;

	public GameObject baitAttachedFx;

	public int pinkSnareWeight;

	public int foodTypeSnareWeight;

	public int favoredFoodSnareWeight;

	private SnareModel model;

	public void Awake()
	{
		AllGordoSnares.Add(this);
	}

	public void OnDestroy()
	{
		AllGordoSnares.Remove(this);
	}

	public void InitModel(GadgetModel model)
	{
	}

	public void SetModel(GadgetModel model)
	{
		this.model = (SnareModel)model;
		if (this.model.baitTypeId != 0)
		{
			AttachBait(this.model.baitTypeId);
		}
		else if (this.model.gordoTypeId != 0)
		{
			SnareGordo(this.model.gordoTypeId);
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger || !(bait == null) || isSnared)
		{
			return;
		}
		Identifiable component = col.GetComponent<Identifiable>();
		if (component != null && Identifiable.IsFood(component.id))
		{
			if (baitAttachedFx != null)
			{
				SRBehaviour.SpawnAndPlayFX(baitAttachedFx, base.gameObject);
			}
			Destroyer.DestroyActor(col.gameObject, "GordoSnare.OnTriggerEnter");
			AttachBait(component.id);
		}
	}

	public bool HasSnaredGordo()
	{
		return isSnared;
	}

	public bool IsBaited()
	{
		return model.baitTypeId != Identifiable.Id.NONE;
	}

	public bool SnareGordo()
	{
		if (!IsBaited() || HasSnaredGordo())
		{
			return false;
		}
		Identifiable.Id gordoIdForBait = GetGordoIdForBait();
		SnareGordo(gordoIdForBait);
		if (gordoIdForBait == Identifiable.Id.HUNTER_GORDO)
		{
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.SNARED_HUNTER_GORDOS, 1);
		}
		return true;
	}

	private void SnareGordo(Identifiable.Id id)
	{
		model.gordoTypeId = id;
		GameObject obj = Object.Instantiate(SRSingleton<GameContext>.Instance.LookupDirector.GetGordo(id), base.gameObject.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		GadgetModel.Participant[] components = obj.GetComponents<GadgetModel.Participant>();
		GadgetModel.Participant[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].InitModel(model);
		}
		array = components;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetModel(model);
		}
		isSnared = true;
		ClearBait();
	}

	private Identifiable.Id GetGordoIdForBait()
	{
		Dictionary<Identifiable.Id, float> dictionary = new Dictionary<Identifiable.Id, float>(Identifiable.idComparer);
		Identifiable.Id id = Identifiable.Id.NONE;
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		foreach (GameObject gordoEntry in SRSingleton<GameContext>.Instance.LookupDirector.GordoEntries)
		{
			GordoEat component = gordoEntry.GetComponent<GordoEat>();
			GordoIdentifiable component2 = component.GetComponent<GordoIdentifiable>();
			if (component2.id == Identifiable.Id.PINK_GORDO)
			{
				continue;
			}
			SlimeDiet diet = component.slimeDefinition.Diet;
			List<SlimeDiet.EatMapEntry> list2 = new List<SlimeDiet.EatMapEntry>();
			diet.AddEatMapEntries(model.baitTypeId, list2);
			SlimeDiet.EatMapEntry eatMapEntry = ((list2.Count > 0) ? list2[0] : null);
			bool flag = false;
			for (int i = 0; i < component2.nativeZones.Length; i++)
			{
				if (ZoneDirector.HasAccessToZone(component2.nativeZones[i]))
				{
					flag = true;
				}
			}
			if (flag && eatMapEntry != null)
			{
				if (eatMapEntry.isFavorite)
				{
					Log.Debug("Found favorite", "gordo", component2.id, "hasAccess", flag);
					id = component2.id;
				}
				else
				{
					Log.Debug("Adding potential", "gordo", component2.id, "hasAccess", flag);
					list.Add(component2.id);
				}
			}
		}
		if (list.Count > 0)
		{
			float value = foodTypeSnareWeight / list.Count;
			for (int j = 0; j < list.Count; j++)
			{
				dictionary.Add(list[j], value);
			}
		}
		if (id != 0)
		{
			dictionary.Add(id, favoredFoodSnareWeight);
		}
		dictionary.Add(Identifiable.Id.PINK_GORDO, pinkSnareWeight);
		return Randoms.SHARED.Pick(dictionary, Identifiable.Id.PINK_GORDO);
	}

	private void AttachBait(Identifiable.Id id)
	{
		ClearBait();
		model.baitTypeId = id;
		GameObject prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(id);
		bait = Object.Instantiate(prefab, base.transform);
		bait.transform.position = baitPosition.transform.position;
		bait.transform.rotation = Quaternion.identity;
		RemoveComponents<Collider>(bait);
		RemoveComponent<DragFloatReactor>(bait);
		RemoveComponent<Rigidbody>(bait);
		RemoveComponent<KeepUpright>(bait);
		RemoveComponent<DontGoThroughThings>(bait);
		RemoveComponent<SECTR_PointSource>(bait);
		RemoveComponent<RegionMember>(bait);
		RemoveComponent<ChickenRandomMove>(bait);
		RemoveComponent<ChickenVampirism>(bait);
		RemoveComponent<PlaySoundOnHit>(bait);
		RemoveComponent<ResourceCycle>(bait);
		RemoveComponent<Reproduce>(bait);
		RemoveComponent<SlimeSubbehaviourPlexer>(bait);
		Animator componentInChildren = bait.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			componentInChildren.SetBool("grounded", value: true);
		}
	}

	public void Destroy()
	{
		Gadget componentInParent = GetComponentInParent<Gadget>();
		if (componentInParent != null)
		{
			componentInParent.DestroyGadget();
		}
	}

	private void ClearBait()
	{
		if (bait != null)
		{
			model.baitTypeId = Identifiable.Id.NONE;
			Destroyer.Destroy(bait, 0f, "GordoSnare.ClearBait", asActorOk: true);
		}
	}

	private void RemoveComponent<T>(GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if (component != null)
		{
			Destroyer.Destroy(component, "GordoSnare.RemoveComponent");
		}
	}

	private void RemoveComponents<T>(GameObject gameObject) where T : Component
	{
		T[] components = gameObject.GetComponents<T>();
		for (int i = 0; i < components.Length; i++)
		{
			Destroyer.Destroy(components[i], "GordoSnare.RemoveComponents");
		}
	}

	private void RemoveComponentInChildren<T>(GameObject gameObject) where T : Component
	{
		T componentInChildren = gameObject.GetComponentInChildren<T>();
		if (componentInChildren != null)
		{
			Destroyer.Destroy(componentInChildren, "GordoSnare.RemoveComponentInChildren");
		}
	}

	private void RemoveComponentsInChildren<T>(GameObject gameObject) where T : Component
	{
		T[] componentsInChildren = gameObject.GetComponentsInChildren<T>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Destroyer.Destroy(componentsInChildren[i], "GordoSnare.RemoveComponentsInChildren");
		}
	}

	public static bool HasSnaredGordo(GadgetSite site)
	{
		if (site.HasAttached())
		{
			Gadget.Id attachedId = site.GetAttachedId();
			if (attachedId == Gadget.Id.GORDO_SNARE_NOVICE || attachedId == Gadget.Id.GORDO_SNARE_ADVANCED || attachedId == Gadget.Id.GORDO_SNARE_MASTER)
			{
				GordoSnare componentInChildren = site.GetAttached().GetComponentInChildren<GordoSnare>();
				if (componentInChildren != null)
				{
					return componentInChildren.HasSnaredGordo();
				}
			}
		}
		return false;
	}
}
