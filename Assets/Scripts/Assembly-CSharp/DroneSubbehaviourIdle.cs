using UnityEngine;

public class DroneSubbehaviourIdle : DroneSubbehaviour
{
	private double cooldown;

	private Quaternion? rotation;

	public override bool Relevancy()
	{
		if (!drone.station.battery.HasAny())
		{
			return false;
		}
		if (!timeDirector.HasReached(cooldown))
		{
			return false;
		}
		if (!Randoms.SHARED.GetProbability(0.1f))
		{
			return false;
		}
		return true;
	}

	public override void Selected()
	{
		base.Selected();
		drone.movement.rigidbody.isKinematic = true;
		rotation = Quaternion.LookRotation(SRSingleton<SceneContext>.Instance.Player.transform.position - drone.transform.position);
		cooldown = timeDirector.HoursFromNow(3f);
	}

	public override void Deselected()
	{
		base.Deselected();
		drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
		drone.movement.rigidbody.isKinematic = false;
	}

	public override void Action()
	{
		if (rotation.HasValue && drone.movement.RotateTowards(rotation.Value))
		{
			drone.animator.SetAnimation(DroneAnimator.Id.IDLE_CELEBRATE);
			drone.animator.OnStateExit(DroneAnimatorState.Id.IDLE_CELEBRATE, delegate
			{
				base.plexer.ForceRethink();
			});
			rotation = null;
		}
	}
}
