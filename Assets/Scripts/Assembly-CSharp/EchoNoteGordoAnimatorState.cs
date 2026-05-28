using UnityEngine;

public class EchoNoteGordoAnimatorState : SRAnimatorState<EchoNoteGordoAnimator>
{
	public enum Id
	{
		NONE = 0,
		PRE_ACTIVATION = 1,
		ACTIVATION = 2
	}

	[Tooltip("Animation state identifier.")]
	public Id id;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateEnter(animator, state, layerIndex);
		GetAnimatorWrapper(animator).parent.OnAnimationEvent_StateEnter(id);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateExit(animator, state, layerIndex);
		GetAnimatorWrapper(animator).parent.OnAnimationEvent_StateExit(id);
	}
}
