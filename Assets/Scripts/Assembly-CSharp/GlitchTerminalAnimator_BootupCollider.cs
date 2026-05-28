using UnityEngine;

public class GlitchTerminalAnimator_BootupCollider : SRBehaviour
{
	private GlitchTerminalAnimator animator;

	public void Awake()
	{
		animator = GetRequiredComponentInParent<GlitchTerminalAnimator>();
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider) && animator.activator.GetLinkState() > GlitchTerminalActivator.LinkState.INACTIVE_PROGRESS)
		{
			animator.animator.SetBool("state_sleeping", value: false);
			Destroyer.Destroy(base.gameObject, "GlitchTerminalAnimator_BootupCollider.OnTriggerEnter");
		}
	}
}
