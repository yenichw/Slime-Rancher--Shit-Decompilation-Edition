using UnityEngine;

public class FeralSlimeButtstomp : SlimeSubbehaviour, Collidable
{
	private enum Mode
	{
		WAITING = 0,
		MIDAIR = 1,
		WAIT_FOR_GROUND_IMPACT = 2,
		STOMPING = 3,
		LANDED = 4
	}

	public GameObject stompFX;

	public float explodePower = 600f;

	public float explodeRadius = 7f;

	public float minPlayerDamage = 15f;

	public float maxPlayerDamage = 45f;

	private Mode mode;

	private float nextStompTime;

	private SlimeFeral feral;

	private SlimeAudio slimeAudio;

	private GameModeConfig theGameModeConfig;

	private const float MAX_DIST = 20f;

	private const float MAX_DIST_SQR = 400f;

	private const float MIN_DIST = 5f;

	private const float MIN_DIST_SQR = 25f;

	private const float STOMP_RESET_TIME = 5f;

	private const float PLAYER_FORCE_FACTOR = 0.001f;

	private const float UNDERNEATH_THRESHOLD = 0.5f;

	public override void Awake()
	{
		base.Awake();
		feral = GetComponent<SlimeFeral>();
		slimeAudio = GetComponent<SlimeAudio>();
		theGameModeConfig = SRSingleton<SceneContext>.Instance.GameModeConfig;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (isGrounded && feral.IsFeral() && !theGameModeConfig.GetModeSettings().preventHostiles && Time.time >= nextStompTime)
		{
			float sqrMagnitude = (SlimeSubbehaviour.GetGotoPos(SRSingleton<SceneContext>.Instance.Player) - base.transform.position).sqrMagnitude;
			if (sqrMagnitude <= 400f && sqrMagnitude >= 25f)
			{
				return Randoms.SHARED.GetInRange(0.3f, 1f);
			}
		}
		return 0f;
	}

	public override bool CanRethink()
	{
		if (mode != 0)
		{
			return mode == Mode.LANDED;
		}
		return true;
	}

	public override void Selected()
	{
		mode = Mode.WAITING;
	}

	public override void Action()
	{
		switch (mode)
		{
		case Mode.WAITING:
			LaunchAction();
			break;
		case Mode.MIDAIR:
			MidairAction();
			break;
		case Mode.WAIT_FOR_GROUND_IMPACT:
			if (plexer.IsFloating())
			{
				mode = Mode.STOMPING;
			}
			break;
		case Mode.STOMPING:
			StompingAction();
			break;
		case Mode.LANDED:
			break;
		}
	}

	private void LaunchAction()
	{
		Vector3 vector = SRSingleton<SceneContext>.Instance.Player.transform.TransformPoint(new Vector3(0f, 0f, 2f)) - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		Vector3 normalized = vector.normalized;
		RotateTowards(normalized, 1f, 5f);
		float num = 1.2f;
		float num2 = 1.4f;
		float num3 = Mathf.Sqrt(Mathf.Sqrt(sqrMagnitude) * Physics.gravity.magnitude) * num * num2;
		slimeBody.AddForce((normalized + Vector3.up).normalized * num3, ForceMode.VelocityChange);
		slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
		slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
		slimeAudio.Play(slimeAudio.slimeSounds.stompJumpCue);
		mode = Mode.MIDAIR;
	}

	private void MidairAction()
	{
		if (slimeBody.velocity.y <= 0f)
		{
			float magnitude = slimeBody.velocity.magnitude;
			slimeBody.velocity = new Vector3(0f, 0f - magnitude, 0f);
			mode = Mode.WAIT_FOR_GROUND_IMPACT;
		}
	}

	private void StompingAction()
	{
		if (stompFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(stompFX, base.transform.position, base.transform.rotation);
		}
		slimeAudio.Play(slimeAudio.slimeSounds.stompLandCue);
		Explode();
		mode = Mode.LANDED;
		nextStompTime = Time.time + 5f;
	}

	private void Explode()
	{
		PhysicsUtil.Explode(base.gameObject, explodeRadius, explodePower, minPlayerDamage, maxPlayerDamage);
	}

	public void ProcessCollisionEnter(Collision col)
	{
		if (mode == Mode.WAIT_FOR_GROUND_IMPACT && base.transform.position.y - col.contacts[0].point.y >= 0.5f)
		{
			mode = Mode.STOMPING;
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}
}
