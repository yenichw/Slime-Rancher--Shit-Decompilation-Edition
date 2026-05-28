using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShuriken : MonoBehaviour
{
	public bool OnlyDeactivate;

	public bool RecycleOnCompletion;

	public bool RecycleParent;

	private float nextCheckTime;

	private float endTime;

	private const float CHECK_DELAY = 0.5f;

	private ParticlesRunWhilePaused particlesRunWhilePaused;

	private const float LIFETIME_SAFETY_MARGIN = 1.5f;

	public void Awake()
	{
		particlesRunWhilePaused = GetComponent<ParticlesRunWhilePaused>();
	}

	public void OnEnable()
	{
		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		endTime = (main.loop ? float.PositiveInfinity : (GetTime() + main.duration * 1.5f));
	}

	private float GetTime()
	{
		if (particlesRunWhilePaused != null && particlesRunWhilePaused.enabled)
		{
			return Time.unscaledTime;
		}
		return Time.time;
	}

	public void Update()
	{
		float time = GetTime();
		if (nextCheckTime > time)
		{
			return;
		}
		if (endTime <= GetTime() || !GetComponent<ParticleSystem>().IsAlive(withChildren: true))
		{
			if (OnlyDeactivate)
			{
				base.gameObject.SetActive(value: false);
			}
			else if (RecycleOnCompletion)
			{
				if (RecycleParent)
				{
					SRSingleton<SceneContext>.Instance.fxPool.Recycle(base.transform.parent.gameObject);
				}
				else
				{
					SRSingleton<SceneContext>.Instance.fxPool.Recycle(base.gameObject);
				}
			}
			else
			{
				Destroyer.Destroy(base.gameObject, "CFX_AutoDestructShuriken.Update");
			}
		}
		else
		{
			nextCheckTime = time + 0.5f;
		}
	}
}
