using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class DestroyPlortAfterTime : RegisteredActorBehaviour, RegistryUpdateable, ActorModel.Participant
{
	public float lifeTimeHours = 24f;

	public GameObject destroyFX;

	private TimeDirector timeDir;

	private bool destroying;

	private PlortModel plortModel;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void InitModel(ActorModel model)
	{
		((PlortModel)model).destroyTime = timeDir.HoursFromNowOrStart(lifeTimeHours);
	}

	public void SetModel(ActorModel model)
	{
		plortModel = (PlortModel)model;
	}

	public void RegistryUpdate()
	{
		if (!timeDir.HasReached(plortModel.destroyTime) || destroying)
		{
			return;
		}
		destroying = true;
		bool num = timeDir.HasReached(plortModel.destroyTime + 3600.0);
		GetComponent<DestroyAfterTimeListener>()?.WillDestroyAfterTime();
		if (num)
		{
			DoDestroy("DestroyAfterTime.RegistryUpdate (skippedFX)");
			return;
		}
		DoDestroy("DestroyAfterTime.RegistryUpdate");
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, Quaternion.identity);
		}
	}

	private void DoDestroy(string reason)
	{
		Destroyer.DestroyActor(base.gameObject, reason);
	}
}
