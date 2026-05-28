using UnityEngine;

public class BeatrixRandom : StateMachineBehaviour
{
	public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
	{
		animator.SetInteger("IdleIndex", Random.Range(0, 8));
	}
}
