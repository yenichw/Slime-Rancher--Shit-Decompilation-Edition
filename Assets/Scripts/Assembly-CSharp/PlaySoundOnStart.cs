using UnityEngine;

public class PlaySoundOnStart : MonoBehaviour
{
	[Tooltip("SFX played when this object is started.")]
	public SECTR_AudioCue cue;

	public void Start()
	{
		SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}
}
