using UnityEngine;

public class TimerSpiral : MonoBehaviour
{
	private const float MAX_CHANGE_PER_FRAME = 0.004f;

	private VacDisplayTimer.TimeSource source;

	private Renderer renderer;

	private double ratio;

	public void Awake()
	{
		renderer = GetComponent<Renderer>();
		renderer.material.SetFloat("_Timer", 0f);
		ratio = 0.0;
	}

	public void SetTimeSource(VacDisplayTimer.TimeSource source)
	{
		this.source = source;
		double? num = source?.GetTimeRemaining();
		double? num2 = source?.GetMaxTimeRemaining();
		ratio = ((!num2.HasValue) ? 0.0 : (0.5 / num2.Value));
		renderer.material.SetFloat("_Timer", num.HasValue ? ((float)(ratio * num.Value)) : 0f);
	}

	public void SetWarningThreshold(float percentage)
	{
		renderer.material.SetFloat("_WarningThreshold", percentage);
	}

	public void Update()
	{
		if (ratio > 0.0 && source != null)
		{
			float @float = renderer.material.GetFloat("_Timer");
			float num = Mathf.Clamp01((float)(ratio * source.GetTimeRemaining().Value));
			renderer.material.SetFloat("_Timer", @float + Mathf.Max(-0.004f, Mathf.Min(0.004f, num - @float)));
		}
	}

	public void OnDestroy()
	{
		renderer.material.SetFloat("_Timer", 0f);
	}
}
