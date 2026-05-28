using System;
using UnityEngine;

public class QuicksilverEnergyActivator : MonoBehaviour, TechActivator
{
	[Tooltip("Energy generator to activate.")]
	public QuicksilverEnergyGenerator generator;

	public SECTR_AudioCue pressButtonCue;

	public GameObject pressButtonFX;

	[Tooltip("SFX played when the button is pressed and the generator cannot be activated. (optional)")]
	public SECTR_AudioCue onPressButtonFailureCue;

	private Component[] buttonRenderer;

	private Animator buttonAnimator;

	private int buttonPressedTriggerId;

	public Animator generatorAnimator;

	public void Awake()
	{
		buttonAnimator = GetComponentInParent<Animator>();
		buttonPressedTriggerId = Animator.StringToHash("ButtonPressed");
		Component[] componentsInChildren = base.transform.parent.gameObject.GetComponentsInChildren<Renderer>();
		buttonRenderer = componentsInChildren;
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Combine(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	public void OnDestroy()
	{
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Remove(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	private void OnGeneratorStateChanged()
	{
		if (pressButtonFX != null)
		{
			pressButtonFX.SetActive(generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE || generator.GetState() == QuicksilverEnergyGenerator.State.COUNTDOWN);
		}
		if (generatorAnimator != null)
		{
			generatorAnimator.SetBool("generatorState", generator.GetState() != QuicksilverEnergyGenerator.State.ACTIVE && generator.GetState() != QuicksilverEnergyGenerator.State.COUNTDOWN);
		}
		float value = (IsReady() ? 0.5f : 0f);
		Component[] array = buttonRenderer;
		for (int i = 0; i < array.Length; i++)
		{
			((Renderer)array[i]).material.SetFloat("_SpiralColor", value);
		}
	}

	public void Activate()
	{
		if (buttonAnimator != null)
		{
			buttonAnimator.SetTrigger(buttonPressedTriggerId);
		}
		if (IsReady())
		{
			SECTR_AudioSystem.Play(pressButtonCue, base.transform.position, loop: false);
			generator.Activate();
		}
		else
		{
			SECTR_AudioSystem.Play(onPressButtonFailureCue, base.transform.position, loop: false);
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		if (!IsReady())
		{
			GameObject obj = new GameObject("EmptyGameObject");
			Destroyer.Destroy(obj, "QuicksilverEnergyActivator.GetCustomGuiPrefab");
			return obj;
		}
		return null;
	}

	private bool IsReady()
	{
		return generator.GetState() == QuicksilverEnergyGenerator.State.INACTIVE;
	}
}
