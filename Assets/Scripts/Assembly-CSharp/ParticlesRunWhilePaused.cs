using UnityEngine;

public class ParticlesRunWhilePaused : MonoBehaviour
{
	private ParticleSystem uiParticleSystem;

	public void Awake()
	{
		uiParticleSystem = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		if (Time.timeScale < 0.01f)
		{
			uiParticleSystem.Simulate(Time.unscaledDeltaTime, withChildren: false, restart: false);
			uiParticleSystem.Play();
		}
	}
}
