using UnityEngine;

public class PlaySoundOnDestroy : MonoBehaviour
{
	[Tooltip("SFX played when this object is destroyed.")]
	public SECTR_AudioCue cue;

	public void OnDestroy()
	{
		SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
	}
}
