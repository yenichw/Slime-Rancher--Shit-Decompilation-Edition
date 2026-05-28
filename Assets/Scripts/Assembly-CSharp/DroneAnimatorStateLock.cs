using UnityEngine;

public class DroneAnimatorStateLock : StateMachineBehaviour
{
	private int parameter;

	public void Awake()
	{
		parameter = Animator.StringToHash("TRANSITION_LOCK");
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		animator.SetBool(parameter, value: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		animator.SetBool(parameter, value: true);
	}
}
