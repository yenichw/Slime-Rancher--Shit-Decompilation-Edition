using UnityEngine;

public class DestroyAfterAnimatorStateExit : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateExit(animator, state, layerIndex);
		Destroyer.Destroy(animator.gameObject, "DestroyAfterAnimatorStateExit.OnStateExit");
	}
}
