using System;
using DG.Tweening;
using UnityEngine;

public class GlitchTerminalAnimator_Lights : SRBehaviour
{
	private enum State
	{
		DISABLED = 0,
		ENABLED_RED = 1,
		ENABLED_GREEN = 2
	}

	private GlitchTerminalAnimator animator;

	private const float TRANSITION_SPEED = 0.8f;

	private readonly int PROPERTY_COLOR = Shader.PropertyToID("_SpiralColor");

	private const float PROPERTY_COLOR_RED = 0f;

	private const float PROPERTY_COLOR_GREEN = 0.5f;

	private readonly int PROPERTY_MULTIPLIER = Shader.PropertyToID("_GlowMultiplier");

	private const float PROPERTY_MULTIPLIER_ON = 1.25f;

	private const float PROPERTY_MULTIPLIER_OFF = 0f;

	private State state;

	private Renderer renderer;

	private Tweener multiplierTween;

	private Tweener colorTween;

	public void Awake()
	{
		animator = GetRequiredComponentInParent<GlitchTerminalAnimator>();
		renderer = GetRequiredComponent<Renderer>();
		renderer.sharedMaterial.SetFloat(PROPERTY_COLOR, 0f);
		renderer.sharedMaterial.SetFloat(PROPERTY_MULTIPLIER, 0f);
	}

	public void Update()
	{
		State currentState = GetCurrentState();
		if (state != currentState)
		{
			OnStateChanged(state, currentState);
			state = currentState;
		}
	}

	private State GetCurrentState()
	{
		if (!animator.animator.GetBool("state_sleeping"))
		{
			switch (animator.activator.GetLinkState())
			{
			case GlitchTerminalActivator.LinkState.INACTIVE_PROGRESS:
				return State.DISABLED;
			case GlitchTerminalActivator.LinkState.INACTIVE_AMMO:
				return State.ENABLED_RED;
			case GlitchTerminalActivator.LinkState.PRE_ACTIVE:
			case GlitchTerminalActivator.LinkState.ACTIVE:
				return State.ENABLED_GREEN;
			}
		}
		return State.DISABLED;
	}

	private void OnStateChanged(State previous, State current)
	{
		if (previous == State.DISABLED)
		{
			renderer.sharedMaterial.SetFloat(PROPERTY_COLOR, GetStateColor(current));
			multiplierTween?.Kill();
			multiplierTween = DOTween.To(() => renderer.sharedMaterial.GetFloat(PROPERTY_MULTIPLIER), OnUpdate_PropertyMultiplier, 1.25f, 0.8f).SetSpeedBased().SetEase(Ease.Linear);
		}
		else if (current == State.DISABLED)
		{
			multiplierTween?.Kill();
			multiplierTween = DOTween.To(() => renderer.sharedMaterial.GetFloat(PROPERTY_MULTIPLIER), OnUpdate_PropertyMultiplier, 0f, 0.8f).SetSpeedBased().SetEase(Ease.Linear);
		}
		else
		{
			colorTween?.Kill();
			colorTween = DOTween.To(() => renderer.sharedMaterial.GetFloat(PROPERTY_COLOR), OnUpdate_PropertyColor, GetStateColor(current), 0.8f).SetSpeedBased().SetEase(Ease.Linear);
		}
	}

	private void OnUpdate_PropertyMultiplier(float value)
	{
		renderer.sharedMaterial.SetFloat(PROPERTY_MULTIPLIER, value);
	}

	private void OnUpdate_PropertyColor(float value)
	{
		renderer.sharedMaterial.SetFloat(PROPERTY_COLOR, value);
	}

	private static float GetStateColor(State state)
	{
		switch (state)
		{
		case State.ENABLED_GREEN:
			return 0.5f;
		case State.ENABLED_RED:
			return 0f;
		default:
			throw new ArgumentException();
		}
	}
}
