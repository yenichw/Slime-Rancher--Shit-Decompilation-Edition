using UnityEngine;

public class PuddleSlimeScoot : SlimeSubbehaviour
{
	private enum Mode
	{
		SCOOT = 0,
		TURN = 1,
		RICOCHET = 2
	}

	public float straightlineForceFactor = 1f;

	public bool canRicochet;

	private Mode mode;

	private float turnTorque;

	private float nextModeChoice;

	private Vector3 ricochetDir;

	private const float TURN_PROB = 0.2f;

	private const float MIN_TURN_TORQUE = 1f;

	private const float MAX_TURN_TORQUE = 2f;

	public override void Awake()
	{
		base.Awake();
		Collider[] components = GetComponents<Collider>();
		for (int i = 0; i < components.Length && components[i].isTrigger; i++)
		{
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override bool Forbids(SlimeSubbehaviour toMaybeForbid)
	{
		return toMaybeForbid is SlimeRandomMove;
	}

	public override float Relevancy(bool isGrounded)
	{
		return 0.2f;
	}

	public override void Selected()
	{
		SelectMode();
	}

	public override void Deselected()
	{
		base.Deselected();
	}

	private void SelectMode()
	{
		if (canRicochet && IsBlocked(null))
		{
			mode = Mode.RICOCHET;
			ricochetDir = -base.transform.forward;
		}
		else
		{
			mode = (Randoms.SHARED.GetProbability(0.2f) ? Mode.TURN : Mode.SCOOT);
		}
		nextModeChoice = Time.fixedTime + 1f;
		if (mode == Mode.TURN)
		{
			turnTorque = (float)(Randoms.SHARED.GetBoolean() ? 1 : (-1)) * Randoms.SHARED.GetInRange(1f, 2f);
		}
		else
		{
			turnTorque = 0f;
		}
	}

	public override void Action()
	{
		if (Time.fixedTime >= nextModeChoice)
		{
			SelectMode();
		}
		if (IsGrounded())
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if (mode == Mode.RICOCHET)
			{
				RotateTowards(ricochetDir, 5f, 1f);
				component.AddForce(ricochetDir * (80f * straightlineForceFactor * component.mass * Time.fixedDeltaTime));
				Vector3 position = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
				component.AddForceAtPosition(ricochetDir * (240f * straightlineForceFactor * component.mass * Time.fixedDeltaTime), position);
			}
			else if (mode == Mode.TURN)
			{
				float num = (IsFloating() ? 0.2f : 1f);
				component.AddForce(base.transform.forward * (-80f * num * component.mass * Time.fixedDeltaTime));
				Vector3 position2 = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
				component.AddForceAtPosition(base.transform.forward * (-240f * num * component.mass * Time.fixedDeltaTime), position2);
				component.AddTorque(0f, turnTorque * Time.fixedDeltaTime, 0f);
			}
			else
			{
				component.AddForce(base.transform.forward * (straightlineForceFactor * 80f * component.mass * Time.fixedDeltaTime));
				Vector3 position3 = base.transform.position + Vector3.down * (0.5f * base.transform.localScale.y);
				component.AddForceAtPosition(base.transform.forward * (straightlineForceFactor * 240f * component.mass * Time.fixedDeltaTime), position3);
			}
		}
	}
}
