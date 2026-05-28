using System;
using UnityEngine;

public class VacDisplayTimer : MonoBehaviour
{
	public interface TimeSource
	{
		double? GetTimeRemaining();

		double? GetMaxTimeRemaining();

		double? GetWarningTimeSeconds();
	}

	[Tooltip("Text object to update with the time.")]
	public TimerText timer;

	[Tooltip("Renderer containing the color spiral.")]
	public Renderer renderer;

	private QuicksilverEnergyGenerator generator;

	private TimerSpiral spiral;

	private TimeSource source;

	public void Awake()
	{
		spiral = renderer.gameObject.AddComponent<TimerSpiral>();
		timer.UpdateTimeRemaining(null);
	}

	public void Update()
	{
		double? secondsRemaining = ((source == null) ? null : source.GetTimeRemaining());
		double? num = ((source == null) ? null : source.GetWarningTimeSeconds());
		timer.UpdateTimeRemaining(secondsRemaining);
		spiral.SetWarningThreshold((source == null) ? 0f : ((secondsRemaining.HasValue && num.HasValue) ? ((float)((!(secondsRemaining.Value >= num.Value)) ? 1 : 0)) : 0.2f));
	}

	public void OnDestroy()
	{
		SetTimeSource(null);
	}

	public void SetQuicksilverEnergyGenerator(QuicksilverEnergyGenerator generator)
	{
		if (this.generator != null)
		{
			QuicksilverEnergyGenerator quicksilverEnergyGenerator = this.generator;
			quicksilverEnergyGenerator.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Remove(quicksilverEnergyGenerator.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnQuicksilverEnergyGeneratorStateChanged));
		}
		SetTimeSource(generator);
		this.generator = generator;
		if (this.generator != null)
		{
			QuicksilverEnergyGenerator quicksilverEnergyGenerator2 = this.generator;
			quicksilverEnergyGenerator2.onStateChanged = (QuicksilverEnergyGenerator.OnStateChanged)Delegate.Combine(quicksilverEnergyGenerator2.onStateChanged, new QuicksilverEnergyGenerator.OnStateChanged(OnQuicksilverEnergyGeneratorStateChanged));
			OnQuicksilverEnergyGeneratorStateChanged();
		}
	}

	private void OnQuicksilverEnergyGeneratorStateChanged()
	{
		SetTimeSource((generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE || generator.GetState() == QuicksilverEnergyGenerator.State.COUNTDOWN) ? generator : null);
	}

	public void SetTimeSource(TimeSource source)
	{
		this.source = source;
		spiral.SetTimeSource(source);
	}
}
