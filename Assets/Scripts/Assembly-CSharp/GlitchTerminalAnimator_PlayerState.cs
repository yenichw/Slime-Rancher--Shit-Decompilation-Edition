using UnityEngine;

public class GlitchTerminalAnimator_PlayerState : SRAnimatorState<GlitchTerminalAnimator_Player>
{
	public enum Id
	{
		NONE = 0,
		ENTERING = 1,
		EXITING = 2
	}

	[Tooltip("State identifier.")]
	public Id id;

	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateExit(animator, state, layerIndex);
		GetAnimatorWrapper(animator).OnStateExit(id);
	}
}
