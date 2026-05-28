using UnityEngine;

public class GadgetPortableSlimeBait : SRBehaviour
{
	[Tooltip("SFX played when the slime bait is hit.")]
	public SECTR_AudioCue onHitCue;

	private float nextHitTime;

	public void OnHit(Transform onHitTransform)
	{
		if (Time.time >= nextHitTime)
		{
			SECTR_AudioSystem.Play(onHitCue, onHitTransform.position, loop: false);
			nextHitTime = Time.time + 1f;
		}
	}
}
