using UnityEngine;

public class PooledSceneParticle : SRBehaviour
{
	public GameObject particlePrefab;

	[Tooltip("Any animators we need to inform that we've messed with the hierarchy")]
	public Animator[] animsToRebind;

	protected GameObject particle;

	private bool initialized;

	private bool isShuttingDown;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.SceneParticleDirector.AddSecondFrameListener(this);
	}

	public void OnEnable()
	{
		if (initialized)
		{
			InitParticle();
		}
	}

	public void OnSecondFrame()
	{
		initialized = true;
		if (base.isActiveAndEnabled)
		{
			InitParticle();
		}
	}

	protected virtual void InitParticle()
	{
		if (particlePrefab != null && particle == null)
		{
			particle = SRBehaviour.SpawnAndPlayFX(particlePrefab, base.gameObject);
			Animator[] array = animsToRebind;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Rebind();
			}
		}
	}

	public void OnApplicationQuit()
	{
		isShuttingDown = true;
	}

	public void OnDisable()
	{
		if (particle != null && !isShuttingDown)
		{
			if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.fxPool != null)
			{
				SRSingleton<SceneContext>.Instance.fxPool.RecycleAfterFrame(particle);
				particle = null;
			}
			Animator[] array = animsToRebind;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Rebind();
			}
		}
	}

	public ParticleSystem GetParticleSystem()
	{
		if (!(particle != null))
		{
			return null;
		}
		return particle.GetComponent<ParticleSystem>();
	}
}
