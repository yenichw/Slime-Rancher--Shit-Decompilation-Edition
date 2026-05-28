using UnityEngine;

public class SlimeFlee : SlimeSubbehaviour
{
	public GameObject disappearFX;

	public float facingStability = 1f;

	public float facingSpeed = 5f;

	public float fleeSpeedFactor = 1f;

	public SECTR_AudioCue fleeCue;

	protected TimeDirector timeDir;

	protected Vector3? fleeDir { get; private set; }

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void StartFleeing(GameObject fleeFrom)
	{
		SetFleeDirection(base.transform.position - SlimeSubbehaviour.GetGotoPos(fleeFrom));
		plexer.ForceRethink();
	}

	protected void SetFleeDirection(Vector3 direction)
	{
		direction.y = 0f;
		fleeDir = direction.normalized;
	}

	public bool IsFleeing()
	{
		return fleeDir.HasValue;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (fleeDir.HasValue)
		{
			return 1f;
		}
		return 0f;
	}

	public override void Selected()
	{
		if (fleeCue != null)
		{
			SlimeAudio component = GetComponent<SlimeAudio>();
			if (component != null)
			{
				component.Play(fleeCue);
			}
		}
	}

	public override void Action()
	{
		if (fleeDir.HasValue)
		{
			if (plexer.IsBlocked(null, fleeDir.Value, 0, forceCheckFullDist: false))
			{
				SRBehaviour.SpawnAndPlayFX(disappearFX, base.transform.position, base.transform.rotation);
				Destroyer.DestroyActor(base.gameObject, "SlimeFlee.Action");
			}
			else
			{
				MoveTowards(fleeDir.Value);
			}
		}
	}

	protected void MoveTowards(Vector3 dirToTarget)
	{
		RotateTowards(dirToTarget, facingSpeed, facingStability);
		slimeBody.AddForce(dirToTarget * (300f * slimeBody.mass * fleeSpeedFactor * Time.fixedDeltaTime));
		Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
		slimeBody.AddForceAtPosition(dirToTarget * (540f * slimeBody.mass * Time.fixedDeltaTime), position);
	}
}
