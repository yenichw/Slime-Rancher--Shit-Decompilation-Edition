using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
	[Tooltip("SFX played when this object is enabled.")]
	public SECTR_AudioCue cue;

	private SECTR_AudioCueInstance instance;

	public void OnEnable()
	{
		instance = SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}

	public void OnDisable()
	{
		instance.Stop(stopImmediately: false);
	}
}
