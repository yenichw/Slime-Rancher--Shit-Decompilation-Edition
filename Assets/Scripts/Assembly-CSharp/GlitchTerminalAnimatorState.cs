using UnityEngine;

public class GlitchTerminalAnimatorState : SRAnimatorState<GlitchTerminalAnimator>
{
	public enum Id
	{
		NONE = 0,
		SLEEP = 1,
		BOOT_UP = 2,
		IDLE = 3
	}

	[Tooltip("State identifier.")]
	public Id id;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateEnter(animator, state, layerIndex);
		GetAnimatorWrapper(animator).OnStateEnter(id);
	}
}
