using UnityEngine;

public class PlayerDamageable : SRBehaviour, Damageable
{
	private PlayerState playerState;

	private SECTR_AudioSource playerAudio;

	private ScreenShaker screenShaker;

	public SECTR_AudioCue damagedCue;

	private const float PER_DAMAGE_SCREEN_SHAKE = 0.2f;

	private void Start()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		playerAudio = GetComponent<SECTR_AudioSource>();
		screenShaker = GetComponent<ScreenShaker>();
	}

	public bool Damage(int healthLoss, GameObject source)
	{
		healthLoss = Mathf.RoundToInt((float)healthLoss * SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().playerDamageMultiplier);
		if (playerState.CanBeDamaged())
		{
			SRSingleton<Overlay>.Instance.PlayDamage();
			playerAudio.Cue = GetDamageCue(source);
			playerAudio.Play();
			screenShaker.ShakeDamage(0.2f * (float)healthLoss);
			return playerState.Damage(healthLoss, source);
		}
		return false;
	}

	private SECTR_AudioCue GetDamageCue(GameObject source)
	{
		if (source != null && Identifiable.GetId(source) == Identifiable.Id.GLITCH_TARR_SLIME)
		{
			return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.damageLossExposure.onExposedSFX;
		}
		return damagedCue;
	}
}
