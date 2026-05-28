using UnityEngine;

public class QuicksilverSlowField : SRBehaviour
{
	[Tooltip("Duration, in game minutes, that the slow field is active.")]
	public float activeMinutes;

	[Tooltip("Duration, in game minutes, that the slow is applied to the slime.")]
	public float slowMinutes;

	[Tooltip("SFX played when a slime is slowed by this field.")]
	public SECTR_AudioCue onSlowAppliedCue;

	private double deathTime;

	public void Awake()
	{
		deathTime = SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNow(activeMinutes * (1f / 60f));
	}

	public void Update()
	{
		if (SRSingleton<SceneContext>.Instance.TimeDirector.HasReached(deathTime))
		{
			Destroyer.Destroy(base.gameObject, "QuicksilverSlowField.Update");
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		FollowWaypoints component = collider.GetComponent<FollowWaypoints>();
		if (component != null)
		{
			SECTR_AudioSystem.Play(onSlowAppliedCue, collider.transform.position, loop: false);
			component.ApplySlow(slowMinutes * (1f / 60f));
		}
	}
}
