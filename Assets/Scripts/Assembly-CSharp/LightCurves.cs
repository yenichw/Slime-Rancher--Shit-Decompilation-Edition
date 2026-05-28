using UnityEngine;

public class LightCurves : MonoBehaviour
{
	public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphScaleX = 1f;

	public float GraphScaleY = 1f;

	private float startTime;

	private Light lightSource;

	private void Start()
	{
		lightSource = GetComponent<Light>();
	}

	private void OnEnable()
	{
		startTime = Time.time;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (num <= GraphScaleX)
		{
			float intensity = LightCurve.Evaluate(num / GraphScaleX) * GraphScaleY;
			lightSource.intensity = intensity;
		}
	}
}
