using UnityEngine;

public class ChickenRandomMove : SlimeSubbehaviour, RegistryUpdateable
{
	private enum Mode
	{
		JUMP = 0,
		PECK = 1,
		WALK = 2,
		WAIT = 3
	}

	public float maxJump = 1f;

	public float walkForwardForce = 0.05f;

	public SECTR_AudioCue flapCue;

	private const float JUMP_PROB = 0.5f;

	private const float JUMP_TORQUE = 0.2f;

	private SlimeAudio slimeAudio;

	private Mode mode;

	private float nextModeChoice;

	private Animator animator;

	private int animGroundedId;

	private int animWalkId;

	private int animPeckId;

	private const float MAX_VEL_TO_BOUNCE = 0.1f;

	private const float SQR_MAX_VEL_TO_BOUNCE = 0.010000001f;

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
		animator = GetComponentInChildren<Animator>();
		animGroundedId = Animator.StringToHash("grounded");
		animWalkId = Animator.StringToHash("walk");
		animPeckId = Animator.StringToHash("peck");
	}

	public override void Start()
	{
		base.Start();
	}

	public override float Relevancy(bool isGrounded)
	{
		return 0.2f;
	}

	public override void Selected()
	{
		SelectMode();
	}

	private void SelectMode()
	{
		mode = ((!Randoms.SHARED.GetProbability(0.5f)) ? (Randoms.SHARED.GetChance(2) ? Mode.PECK : Mode.WALK) : Mode.JUMP);
		nextModeChoice = Time.time + 1f;
	}

	public override void Action()
	{
		if (Time.fixedTime >= nextModeChoice)
		{
			SelectMode();
		}
		if (IsGrounded())
		{
			if (mode == Mode.JUMP)
			{
				if (slimeBody.velocity.sqrMagnitude <= 0.010000001f)
				{
					float min = 0.5f * maxJump * slimeBody.mass;
					slimeBody.AddForce(0f, Random.Range(min, maxJump), 0f, ForceMode.Impulse);
					slimeBody.AddTorque(0f, Random.Range(-0.2f, 0.2f), 0f, ForceMode.Impulse);
					slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
					slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
					mode = Mode.WAIT;
				}
			}
			else if (mode != Mode.PECK && mode == Mode.WALK)
			{
				float num = 1f;
				slimeBody.AddForce(base.transform.forward * (walkForwardForce * slimeBody.mass * num * Time.fixedDeltaTime), ForceMode.Impulse);
				Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
				slimeBody.AddForceAtPosition(base.transform.forward * (2f * walkForwardForce * slimeBody.mass * num * Time.fixedDeltaTime), position, ForceMode.Impulse);
			}
		}
		animator.SetBool(animWalkId, mode == Mode.WALK);
		animator.SetBool(animPeckId, mode == Mode.PECK);
	}

	public void RegistryUpdate()
	{
		bool flag = IsGrounded();
		if (animator.GetBool(animGroundedId) && !flag)
		{
			slimeAudio.Play(flapCue);
		}
		animator.SetBool(animGroundedId, flag);
	}
}
