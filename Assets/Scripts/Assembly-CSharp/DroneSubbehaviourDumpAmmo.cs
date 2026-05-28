using UnityEngine;

public class DroneSubbehaviourDumpAmmo : DroneSubbehaviour
{
	private enum State
	{
		ANIMATE = 0,
		ELEVATE = 1,
		DUMP = 2
	}

	[HideInInspector]
	public bool destructive;

	private State state;

	private double time;

	public override bool Relevancy()
	{
		if (drone.ammo.IsEmpty())
		{
			return false;
		}
		return true;
	}

	public override void Selected()
	{
		base.Selected();
		drone.movement.rigidbody.isKinematic = true;
		drone.movement.rigidbody.velocity = Vector3.zero;
		drone.movement.rigidbody.angularVelocity = Vector3.zero;
		state = State.ANIMATE;
		drone.animator.SetAnimation(DroneAnimator.Id.IDLE_GRUMP);
		drone.animator.OnStateExit(DroneAnimatorState.Id.IDLE_GRUMP, delegate
		{
			drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
			state = State.ELEVATE;
		});
	}

	public override void Deselected()
	{
		base.Deselected();
		drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
		drone.movement.rigidbody.isKinematic = false;
	}

	public override void Action()
	{
		if (state == State.ELEVATE)
		{
			if (Physics.Raycast(drone.transform.position, Vector3.down, 3f))
			{
				Vector3 position = drone.transform.position + Vector3.up;
				drone.movement.MoveTowards(position);
			}
			else
			{
				state = State.DUMP;
				time = 0.0;
			}
		}
		if (state != State.DUMP || !OnAction_DumpAmmo(ref time) || !drone.ammo.IsEmpty())
		{
			return;
		}
		base.plexer.ForceRethink();
		if (destructive)
		{
			if (drone.metadata.onTeleportFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(drone.metadata.onTeleportFX, drone.transform.position, drone.transform.rotation);
			}
			Destroyer.Destroy(drone.gameObject, "DroneSubbehaviourDumpAmmo.Destructive.Action");
		}
	}
}
