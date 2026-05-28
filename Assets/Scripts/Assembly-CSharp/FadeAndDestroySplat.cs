using UnityEngine;

public class FadeAndDestroySplat : MonoBehaviour
{
	public float timeBeforeFade = 5f;

	public float minQualTimeBeforeFade = 1f;

	public float fadeTime = 1f;

	private float fadeInTime = 0.125f;

	public Texture2D[] textures;

	public GameObject splatFX;

	[Tooltip("SFX played when the fade begins.")]
	public SECTR_AudioCue onFadeBeginCue;

	private bool hasBegunFade;

	private float fadeStartTime;

	private float fadeEndTime;

	private float invFadeTime;

	private float fadeInStartTime;

	private float fadeInEndTime;

	private Projector projector;

	private const float BASE_ORTHO_SIZE = 0.5f;

	private Material mat;

	public void Awake()
	{
		projector = GetComponentInChildren<Projector>();
		mat = GetMaterial();
		float num = TimeForParticlesLevel(SRQualitySettings.Particles);
		fadeStartTime = Time.time + num;
		fadeEndTime = fadeStartTime + fadeTime;
		fadeInStartTime = Time.time;
		fadeInEndTime = fadeInStartTime + fadeInTime;
		invFadeTime = 1f / fadeTime;
		if (textures.Length != 0)
		{
			mat.SetTexture("_DecalTex", Randoms.SHARED.Pick(textures));
			mat.SetFloat("_Alpha", 0f);
		}
	}

	private float TimeForParticlesLevel(SRQualitySettings.ParticlesLevel level)
	{
		switch (level)
		{
		case SRQualitySettings.ParticlesLevel.LOW:
			return minQualTimeBeforeFade;
		case SRQualitySettings.ParticlesLevel.MEDIUM:
			return (timeBeforeFade + minQualTimeBeforeFade) * 0.5f;
		case SRQualitySettings.ParticlesLevel.HIGH:
			return timeBeforeFade;
		default:
			Log.Warning("Unknown particles level: " + level);
			return minQualTimeBeforeFade;
		}
	}

	public void Update()
	{
		float time = Time.time;
		float num = 1f;
		if (mat == null)
		{
			Log.Error("Updating splat for destroyed material.");
			SentrySdk.CaptureMessage("Attempting to update splat with destroyed material!");
			SRSingleton<SceneContext>.Instance.fxPool.Recycle(base.gameObject);
			return;
		}
		if (mat.shader == null)
		{
			Log.Error("Updating splat for material with destroyed shader.");
			SentrySdk.CaptureMessage("Attempting to update splat with destroyed shader!");
			SRSingleton<SceneContext>.Instance.fxPool.Recycle(base.gameObject);
			return;
		}
		if (time <= fadeInEndTime)
		{
			num = Mathf.Lerp(0f, 1f, (time - fadeInStartTime) / fadeInTime);
			mat.SetFloat("_Alpha", num);
		}
		else if (time <= fadeStartTime)
		{
			num = 1f;
			mat.SetFloat("_Alpha", num);
		}
		if (time >= fadeEndTime)
		{
			SRSingleton<SceneContext>.Instance.fxPool.Recycle(base.gameObject);
		}
		else if (time > fadeStartTime)
		{
			num = 1f - (time - fadeStartTime) * invFadeTime;
			mat.SetFloat("_Alpha", num);
			if (!hasBegunFade)
			{
				SECTR_AudioSystem.Play(onFadeBeginCue, base.transform.position, loop: false);
				hasBegunFade = true;
			}
		}
	}

	protected Material GetMaterial()
	{
		Material material = Object.Instantiate(projector.material);
		projector.material = material;
		return material;
	}

	public void SetScale(float scale)
	{
		projector.orthographicSize = scale * 0.5f;
	}

	public void SetColors(Color topColor, Color midColor, Color btmColor)
	{
		mat.SetColor("_TopColor", topColor);
		mat.SetColor("_MiddleColor", midColor);
		mat.SetColor("_BottomColor", btmColor);
	}

	public void OnDestroy()
	{
		Destroyer.Destroy(mat, "FadeAndDestroySplat.OnDestroy");
	}
}
