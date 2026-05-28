using UnityEngine;

public class GlitchTerminalAudio : SRBehaviour
{
	[Tooltip("Reference to the parent animator.")]
	public GlitchTerminalAnimator animator;

	[Tooltip("Transform of where to play the BOOT_UP state sound.")]
	public Transform onStateBootup;

	[Tooltip("Sound component playing the IDLE state sound.")]
	public PlaySoundOnEnable onStateIdle;

	public void Awake()
	{
		GlitchMetadata metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		onStateIdle.cue = metadata.animationOnTerminalIdleCue;
		onStateIdle.gameObject.SetActive(value: false);
		animator.onStateEnter += delegate(GlitchTerminalAnimatorState.Id id)
		{
			switch (id)
			{
			case GlitchTerminalAnimatorState.Id.SLEEP:
				onStateIdle.gameObject.SetActive(value: false);
				break;
			case GlitchTerminalAnimatorState.Id.BOOT_UP:
				SECTR_AudioSystem.Play(metadata.animationOnTerminalBootupCue, onStateBootup.position, loop: false);
				break;
			case GlitchTerminalAnimatorState.Id.IDLE:
				onStateIdle.gameObject.SetActive(value: true);
				break;
			}
		};
	}
}
