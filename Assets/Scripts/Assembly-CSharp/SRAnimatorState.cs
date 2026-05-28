using UnityEngine;

public abstract class SRAnimatorState<T> : StateMachineBehaviour where T : SRAnimator
{
	private T wrapper;

	protected T GetAnimatorWrapper(Animator animator)
	{
		if (wrapper == null)
		{
			wrapper = animator.gameObject.GetComponent<T>();
		}
		return wrapper;
	}
}
