using System;
using UnityEngine;

public class QuicksilverEnergyCheckpoint : MonoBehaviour
{
	[Tooltip("Energy generator to extend.")]
	public QuicksilverEnergyGenerator generator;

	[Tooltip("Time, in game hours, this checkpoint will extend the energy timer.")]
	public float extensionHours;

	[Tooltip("Time, in game hours, this checkpoint is on cooldown after use.")]
	public float cooldownHours;

	[Tooltip("FX to display when the checkpoint is available. (optional)")]
	public GameObject activeFX;

	[Tooltip("SFX played when the checkpoint is triggered by the player.")]
	public SECTR_AudioCue onPickupCue;

	[Tooltip("FX played when the checkpoint is triggered by the player. (optional)")]
	public GameObject onPickupFX;

	private Renderer padRenderer;

	public GameObject padObject;

	private double cooldown;

	private bool? wasReady;

	private TimeDirector timeDirector;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Combine(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
		if (padObject != null)
		{
			padRenderer = padObject.GetComponent<Renderer>();
		}
	}

	public void OnDestroy()
	{
		QuicksilverEnergyGenerator quicksilverEnergyGenerator = generator;
		quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Remove(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnGeneratorStateChanged));
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider) && timeDirector.HasReached(cooldown) && generator.ExtendActiveDuration(extensionHours))
		{
			SECTR_AudioSystem.Play(onPickupCue, base.transform.position, loop: false);
			cooldown = timeDirector.HoursFromNow(cooldownHours);
			onPickupFX.SetActive(value: true);
		}
	}

	public void Update()
	{
		bool flag = IsReady();
		if (!wasReady.HasValue || wasReady != flag)
		{
			if (activeFX != null)
			{
				activeFX.SetActive(flag);
			}
			if (padRenderer != null)
			{
				float value = (flag ? 0.5f : 0f);
				padRenderer.material.SetFloat("_SpiralColor", value);
			}
			wasReady = flag;
		}
	}

	private void OnGeneratorStateChanged()
	{
		if (generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE)
		{
			onPickupFX.SetActive(value: false);
			cooldown = 0.0;
		}
	}

	private bool IsReady()
	{
		if (generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE)
		{
			return timeDirector.HasReached(cooldown);
		}
		return false;
	}
}
