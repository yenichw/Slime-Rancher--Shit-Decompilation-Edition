using UnityEngine;

public class GlareAtPlayer : SlimeSubbehaviour
{
	public float glareTime = 5f;

	public float minGlareDelay = 5f;

	public float minGlareDistance;

	public float maxGlareDistance = 20f;

	private bool isGlaring;

	private SlimeFeral feral;

	private float nextGlareTime;

	private float stopGlareTime;

	public override void Awake()
	{
		base.Awake();
		feral = GetComponent<SlimeFeral>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (isGrounded && feral.IsFeral() && Time.time >= nextGlareTime)
		{
			float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(SRSingleton<SceneContext>.Instance.Player) - base.transform.position).sqrMagnitude;
			if (sqrMagnitude <= maxGlareDistance && sqrMagnitude >= minGlareDistance)
			{
				return Randoms.SHARED.GetInRange(0.3f, 1f);
			}
		}
		return 0f;
	}

	public override bool CanRethink()
	{
		return !isGlaring;
	}

	public override void Selected()
	{
		isGlaring = true;
		stopGlareTime = Time.time + glareTime;
	}

	public override void Action()
	{
		if (isGlaring && Time.time >= stopGlareTime)
		{
			isGlaring = false;
			nextGlareTime = Time.time + minGlareDelay;
			return;
		}
		Vector3 vector = SRSingleton<SceneContext>.Instance.Player.transform.TransformPoint(new Vector3(0f, 0f, 2f)) - base.transform.position;
		_ = vector.sqrMagnitude;
		Vector3 normalized = vector.normalized;
		RotateTowards(normalized, 1f, 5f);
	}
}
