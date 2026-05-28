using UnityEngine;

public class SlimeAnimatorStateIdle : StateMachineBehaviour
{
	private bool? isCurrentState;

	public bool IsCurrentState => isCurrentState == true;

	public bool IsInitialized => isCurrentState.HasValue;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateEnter(animator, state, layerIndex);
		isCurrentState = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateExit(animator, state, layerIndex);
		isCurrentState = false;
	}
}
