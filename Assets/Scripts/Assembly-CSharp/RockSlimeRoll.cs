using UnityEngine;

public class RockSlimeRoll : SlimeSubbehaviour
{
	private static float ROLL_MIN_DELAY = 3f;

	private static float ROLL_MAX_DELAY = 15f;

	private SlimeAudio slimeAudio;

	private CalmedByWaterSpray calmed;

	private float nextRollTime;

	private Randoms rand;

	private Vector3 rollAxis;

	private Vector3 forward;

	private float spinTime;

	private float endTime;

	private int animRockModeId;

	private const float SPIN_TIME = 1f;

	private const float ROLL_TIME = 2f;

	private const float MAX_VEL_TO_ROLL = 0.1f;

	private const float SQR_MAX_VEL_TO_ROLL = 0.010000001f;

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
		calmed = GetComponent<CalmedByWaterSpray>();
		rand = new Randoms();
		animRockModeId = Animator.StringToHash("RockMode");
	}

	public override void Start()
	{
		base.Start();
		nextRollTime = Time.fixedTime + RollDelay();
	}

	public override bool CanRethink()
	{
		return Time.time >= endTime;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (nextRollTime <= Time.fixedTime && !calmed.IsCalmed() && isGrounded)
		{
			rollAxis = base.transform.right;
			rollAxis.y = 0f;
			rollAxis.Normalize();
			forward = Vector3.Cross(rollAxis, Vector3.up);
			return 0.3f;
		}
		return 0f;
	}

	public override void Selected()
	{
		GetComponentInChildren<Animator>().SetBool(animRockModeId, value: true);
		spinTime = Time.fixedTime + 1f;
		endTime = spinTime + 2f;
		nextRollTime = endTime + RollDelay();
		slimeAudio.Play(slimeAudio.slimeSounds.rollCue);
	}

	public void FixedUpdate()
	{
		if (calmed.IsCalmed())
		{
			nextRollTime += Time.fixedDeltaTime;
		}
	}

	private float RollDelay()
	{
		return Mathf.Lerp(ROLL_MIN_DELAY, ROLL_MAX_DELAY, Mathf.Clamp(rand.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f));
	}

	public override void Deselected()
	{
		base.Deselected();
		GetComponentInChildren<Animator>().SetBool(animRockModeId, value: false);
	}

	public override void Action()
	{
		if (IsGrounded() && Time.time > spinTime)
		{
			slimeBody.AddTorque(rollAxis * (1200f * slimeBody.mass * Time.fixedDeltaTime));
			slimeBody.AddForce(forward * (720f * slimeBody.mass * Time.fixedDeltaTime));
		}
	}
}
