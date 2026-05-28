using UnityEngine;

public class QuicksilverElectricField : SRBehaviour
{
	[Tooltip("Duration, in game minutes, that the electric field is active.")]
	public float activeMinutes;

	[Tooltip("SFX played when a slime is electrified by the field and produces a plort.")]
	public SECTR_AudioCue onElectrifiedCue;

	private double deathTime;

	public void Awake()
	{
		ResetDeathTime();
	}

	public void Update()
	{
		if (SRSingleton<SceneContext>.Instance.TimeDirector.HasReached(deathTime))
		{
			Destroyer.Destroy(base.gameObject, "QuicksilverElectricField.Update");
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		ReactToShock component = collider.GetComponent<ReactToShock>();
		if (component != null && component.MaybeCreatePlorts(1, ReactToShock.PlortSounds.SUCCESS))
		{
			SECTR_AudioSystem.Play(onElectrifiedCue, collider.transform.position, loop: false);
		}
	}

	public void ResetDeathTime()
	{
		deathTime = SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNow(activeMinutes * (1f / 60f));
	}
}
