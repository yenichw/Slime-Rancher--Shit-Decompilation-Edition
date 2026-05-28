using UnityEngine;

public class SlimeAnimationEventHandler : MonoBehaviour
{
	private SlimeFaceAnimator sfAnimator;

	public void Awake()
	{
		sfAnimator = GetComponent<SlimeFaceAnimator>();
	}

	public void TriggerAweFace()
	{
		TriggerFace("triggerAwe");
	}

	public void TriggerAlarmFace()
	{
		TriggerFace("triggerAlarm");
	}

	public void TriggerWinceFace()
	{
		TriggerFace("triggerWince");
	}

	public void TriggerMinorWinceFace()
	{
		TriggerFace("triggerMinorWince");
	}

	public void TriggerAttackTelegraphFace()
	{
		TriggerFace("triggerAttackTelegraph");
	}

	public void TriggerChompOpenFace()
	{
		TriggerFace("triggerChompOpen");
	}

	public void TriggerChompOpenQuickFace()
	{
		TriggerFace("triggerChompOpenQuick");
	}

	public void TriggerChompClosedFace()
	{
		TriggerFace("triggerChompClosed");
	}

	public void TriggerInvokeFace()
	{
		TriggerFace("triggerConcentrate");
	}

	public void TriggerGrimaceFace()
	{
		TriggerFace("triggerGrimace");
	}

	public void TriggerFriedFace()
	{
		TriggerFace("triggerFried");
	}

	private void TriggerFace(string faceTrigger)
	{
		sfAnimator.SetTrigger(faceTrigger);
	}
}
