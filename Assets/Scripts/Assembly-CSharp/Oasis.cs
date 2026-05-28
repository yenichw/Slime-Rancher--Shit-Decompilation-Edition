using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class Oasis : IdHandler, OasisModel.Participant
{
	public float scaleUpTime = 8f;

	public int targetSlimeCount = 20;

	public int targetAnimalCount = 5;

	public FireColumn[] suppressColumns;

	private Transform[] children;

	private SphereCollider oasisCollider;

	private List<Identifiable> slimeIdents = new List<Identifiable>();

	private List<Identifiable> animalIdents = new List<Identifiable>();

	private BoundingSphere boundingSphere;

	private OasisModel model;

	public static ExposedArrayList<BoundingSphere> oasisSpheres = new ExposedArrayList<BoundingSphere>(20);

	public void Awake()
	{
		oasisCollider = GetComponent<SphereCollider>();
		if (oasisCollider != null)
		{
			oasisCollider.enabled = false;
		}
		SRSingleton<SceneContext>.Instance.GameModel.RegisterOasis(base.id, base.gameObject);
		CreateBoundingSphere();
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterOasis(base.id);
		}
	}

	private void CreateBoundingSphere()
	{
		Vector3 position = base.transform.position;
		boundingSphere = new BoundingSphere(new Vector4(position.x + oasisCollider.center.x, position.y + oasisCollider.center.y, position.z + oasisCollider.center.z, oasisCollider.radius));
	}

	public void InitModel(OasisModel model)
	{
	}

	public void SetModel(OasisModel model)
	{
		this.model = model;
		int childCount = base.transform.childCount;
		children = new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			children[i] = base.transform.GetChild(i);
			if (!model.isLive)
			{
				children[i].gameObject.SetActive(value: false);
			}
		}
		if (model.isLive)
		{
			OnSetLive(immediate: true);
		}
	}

	public void SetLive(bool immediate)
	{
		if (!model.isLive)
		{
			OnSetLive(immediate);
		}
	}

	private void OnSetLive(bool immediate)
	{
		if (oasisCollider != null)
		{
			oasisCollider.enabled = true;
			oasisSpheres.Add(boundingSphere);
		}
		model.isLive = true;
		TweenScaleChildren(immediate);
		if (!immediate)
		{
			StartCoroutine(DelayedTriggerAllSpawners());
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.ACTIVATED_OASES, 1);
		}
		FireColumn[] array = suppressColumns;
		foreach (FireColumn fireColumn in array)
		{
			if (fireColumn != null)
			{
				fireColumn.NoteInOasis();
			}
		}
	}

	public bool IsLive()
	{
		return model.isLive;
	}

	private IEnumerator DelayedTriggerAllSpawners()
	{
		yield return new WaitForSeconds(scaleUpTime);
		TriggerAllSpawners();
	}

	private void TriggerAllSpawners()
	{
		DirectedSlimeSpawner[] componentsInChildren = GetComponentsInChildren<DirectedSlimeSpawner>();
		List<DirectedSlimeSpawner> list = new List<DirectedSlimeSpawner>();
		DirectedSlimeSpawner[] array = componentsInChildren;
		foreach (DirectedSlimeSpawner directedSlimeSpawner in array)
		{
			if (directedSlimeSpawner.allowDirectedSpawns)
			{
				list.Add(directedSlimeSpawner);
			}
		}
		foreach (DirectedSlimeSpawner item in list)
		{
			StartCoroutine(item.Spawn(targetSlimeCount / list.Count, Randoms.SHARED));
		}
		DirectedAnimalSpawner[] componentsInChildren2 = GetComponentsInChildren<DirectedAnimalSpawner>();
		List<DirectedAnimalSpawner> list2 = new List<DirectedAnimalSpawner>();
		DirectedAnimalSpawner[] array2 = componentsInChildren2;
		foreach (DirectedAnimalSpawner directedAnimalSpawner in array2)
		{
			if (directedAnimalSpawner.allowDirectedSpawns)
			{
				list2.Add(directedAnimalSpawner);
			}
		}
		foreach (DirectedAnimalSpawner item2 in list2)
		{
			StartCoroutine(item2.Spawn(targetAnimalCount / list2.Count, Randoms.SHARED));
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		FireBall component = col.GetComponent<FireBall>();
		if (component != null)
		{
			component.Vaporize();
			return;
		}
		Identifiable component2 = col.GetComponent<Identifiable>();
		if (component2 != null)
		{
			if (Identifiable.IsSlime(component2.id))
			{
				slimeIdents.Add(component2);
			}
			else if (Identifiable.IsAnimal(component2.id))
			{
				animalIdents.Add(component2);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		Identifiable component = col.GetComponent<Identifiable>();
		if (component != null)
		{
			if (Identifiable.IsSlime(component.id))
			{
				slimeIdents.Remove(component);
			}
			else if (Identifiable.IsAnimal(component.id))
			{
				animalIdents.Remove(component);
			}
		}
	}

	public bool NeedsMoreSlimes()
	{
		EnsureNoDestroyedIdents();
		return slimeIdents.Count < targetSlimeCount;
	}

	public bool NeedsMoreAnimals()
	{
		EnsureNoDestroyedIdents();
		return animalIdents.Count < targetAnimalCount;
	}

	private void TweenScaleChildren(bool immediate)
	{
		Transform[] array = children;
		foreach (Transform transform in array)
		{
			transform.gameObject.SetActive(value: true);
			if (!immediate)
			{
				SpawnResource[] spawners = transform.GetComponentsInChildren<SpawnResource>(includeInactive: true);
				SpawnResource[] array2 = spawners;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].RegisterSpawnBlocker();
				}
				TweenUtil.ScaleIn(transform.gameObject, scaleUpTime).OnComplete(delegate
				{
					TweenScaleChildren_OnTweenComplete(spawners);
				});
			}
		}
	}

	private void TweenScaleChildren_OnTweenComplete(SpawnResource[] spawners)
	{
		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].DeregisterSpawnBlocker();
		}
	}

	private void EnsureNoDestroyedIdents()
	{
		slimeIdents.RemoveAll((Identifiable ident) => ident == null);
		animalIdents.RemoveAll((Identifiable ident) => ident == null);
	}

	protected override string IdPrefix()
	{
		return "oasis";
	}
}
