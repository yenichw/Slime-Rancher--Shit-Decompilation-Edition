using UnityEngine;

public abstract class SlimeSubbehaviour : CollidableActorBehaviour
{
	protected SlimeEmotions emotions;

	protected Vacuumable vacuumable;

	protected SlimeSubbehaviourPlexer plexer;

	protected Rigidbody slimeBody;

	private const float STABILIZE_FACTOR = 0.1f;

	public override void Awake()
	{
		base.Awake();
		plexer = GetComponent<SlimeSubbehaviourPlexer>();
		emotions = GetComponent<SlimeEmotions>();
		vacuumable = GetComponent<Vacuumable>();
		slimeBody = GetComponent<Rigidbody>();
	}

	public abstract float Relevancy(bool isGrounded);

	public abstract void Action();

	public abstract void Selected();

	public virtual void Deselected()
	{
	}

	public virtual bool CanRethink()
	{
		return true;
	}

	public virtual bool Forbids(SlimeSubbehaviour toMaybeForbid)
	{
		return false;
	}

	protected bool IsFloating()
	{
		if (plexer != null)
		{
			return plexer.IsFloating();
		}
		return false;
	}

	protected bool IsGrounded()
	{
		if (plexer != null)
		{
			return plexer.IsGrounded();
		}
		return false;
	}

	protected bool IsNearGrounded(float dist)
	{
		if (plexer != null)
		{
			return plexer.IsNearGrounded(dist);
		}
		return false;
	}

	protected bool IsBlocked(GameObject obj, int layersToIgnore = 0, bool forceCheckFullDist = false)
	{
		if (plexer != null)
		{
			return plexer.IsBlocked(obj, layersToIgnore, forceCheckFullDist);
		}
		return false;
	}

	protected bool IsCaptive()
	{
		bool result = false;
		if (vacuumable != null)
		{
			result = vacuumable.isCaptive();
		}
		return result;
	}

	protected void RotateTowards(Vector3 dirToTarget, float facingSpeed, float facingStability)
	{
		Vector3 angularVelocity = slimeBody.angularVelocity;
		Vector3 vector = Vector3.Cross(Quaternion.AngleAxis(angularVelocity.magnitude * 57.29578f * facingStability * 0.1f / facingSpeed, angularVelocity) * base.transform.forward, dirToTarget);
		slimeBody.AddTorque(vector * (facingSpeed * facingSpeed) * slimeBody.mass);
	}

	public static Vector3 GetGotoPos(GameObject obj)
	{
		if (!(obj == SRSingleton<SceneContext>.Instance.Player))
		{
			return obj.transform.position;
		}
		return obj.transform.position + Vector3.up;
	}
}
