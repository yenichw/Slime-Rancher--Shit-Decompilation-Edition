using UnityEngine;

public class LuckySlimeFlee : SlimeSubbehaviour
{
	public GameObject disappearFX;

	public SECTR_AudioCue disappearCue;

	private double? fleeProximityDisappearTime;

	private double? fleeTriggeredDisappearTime;

	private TimeDirector timeDir;

	private const float FLEE_PROXIMITY_WORLD_TIME = 600f;

	private const float FLEE_TRIGGERED_WORLD_TIME = 600f;

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void OnTriggerEnter(Collider collider)
	{
		Identifiable component = collider.gameObject.GetComponent<Identifiable>();
		if (!fleeProximityDisappearTime.HasValue && !fleeTriggeredDisappearTime.HasValue && component != null && component.id == Identifiable.Id.PLAYER)
		{
			Flee(collider.gameObject, triggered: false);
		}
	}

	public void StartFleeing(GameObject fleeFrom)
	{
		if (!fleeTriggeredDisappearTime.HasValue)
		{
			Flee(fleeFrom, triggered: true);
		}
	}

	private void Flee(GameObject fleeFrom, bool triggered)
	{
		if (triggered)
		{
			fleeTriggeredDisappearTime = timeDir.WorldTime() + 600.0;
			fleeProximityDisappearTime = null;
		}
		else
		{
			fleeProximityDisappearTime = timeDir.WorldTime() + 600.0;
		}
		plexer.ForceRethink();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (fleeProximityDisappearTime.HasValue || fleeTriggeredDisappearTime.HasValue)
		{
			return 1f;
		}
		return 0f;
	}

	public override void Selected()
	{
	}

	public override void Action()
	{
		if ((fleeProximityDisappearTime.HasValue && timeDir.HasReached(fleeProximityDisappearTime.Value)) || (fleeTriggeredDisappearTime.HasValue && timeDir.HasReached(fleeTriggeredDisappearTime.Value)))
		{
			SRBehaviour.SpawnAndPlayFX(disappearFX, base.transform.position, base.transform.rotation);
			if (disappearCue != null)
			{
				SECTR_AudioSystem.Play(disappearCue, base.transform.position, loop: false);
			}
			Destroyer.DestroyActor(base.gameObject, "LuckySlimeFlee.Action");
		}
	}
}
