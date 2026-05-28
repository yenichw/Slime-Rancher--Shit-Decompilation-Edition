using System.Collections;
using UnityEngine;

public class GlitchTerminalAnimator_Player : SRAnimator
{
	private delegate void OnStateChanged(GlitchTerminalAnimator_PlayerState.Id id);

	public enum AnimationEvent
	{
		ENTERING_FULLY_COVERED = 0
	}

	private delegate void OnAnimationEventListener(AnimationEvent eventId);

	public const string TRIGGER_ENTER_SLIMULATION = "trigger_enter_slimulation";

	public const string TRIGGER_EXIT_SLIMULATION = "trigger_exit_slimulation";

	private event OnStateChanged onStateExit;

	private event OnAnimationEventListener onAnimationEvent;

	public IEnumerator WaitForStateExit(GlitchTerminalAnimator_PlayerState.Id id)
	{
		bool wasTriggered = false;
		OnStateChanged listener = delegate(GlitchTerminalAnimator_PlayerState.Id otherid)
		{
			wasTriggered |= id == otherid;
		};
		onStateExit += listener;
		yield return new WaitUntil(() => wasTriggered);
		onStateExit -= listener;
	}

	public void OnStateExit(GlitchTerminalAnimator_PlayerState.Id id)
	{
		if (this.onStateExit != null)
		{
			this.onStateExit(id);
		}
	}

	public IEnumerator WaitForAnimationEvent(AnimationEvent eventId)
	{
		bool wasTriggered = false;
		OnAnimationEventListener listener = delegate(AnimationEvent otherid)
		{
			wasTriggered |= eventId == otherid;
		};
		onAnimationEvent += listener;
		yield return new WaitUntil(() => wasTriggered);
		onAnimationEvent -= listener;
	}

	public void OnAnimationEvent(AnimationEvent eventId)
	{
		if (this.onAnimationEvent != null)
		{
			this.onAnimationEvent(eventId);
		}
	}
}
