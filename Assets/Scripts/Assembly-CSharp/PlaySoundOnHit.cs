using UnityEngine;

public class PlaySoundOnHit : MonoBehaviour, ControllerCollisionListener
{
	[Tooltip("The audio cue to play on hit")]
	public SECTR_AudioCue hitCue;

	[Tooltip("Minimum time between playing sound, in seconds.")]
	public float minTimeBetween = 1f;

	[Tooltip("Minimum force to trigger the sound.")]
	public float minForce;

	[Tooltip("Whether we should count controller collisions for whether we play the hit.")]
	public bool includeControllerCollisions;

	private float nextTime;

	public void OnCollisionEnter(Collision col)
	{
		if (col.impulse.sqrMagnitude >= minForce * minForce)
		{
			MaybePlaySound();
		}
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		if (includeControllerCollisions)
		{
			MaybePlaySound();
		}
	}

	private void MaybePlaySound()
	{
		if (Time.time >= nextTime)
		{
			if (hitCue != null)
			{
				SECTR_AudioSystem.Play(hitCue, base.transform.position, loop: false);
			}
			nextTime = Time.time + minTimeBetween;
		}
	}
}
