using UnityEngine;

public class ReflectionProbeRefreshTimer : MonoBehaviour
{
	public float delayBetweenUpdateSecs = 10f;

	private ReflectionProbe rProbe;

	private float nextUpdateTime;

	public void Start()
	{
		rProbe = GetComponent<ReflectionProbe>();
	}

	public void Update()
	{
		if (Time.time >= nextUpdateTime)
		{
			nextUpdateTime = Time.time + delayBetweenUpdateSecs;
			rProbe.RenderProbe();
		}
	}
}
