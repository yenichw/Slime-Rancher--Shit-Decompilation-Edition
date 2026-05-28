using System;
using UnityEngine;

public class SENBDLGlowingOrbitingCube : MonoBehaviour
{
	private float pulseSpeed;

	private float phase;

	private Vector3 Vec3(float x)
	{
		return new Vector3(x, x, x);
	}

	private void Start()
	{
		base.transform.localScale = Vec3(1.5f);
		pulseSpeed = UnityEngine.Random.Range(4f, 8f);
		phase = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
	}

	private void Update()
	{
		Color glowColor = SENBDLGlobal.mainCube.glowColor;
		glowColor.r = 1f - glowColor.r;
		glowColor.g = 1f - glowColor.g;
		glowColor.b = 1f - glowColor.b;
		glowColor = Color.Lerp(glowColor, Color.white, 0.1f);
		glowColor *= Mathf.Pow(Mathf.Sin(Time.time * pulseSpeed + phase) * 0.49f + 0.51f, 2f);
		GetComponent<Renderer>().material.SetColor("_EmissionColor", glowColor);
		GetComponent<Light>().color = glowColor;
	}
}
