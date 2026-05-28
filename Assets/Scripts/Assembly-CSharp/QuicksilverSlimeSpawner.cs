using System;
using System.Collections.Generic;
using UnityEngine;

public class QuicksilverSlimeSpawner : DirectedSlimeSpawner
{
	[Tooltip("Energy generator to control spawning.")]
	public QuicksilverEnergyGenerator generator;

	[Tooltip("Duration, in game minutes, that the slimes will flee before being destroyed. Prevents stuck slimes from never despawning.")]
	public float maxFleeMinutes = 30f;

	private CellDirector cellDirector;

	private List<GameObject> spawned = new List<GameObject>();

	public override void Awake()
	{
		base.Awake();
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Combine(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	public void OnDestroy()
	{
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Remove(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	protected override void Register(CellDirector cellDirector)
	{
		base.Register(cellDirector);
		this.cellDirector = cellDirector;
	}

	protected override void OnActorSpawned(GameObject slime)
	{
		base.OnActorSpawned(slime);
		spawned.Add(slime);
	}

	private void OnGeneratorStateChanged()
	{
		if (generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE)
		{
			if (cellDirector != null)
			{
				cellDirector.ForceCheckSpawn();
			}
			return;
		}
		for (int i = 0; i < spawned.Count; i++)
		{
			GameObject gameObject = spawned[i];
			if (gameObject != null)
			{
				if (SRSingleton<SceneContext>.Instance.Player != null)
				{
					gameObject.GetComponent<SlimeFlee>().StartFleeing(SRSingleton<SceneContext>.Instance.Player);
					DestroyAfterTime destroyAfterTime = gameObject.AddComponent<DestroyAfterTime>();
					destroyAfterTime.SetDeathTime(SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNow(maxFleeMinutes * (1f / 60f)));
					destroyAfterTime.ScaleDownOnDestroy();
				}
				else
				{
					Destroyer.Destroy(gameObject, "QuicksilverSlimeSpawner.OnGeneratorStateChanged");
				}
			}
		}
		spawned.Clear();
	}

	public override bool CanSpawn(float? forHour = null)
	{
		if (base.CanSpawn(forHour))
		{
			return generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE;
		}
		return false;
	}

	protected override bool CanContinueSpawning()
	{
		if (base.CanContinueSpawning())
		{
			return generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE;
		}
		return false;
	}
}
