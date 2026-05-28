using System.Collections.Generic;
using UnityEngine;

public class DroneAnimatorState : SRAnimatorState<DroneAnimator>
{
	public enum Id
	{
		NONE = 0,
		GATHER_BEGIN = 10,
		GATHER_LOOP = 11,
		GATHER_END = 12,
		DEPOSIT_BEGIN = 20,
		DEPOSIT_LOOP = 21,
		DEPOSIT_END = 22,
		REST_BEGIN = 30,
		REST_LOOP = 31,
		REST_END = 32,
		IDLE_CELEBRATE = 100,
		IDLE_GRUMP = 200
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public static IdComparer Instance = new IdComparer();

		public bool Equals(Id a, Id b)
		{
			return a == b;
		}

		public int GetHashCode(Id a)
		{
			return (int)a;
		}
	}

	[Tooltip("Looping state identifier.")]
	public Id id;

	private Drone drone;

	private DroneAudioOnActive audio;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateEnter(animator, state, layerIndex);
		Drone drone = GetDrone(animator);
		Destroyer.Destroy(audio, "DroneAnimatorState.OnStateEnter");
		audio = drone.SFX(GetAudioCue(id, drone.metadata));
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo state, int layerIndex)
	{
		base.OnStateExit(animator, state, layerIndex);
		Destroyer.Destroy(audio, "DroneAnimatorState.OnStateExit");
		GetAnimatorWrapper(animator).OnStateExit(id);
	}

	public void OnDestroy()
	{
		Destroyer.Destroy(audio, "DroneAnimatorState.OnStateExit");
	}

	private Drone GetDrone(Animator animator)
	{
		if (drone == null)
		{
			drone = animator.gameObject.GetComponentInParent<Drone>();
		}
		return drone;
	}

	private static SECTR_AudioCue GetAudioCue(Id id, DroneMetadata metadata)
	{
		switch (id)
		{
		case Id.GATHER_BEGIN:
			return metadata.onGatherBeginCue;
		case Id.GATHER_LOOP:
			return metadata.onGatherLoopCue;
		case Id.GATHER_END:
			return metadata.onGatherEndCue;
		case Id.DEPOSIT_BEGIN:
			return metadata.onDepositBeginCue;
		case Id.DEPOSIT_LOOP:
			return metadata.onDepositLoopCue;
		case Id.DEPOSIT_END:
			return metadata.onDepositEndCue;
		case Id.REST_BEGIN:
			return metadata.onRestBeginCue;
		case Id.REST_LOOP:
			return metadata.onRestLoopCue;
		case Id.REST_END:
			return metadata.onRestEndCue;
		case Id.IDLE_CELEBRATE:
			return metadata.onHappyCue;
		case Id.IDLE_GRUMP:
			return metadata.onGrumpyCue;
		default:
			return null;
		}
	}
}
