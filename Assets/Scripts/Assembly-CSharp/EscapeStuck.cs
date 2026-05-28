using UnityEngine;

public class EscapeStuck : SlimeSubbehaviour
{
	private float verticalFactor = 0.5f;

	private float maxJump = 4f;

	private float? stuckSince;

	private SlimeAudio slimeAudio;

	private const float MIN_TIME_TO_ACT = 2f;

	private const float STUCK_VEL = 0.01f;

	private const float STUCK_VEL_SQR = 0.0001f;

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!isGrounded && slimeBody.velocity.sqrMagnitude < 0.0001f)
		{
			if (!stuckSince.HasValue)
			{
				stuckSince = Time.time;
			}
		}
		else
		{
			stuckSince = null;
		}
		if (stuckSince.HasValue && Time.time - stuckSince.Value >= 2f)
		{
			return 1f;
		}
		return 0f;
	}

	public override void Selected()
	{
	}

	public override void Action()
	{
		if (stuckSince.HasValue)
		{
			float num = 0.5f * maxJump * slimeBody.mass;
			slimeBody.AddForce(Random.Range(0f - num, num), verticalFactor * Random.Range(num, maxJump), Random.Range(0f - num, num), ForceMode.Impulse);
			slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
			slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
			stuckSince = null;
		}
	}
}
