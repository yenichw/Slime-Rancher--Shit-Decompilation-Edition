using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public abstract class IdHandler : SRBehaviour
{
	private IdDirector director;

	public string id
	{
		get
		{
			if (director == null)
			{
				director = GetRequiredComponentInParent<IdDirector>();
			}
			return director.GetPersistenceIdentifier(this);
		}
	}

	protected abstract string IdPrefix();
}
public abstract class IdHandler<M> : IdHandler, IdHandlerModel.Participant where M : IdHandlerModel
{
	private GameModel.Unregistrant unregistrant;

	public void InitModel(IdHandlerModel model)
	{
		M model2 = model as M;
		InitModel(model2);
	}

	public void SetModel(IdHandlerModel model)
	{
		M model2 = model as M;
		SetModel(model2);
	}

	public string GetId()
	{
		return base.id;
	}

	public virtual void Awake()
	{
		if (Application.isPlaying && SRSingleton<SceneContext>.Instance != null)
		{
			unregistrant = Register(SRSingleton<SceneContext>.Instance.GameModel);
		}
	}

	public virtual void OnDestroy()
	{
		if (unregistrant != null)
		{
			unregistrant();
			unregistrant = null;
		}
	}

	protected abstract GameModel.Unregistrant Register(GameModel game);

	protected abstract void InitModel(M model);

	protected abstract void SetModel(M model);
}
