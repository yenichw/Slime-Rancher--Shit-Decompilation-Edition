using System;
using UnityEngine;

public abstract class PooledConfigurableSceneParticle : PooledSceneParticle
{
	[Serializable]
	public class MinMaxGradientData
	{
		public ParticleSystemGradientMode mode;

		public Color color;

		public Gradient gradient;

		public Color colorMin;

		public Color colorMax;

		public Color gradientMin;

		public Color gradientMax;
	}

	protected override void InitParticle()
	{
		base.InitParticle();
		if (particle != null)
		{
			ConfigureParticles();
		}
	}

	protected abstract void ConfigureParticles();

	protected void SetColors(string relObjPath, MinMaxGradientData colorData)
	{
		Transform transform = ((relObjPath == null) ? particle.transform : particle.transform.Find(relObjPath));
		ParticleSystem particleSystem = ((transform == null) ? null : transform.GetComponent<ParticleSystem>());
		if (particleSystem != null)
		{
			SetStartColors(particleSystem, colorData);
		}
	}

	private static void SetStartColors(ParticleSystem part, MinMaxGradientData colorData)
	{
		ParticleSystem.MainModule main = part.main;
		ParticleSystem.MinMaxGradient startColor = null;
		switch (colorData.mode)
		{
		case ParticleSystemGradientMode.Color:
			startColor = new ParticleSystem.MinMaxGradient(colorData.color);
			break;
		case ParticleSystemGradientMode.Gradient:
			startColor = new ParticleSystem.MinMaxGradient(colorData.gradient);
			break;
		case ParticleSystemGradientMode.TwoColors:
			startColor = new ParticleSystem.MinMaxGradient(colorData.colorMin, colorData.colorMax);
			break;
		case ParticleSystemGradientMode.TwoGradients:
			startColor = new ParticleSystem.MinMaxGradient(colorData.gradientMin, colorData.gradientMax);
			break;
		case ParticleSystemGradientMode.RandomColor:
			startColor = new ParticleSystem.MinMaxGradient(colorData.gradient);
			break;
		}
		startColor.mode = colorData.mode;
		main.startColor = startColor;
	}
}
