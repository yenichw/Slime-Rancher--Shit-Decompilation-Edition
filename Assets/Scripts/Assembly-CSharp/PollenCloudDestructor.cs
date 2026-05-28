using UnityEngine;

public class PollenCloudDestructor : RegisteredActorBehaviour, RegistryUpdateable
{
	public float gameHrsToLive = 0.5f;

	public float gameHrsInContactBeforeDeath = 0.05f;

	public GameObject destroyFX;

	private double dieAtTime;

	private double contactDeathTime = double.PositiveInfinity;

	private int contacts;

	private TimeDirector timeDir;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		dieAtTime = timeDir.HoursFromNow(gameHrsToLive);
	}

	public void RegistryUpdate()
	{
		if (timeDir.HasReached(dieAtTime) || timeDir.HasReached(contactDeathTime))
		{
			Destroyer.DestroyActor(base.gameObject, "PollenCloudDestructor.RegistryUpdate");
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
		}
	}

	public void AddContact()
	{
		if (contacts == 0)
		{
			contactDeathTime = timeDir.HoursFromNow(gameHrsInContactBeforeDeath);
		}
		contacts++;
	}

	public void RemoveContact()
	{
		contacts--;
		if (contacts <= 0)
		{
			contactDeathTime = double.PositiveInfinity;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
