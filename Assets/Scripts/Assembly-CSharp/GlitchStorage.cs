using MonomiPark.SlimeRancher.DataModel;

public class GlitchStorage : IdHandler<GlitchStorageModel>
{
	private const SiloStorage.StorageType STORAGE_TYPE = SiloStorage.StorageType.NON_SLIMES;

	private const int MAX_COUNT = 300;

	private GlitchStorageModel model;

	public Identifiable.Id selected => model.id;

	public int count => model.count;

	public override void Awake()
	{
		base.Awake();
		GetRequiredComponentInChildren<WorldStatusBar>().maxValue = 300f;
	}

	protected override string IdPrefix()
	{
		return "glitchST";
	}

	protected override GameModel.Unregistrant Register(GameModel game)
	{
		return game.Glitch.storage.Register(this);
	}

	protected override void InitModel(GlitchStorageModel model)
	{
	}

	protected override void SetModel(GlitchStorageModel model)
	{
		this.model = model;
	}

	public bool Add(Identifiable.Id id)
	{
		if (model.count > 0 && model.id != id)
		{
			return false;
		}
		if (model.count >= 300)
		{
			return false;
		}
		if (model.count == 0 && !SiloStorage.StorageType.NON_SLIMES.Contains(id))
		{
			return false;
		}
		model.count++;
		model.id = id;
		return true;
	}

	public bool Remove(out Identifiable.Id id)
	{
		id = model.id;
		if (model.count > 0)
		{
			model.count--;
			if (model.count == 0)
			{
				model.id = Identifiable.Id.NONE;
			}
			return true;
		}
		return false;
	}
}
