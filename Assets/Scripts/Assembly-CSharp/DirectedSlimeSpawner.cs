using UnityEngine;

public class DirectedSlimeSpawner : DirectedActorSpawner
{
	[Tooltip("An extra slime effect to play along with each spawn.")]
	public GameObject slimeSpawnFX;

	protected ModDirector modDir;

	protected LookupDirector lookupDir;

	private Oasis oasis;

	public override void Awake()
	{
		base.Awake();
		modDir = SRSingleton<SceneContext>.Instance.ModDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		oasis = GetComponentInParent<Oasis>();
	}

	public override bool CanSpawn(float? forHour = null)
	{
		if (base.CanSpawn(forHour))
		{
			return !IsOasisFull();
		}
		return false;
	}

	protected override void Register(CellDirector cellDir)
	{
		cellDir.Register(this);
	}

	protected override GameObject MaybeReplacePrefab(GameObject prefab)
	{
		if (Randoms.SHARED.GetProbability(modDir.ChanceOfTarrSpawn()))
		{
			return lookupDir.GetPrefab(Identifiable.Id.TARR_SLIME);
		}
		return prefab;
	}

	protected override void SpawnFX(GameObject spawnedObj, Vector3 pos)
	{
		base.SpawnFX(spawnedObj, pos);
		SlimeAppearance.Palette appearancePalette = spawnedObj.GetComponent<SlimeAppearanceApplicator>().GetAppearancePalette();
		RecolorSlimeMaterial[] componentsInChildren = SRBehaviour.SpawnAndPlayFX(slimeSpawnFX, spawnedObj.transform.position, spawnedObj.transform.rotation).GetComponentsInChildren<RecolorSlimeMaterial>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetColors(appearancePalette.Top, appearancePalette.Middle, appearancePalette.Bottom);
		}
	}

	private bool IsOasisFull()
	{
		if (oasis != null)
		{
			return !oasis.NeedsMoreSlimes();
		}
		return false;
	}
}
