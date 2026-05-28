using UnityEngine;

public class DroneAudioOnActive : MonoBehaviour
{
	[Tooltip("SFX cue to play while active.")]
	public SECTR_AudioCue cue;

	private SECTR_AudioCueInstance instance;

	public DroneAudioOnActive Init(SECTR_AudioCue cue)
	{
		instance.Stop(stopImmediately: false);
		this.cue = cue;
		instance = SECTR_AudioSystem.Play(this.cue, base.transform.position, loop: false);
		return this;
	}

	public void OnEnable()
	{
		instance = SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}

	public void OnDisable()
	{
		instance.Stop(stopImmediately: false);
	}

	public void Update()
	{
		instance.Position = base.transform.position;
	}
}
