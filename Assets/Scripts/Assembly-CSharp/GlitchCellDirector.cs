using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using UnityEngine;

public class GlitchCellDirector : CellDirector
{
	[Header("Tarr Properties")]
	[Tooltip("Target number of tarr slimes to be in the cell.")]
	public int targetTarrCount;

	[Tooltip("Number of tarr slime to spawn per spawn. (random range)")]
	public Vector2 tarrSpawnCount;

	[Tooltip("Tarr activation major group.")]
	public GlitchTarrNode.Group tarrActivationGroup;

	private GlitchMetadata metadata;

	private List<GlitchTarrNodeSpawner> tarrSpawners;

	private double tarrNextSpawn;

	public override void Awake()
	{
		base.Awake();
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		tarrSpawners = new List<GlitchTarrNodeSpawner>();
	}

	protected override void UpdateToTime(double worldTime)
	{
		base.UpdateToTime(worldTime);
		if (!TimeUtil.HasReached(worldTime, tarrNextSpawn))
		{
			return;
		}
		if (tarrSpawners.Count > 0 && NeedsMoreTarrs())
		{
			GlitchTarrNodeSpawner glitchTarrNodeSpawner = rand.Pick(tarrSpawners, (GlitchTarrNodeSpawner it) => (!it.CanSpawn()) ? 0f : it.directedSpawnWeight, null);
			if (glitchTarrNodeSpawner != null)
			{
				StartCoroutine(glitchTarrNodeSpawner.Spawn(Mathf.RoundToInt(tarrSpawnCount.GetRandom(rand)), rand));
			}
		}
		tarrNextSpawn = worldTime + (double)(metadata.tarrSpawnerThrottleTime * 60f);
	}

	public override void ForceCheckSpawn()
	{
		base.ForceCheckSpawn();
		tarrNextSpawn = 0.0;
	}

	public void Register(GlitchTarrNodeSpawner spawner)
	{
		tarrSpawners.Add(spawner);
	}

	protected override bool CanSpawnSlimes()
	{
		return true;
	}

	private bool NeedsMoreTarrs()
	{
		return tarrSlimeCount < targetTarrCount;
	}
}
