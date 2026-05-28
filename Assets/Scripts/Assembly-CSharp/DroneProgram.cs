using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DroneProgram : DroneSubbehaviour
{
	public class Orientation
	{
		public Vector3 pos;

		public Quaternion rot;

		public Orientation()
		{
		}

		public Orientation(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}
	}

	private enum State
	{
		PATHING = 0,
		PATHING_ARRIVAL = 1,
		PATHING_ARRIVED = 2,
		ACTION_PRE = 3,
		ACTION_LOOP_FIRST = 4,
		ACTION_LOOP = 5,
		ACTION_POST = 6,
		COMPLETE = 7
	}

	private const float ARRIVE_RAD = 1f;

	private const float NO_CLIP_PERIOD = 10f;

	private const float ARRIVAL_NO_CLIP_PERIOD = 2f;

	private State state;

	private Queue<Vector3> existingPath;

	private Orientation arrivalOrient;

	private Vector3 previousTargetPosition;

	private Vector3 noClipPreviousPosition;

	private float noClipTime;

	protected abstract DroneAnimator.Id animation { get; }

	protected abstract DroneAnimatorState.Id animationStateBegin { get; }

	protected abstract DroneAnimatorState.Id animationStateEnd { get; }

	public override void Selected()
	{
		base.Selected();
		drone.animator.SetAnimation(DroneAnimator.Id.MOVE);
		noClipPreviousPosition = drone.transform.position;
		noClipTime = 10f;
		state = State.PATHING;
	}

	public override void Deselected()
	{
		base.Deselected();
		drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
		drone.movement.rigidbody.isKinematic = false;
		drone.upright.enabled = true;
		existingPath = null;
	}

	public void OnDrawGizmos()
	{
		if (existingPath == null)
		{
			return;
		}
		Gizmos.color = Color.blue;
		Vector3 from = drone.transform.position;
		foreach (Vector3 item in existingPath)
		{
			Gizmos.DrawLine(from, item);
			from = item;
		}
	}

	public sealed override void Action()
	{
		if (state == State.COMPLETE || CanCancel())
		{
			base.plexer.ForceRethink();
			return;
		}
		if ((state == State.PATHING || state == State.PATHING_ARRIVAL) && !drone.noClip.enabled)
		{
			noClipTime -= Time.fixedDeltaTime;
			if (noClipTime <= 0f)
			{
				float sqrMagnitude = (noClipPreviousPosition - drone.transform.position).sqrMagnitude;
				noClipPreviousPosition = drone.transform.position;
				noClipTime = ((state == State.PATHING_ARRIVAL) ? 2f : 10f);
				drone.noClip.enabled = sqrMagnitude <= 0.1f;
			}
		}
		if (state == State.PATHING)
		{
			if (existingPath == null)
			{
				if (!timeDirector.HasReached(drone.network.pathingThrottleUntil))
				{
					return;
				}
				drone.network.pathingThrottleUntil = timeDirector.HoursFromNow(1f / 30f);
				GeneratePath(GetSubnetwork(), GetTargetOrientations(), GetTargetPosition());
			}
			if (existingPath == null)
			{
				base.plexer.ForceRethink();
				return;
			}
			Vector3? vector = ((existingPath.Count > 0) ? new Vector3?(existingPath.Peek()) : null);
			if (!vector.HasValue || (vector.Value - drone.transform.position).sqrMagnitude <= 1f)
			{
				if (existingPath.Count <= 1)
				{
					if ((GetTargetPosition() - previousTargetPosition).sqrMagnitude >= 1f)
					{
						existingPath = null;
						return;
					}
					state = State.PATHING_ARRIVAL;
					noClipTime = Mathf.Min(noClipTime, 2f);
					drone.movement.rigidbody.velocity = Vector3.zero;
					drone.movement.rigidbody.angularVelocity = Vector3.zero;
					drone.upright.enabled = false;
					existingPath = null;
				}
				else
				{
					drone.noClip.enabled = false;
					existingPath.Dequeue();
				}
			}
			else
			{
				drone.movement.PathTowards(vector.Value);
			}
		}
		if (state == State.PATHING_ARRIVAL && drone.movement.MoveTowards(arrivalOrient.pos) && drone.movement.RotateTowards(arrivalOrient.rot))
		{
			drone.movement.rigidbody.isKinematic = true;
			OnReachedDestination();
			state = State.PATHING_ARRIVED;
			return;
		}
		if (state == State.PATHING_ARRIVED)
		{
			Action action = delegate
			{
				state = State.ACTION_LOOP_FIRST;
			};
			if (animationStateBegin != 0)
			{
				state = State.ACTION_PRE;
				drone.animator.SetAnimation(animation);
				drone.animator.OnStateExit(animationStateBegin, action);
			}
			else
			{
				drone.animator.SetAnimation(animation);
				action();
			}
		}
		if (state == State.ACTION_LOOP_FIRST)
		{
			state = State.ACTION_LOOP;
			OnFirstAction();
		}
		if (state == State.ACTION_LOOP && OnAction())
		{
			Action action2 = delegate
			{
				state = State.COMPLETE;
			};
			if (animationStateEnd != 0)
			{
				state = State.ACTION_POST;
				drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
				drone.animator.OnStateExit(animationStateEnd, action2);
			}
			else
			{
				drone.animator.SetAnimation(DroneAnimator.Id.IDLE);
				action2();
			}
		}
	}

	protected bool GeneratePath(GardenDroneSubnetwork subnetwork, IEnumerable<Orientation> orientations, Vector3 position)
	{
		Vector3 position2 = drone.transform.position;
		List<PathingNetwork> list = new List<PathingNetwork>();
		if (subnetwork != null)
		{
			list.Add(subnetwork);
		}
		list.Add(drone.network);
		foreach (Orientation orientation in orientations)
		{
			foreach (PathingNetwork item in list)
			{
				if ((existingPath = item.GeneratePath(position2, orientation.pos)) != null)
				{
					arrivalOrient = orientation;
					previousTargetPosition = position;
					return true;
				}
			}
		}
		OnPathGenerationFailed();
		return false;
	}

	protected virtual GardenDroneSubnetwork GetSubnetwork()
	{
		return null;
	}

	protected abstract IEnumerable<Orientation> GetTargetOrientations();

	protected abstract Vector3 GetTargetPosition();

	protected virtual bool CanCancel()
	{
		return false;
	}

	protected virtual void OnReachedDestination()
	{
	}

	protected virtual void OnFirstAction()
	{
	}

	protected virtual bool OnAction()
	{
		return true;
	}

	protected virtual void OnPathGenerationFailed()
	{
	}
}
