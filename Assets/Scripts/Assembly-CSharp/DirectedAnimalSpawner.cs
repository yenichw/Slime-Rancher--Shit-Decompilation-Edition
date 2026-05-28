using System.Collections;
using MonomiPark.SlimeRancher.DataModel;

public class DirectedAnimalSpawner : DirectedActorSpawner, DirectedAnimalSpawnerModel.Participant
{
	public float minSpawnIntervalGameHours = 12f;

	public float maxSpawnIntervalGameHours = 18f;

	private DirectedAnimalSpawnerModel model;

	private Oasis oasis;

	public override void Awake()
	{
		base.Awake();
		oasis = GetComponentInParent<Oasis>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterAnimalSpawner(this);
	}

	public void InitModel(DirectedAnimalSpawnerModel model)
	{
		model.pos = base.transform.position;
	}

	public void SetModel(DirectedAnimalSpawnerModel model)
	{
		this.model = model;
	}

	public override bool CanSpawn(float? forHour = null)
	{
		if (base.CanSpawn(forHour) && timeDir.HasReached(model.nextSpawnTime))
		{
			return !IsOasisFull();
		}
		return false;
	}

	public override IEnumerator Spawn(int count, Randoms rand)
	{
		model.nextSpawnTime = timeDir.HoursFromNowOrStart(Randoms.SHARED.GetInRange(minSpawnIntervalGameHours, maxSpawnIntervalGameHours));
		yield return base.Spawn(count, rand);
	}

	public double GetNextSpawnTime()
	{
		return model.nextSpawnTime;
	}

	public void SetNextSpawnTime(double time)
	{
		model.nextSpawnTime = time;
	}

	protected override void Register(CellDirector cellDir)
	{
		cellDir.Register(this);
	}

	private bool IsOasisFull()
	{
		if (oasis != null)
		{
			return oasis.NeedsMoreAnimals();
		}
		return false;
	}
}
