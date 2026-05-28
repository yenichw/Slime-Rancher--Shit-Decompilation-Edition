using System;
using UnityEngine;

public class SlimeStealth : RegisteredActorBehaviour, RegistryUpdateable, SpawnListener
{
	private const float STEALTH_INIT_TIME = 5f;

	private const float OPACITY_CHANGE_PER_SEC = 2f;

	private const float STEALTH_OPACITY = 0f;

	private const float OPACITY_CHANGE_TOLERANCE = 0.001f;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private Vacuumable vacuumable;

	private SlimeAudio slimeAudio;

	private MaterialStealthController stealthController;

	private float initStealthUntil;

	private float currentOpacity = 1f;

	private float targetOpacity = 1f;

	private float lastOpacity = 1f;

	public bool IsStealthed => currentOpacity < 1f;

	public void Awake()
	{
		stealthController = new MaterialStealthController(base.gameObject);
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		vacuumable = GetComponent<Vacuumable>();
		slimeAudio = GetComponent<SlimeAudio>();
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateMaterialStealthController();
		}
		if (slimeAppearanceApplicator != null)
		{
			slimeAppearanceApplicator.OnAppearanceChanged += delegate
			{
				UpdateMaterialStealthController();
			};
		}
	}

	public void RegistryUpdate()
	{
		UpdateStealthOpacity();
	}

	public void DidSpawn()
	{
		currentOpacity = 0f;
		initStealthUntil = Time.time + 5f;
	}

	public void SetStealth(bool stealth)
	{
		targetOpacity = (stealth ? 0f : 1f);
		slimeAudio.Play(stealth ? slimeAudio.slimeSounds.cloakCue : slimeAudio.slimeSounds.decloakCue);
	}

	private void SetOpacity(float opacity)
	{
		stealthController.SetOpacity(opacity);
		lastOpacity = opacity;
	}

	private void UpdateMaterialStealthController()
	{
		stealthController.UpdateMaterials(base.gameObject);
		lastOpacity = 1f;
	}

	private void UpdateStealthOpacity()
	{
		float num = ((Time.time < initStealthUntil) ? 0f : targetOpacity);
		if (num > currentOpacity)
		{
			currentOpacity = Mathf.Min(num, currentOpacity + 2f * Time.deltaTime);
		}
		else if (targetOpacity < currentOpacity)
		{
			currentOpacity = Mathf.Max(num, currentOpacity - 2f * Time.deltaTime);
		}
		float num2 = (vacuumable.isHeld() ? 1f : currentOpacity);
		if (Math.Abs(num2 - lastOpacity) > 0.001f)
		{
			SetOpacity(num2);
		}
	}
}
