using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class DecorizerStorage : IdHandler, DecorizerModel.Participant
{
	private static List<GameObject> CLEANUP_RESULTS = new List<GameObject>();

	private DecorizerModel model;

	private DecorizerModel.Settings settings;

	public Identifiable.Id selected
	{
		get
		{
			return settings.selected;
		}
		set
		{
			settings.selected = value;
		}
	}

	protected override string IdPrefix()
	{
		return "decorizer";
	}

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterDecorizer(this);
	}

	public void InitModel(DecorizerModel model)
	{
	}

	public void SetModel(DecorizerModel model)
	{
		this.model = model;
		settings = model.GetSettings(base.id);
	}

	public void OnDecorizerRemoved(Identifiable.Id id)
	{
		if (id == selected && model.GetCount(id) == 0)
		{
			selected = Identifiable.Id.NONE;
		}
	}

	public bool Add(Identifiable.Id id)
	{
		if (model.Add(id))
		{
			if (selected == Identifiable.Id.NONE)
			{
				selected = id;
			}
			return true;
		}
		return false;
	}

	public bool Remove(out Identifiable.Id id)
	{
		id = selected;
		if (model.Remove(selected))
		{
			return true;
		}
		id = Identifiable.Id.NONE;
		return false;
	}

	public int GetCount()
	{
		return model.GetCount(selected);
	}

	public void Cleanup(IEnumerable<Identifiable.Id> ids)
	{
		GetRequiredComponentInParent<CellDirector>().Get(ids, CLEANUP_RESULTS);
		for (int i = 0; i < CLEANUP_RESULTS.Count; i++)
		{
			GameObject actorObj = CLEANUP_RESULTS[i];
			if (Add(Identifiable.GetId(actorObj)))
			{
				Destroyer.DestroyActor(actorObj, "DecorizerStorage.Cleanup");
			}
		}
		CLEANUP_RESULTS.Clear();
	}
}
