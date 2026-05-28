using UnityEngine;

public class PlortCollectorActivator : MonoBehaviour, TechActivator
{
	public PlortCollector collector;

	public SECTR_AudioCue pressButtonCue;

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
			collector.StartCollection();
			if (buttonAnimator != null)
			{
				buttonAnimator.SetTrigger(buttonPressedTriggerId);
			}
			if (pressButtonCue != null)
			{
				SECTR_AudioSystem.Play(pressButtonCue, base.transform.position, loop: false);
			}
			nextAllowedActivationTime = Time.time + 0.4f;
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
