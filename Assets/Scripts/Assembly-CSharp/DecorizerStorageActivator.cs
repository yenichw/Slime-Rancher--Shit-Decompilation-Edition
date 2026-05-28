using UnityEngine;

public class DecorizerStorageActivator : SRBehaviour, TechActivator
{
	private Animator buttonAnimator;

	private int buttonAnimation;

	private const float TIME_BETWEEN_ACTIVATIONS = 0.4f;

	private float nextActivationTime;

	private DecorizerStorage storage;

	public void Awake()
	{
		buttonAnimator = GetComponentInParent<Animator>();
		buttonAnimation = Animator.StringToHash("ButtonPressed");
		storage = GetComponentInParent<DecorizerStorage>();
	}

	public void Activate()
	{
		if (nextActivationTime < Time.time)
		{
			nextActivationTime = Time.time + 0.4f;
			buttonAnimator.SetTrigger(buttonAnimation);
			Object.Instantiate(SRSingleton<GameContext>.Instance.UITemplates.decorizerUIPrefab).GetComponent<DecorizerUI>().storage = storage;
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
