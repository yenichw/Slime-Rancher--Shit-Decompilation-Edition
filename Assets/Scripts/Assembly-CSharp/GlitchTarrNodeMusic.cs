using Assets.Script.Util.Extensions;
using UnityEngine;

public class GlitchTarrNodeMusic : SECTR_PointSource
{
	private const int MIN_DISTANCE = 10;

	private const int MAX_DISTANCE = 20;

	private float minDistance;

	private float maxDistance;

	private float maxVolume;

	protected override void Start()
	{
		base.Start();
		if (Application.isPlaying)
		{
			maxVolume = Random.Range(Cue.Volume.x, Cue.Volume.y);
			GlitchTarrNode requiredComponentInParent = base.gameObject.GetRequiredComponentInParent<GlitchTarrNode>();
			minDistance = 10f * requiredComponentInParent.scale.x;
			maxDistance = 20f * requiredComponentInParent.scale.x;
			minDistance *= minDistance;
			maxDistance *= maxDistance;
		}
	}

	protected void Update()
	{
		if (Application.isPlaying && IsPlaying)
		{
			instance.Volume = maxVolume * GetCurrentMultiplier();
		}
	}

	private float GetCurrentMultiplier()
	{
		float sqrMagnitude = (SRSingleton<SceneContext>.Instance.Player.transform.position - base.transform.position).sqrMagnitude;
		if (sqrMagnitude <= minDistance)
		{
			return 1f;
		}
		if (sqrMagnitude >= maxDistance)
		{
			return 0f;
		}
		return 1f - (sqrMagnitude - minDistance) / (maxDistance - minDistance);
	}
}
