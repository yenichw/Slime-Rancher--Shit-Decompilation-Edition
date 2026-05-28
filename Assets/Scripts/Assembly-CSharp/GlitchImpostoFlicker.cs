using System.Linq;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using UnityEngine;

public class GlitchImpostoFlicker : SRBehaviour
{
	private TimeDirector timeDirector;

	private GlitchMetadata metadata;

	private bool hasStarted;

	private Tween tween;

	private Vector3[] path_cache;

	private Vector3[] FlickerPath
	{
		get
		{
			if (path_cache == null)
			{
				path_cache = (from ii in Enumerable.Range(0, metadata.impostoFlickerPoints)
					select Quaternion.Euler(0f, Randoms.SHARED.GetInRange(0, 360), 0f) * Vector3.forward * metadata.impostoFlickerRadius.GetRandom() + base.gameObject.transform.position).Concat(base.gameObject.transform.position.ToEnumerable()).ToArray();
			}
			return path_cache;
		}
	}

	public void Awake()
	{
		if (Application.isPlaying)
		{
			metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
			timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		}
	}

	public void OnEnable()
	{
		if (Application.isPlaying && hasStarted)
		{
			ResetNextFlickerTime();
		}
	}

	public void Start()
	{
		if (Application.isPlaying)
		{
			hasStarted = true;
			OnEnable();
		}
	}

	public void OnDisable()
	{
		if (Application.isPlaying)
		{
			timeDirector.RemovePassedTimeDelegate(OnTimeReached);
		}
	}

	private void ResetNextFlickerTime()
	{
		timeDirector.AddPassedTimeDelegate(timeDirector.HoursFromNow(metadata.impostoFlickerCooldownTime.GetRandom() * (1f / 60f)), OnTimeReached);
	}

	private void OnTimeReached()
	{
		if (metadata.impostoFlickerFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(metadata.impostoFlickerFX, base.gameObject);
		}
		if (metadata.impostoFlickerCue != null)
		{
			SECTR_AudioSystem.Play(metadata.impostoFlickerCue, base.transform.position, loop: false);
		}
		tween?.Kill(complete: true);
		tween = base.transform.DOPath(FlickerPath, metadata.impostoFlickerSpeed.GetRandom()).SetEase(Ease.Linear).SetSpeedBased(isSpeedBased: true);
		ResetNextFlickerTime();
	}
}
