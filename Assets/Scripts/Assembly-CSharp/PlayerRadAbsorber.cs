using UnityEngine;

public class PlayerRadAbsorber : SRBehaviour
{
	public SECTR_PointSource radAudio;

	private PlayerState playerState;

	private PlayerDamageable damageable;

	private bool absorbingThisFrame;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		damageable = GetComponent<PlayerDamageable>();
	}

	public void FixedUpdate()
	{
		SRSingleton<Overlay>.Instance.SetEnableRad(absorbingThisFrame);
		if (absorbingThisFrame && !radAudio.IsPlaying)
		{
			radAudio.Play();
		}
		else if (!absorbingThisFrame && radAudio.IsPlaying)
		{
			radAudio.Stop(stopImmediately: false);
		}
		absorbingThisFrame = false;
	}

	public void Absorb(GameObject source, float rads)
	{
		int num = playerState.AddRads(rads);
		if (num > 0 && damageable.Damage(num, null))
		{
			DeathHandler.Kill(base.gameObject, DeathHandler.Source.SLIME_RAD, source, "PlayerRadAbsorber.Absorb");
		}
		absorbingThisFrame = true;
	}
}
