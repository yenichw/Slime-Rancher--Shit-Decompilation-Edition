using UnityEngine;

public class FeederSpeedSelector : MonoBehaviour, TechActivator
{
	public SlimeFeeder feeder;

	public SECTR_AudioCue pressButtonCueSlow;

	public SECTR_AudioCue pressButtonCueNormal;

	public SECTR_AudioCue pressButtonCueFast;

	private Animator buttonAnimator;

	private int buttonPressedTriggerId;

	private const float TIME_BETWEEN_ACTIVATIONS = 0.4f;

	private float nextAllowedActivationTime;

	public void Awake()
	{
		buttonAnimator = GetComponentInParent<Animator>();
		buttonPressedTriggerId = Animator.StringToHash("ButtonPressed");
	}

	public void Activate()
	{
		if (nextAllowedActivationTime <= Time.time)
		{
			feeder.StepFeederSpeed();
			if (buttonAnimator != null)
			{
				buttonAnimator.SetTrigger(buttonPressedTriggerId);
			}
			SECTR_AudioCue sECTR_AudioCue = pressButtonCueNormal;
			switch (feeder.GetFeedingCycleSpeed())
			{
			case SlimeFeeder.FeedSpeed.Slow:
				sECTR_AudioCue = pressButtonCueSlow;
				break;
			case SlimeFeeder.FeedSpeed.Normal:
				sECTR_AudioCue = pressButtonCueNormal;
				break;
			case SlimeFeeder.FeedSpeed.Fast:
				sECTR_AudioCue = pressButtonCueFast;
				break;
			default:
				sECTR_AudioCue = pressButtonCueNormal;
				Log.Error("Invalid feeder speed.");
				break;
			}
			if (sECTR_AudioCue != null)
			{
				SECTR_AudioSystem.Play(sECTR_AudioCue, base.transform.position, loop: false);
			}
			nextAllowedActivationTime = Time.time + 0.4f;
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
