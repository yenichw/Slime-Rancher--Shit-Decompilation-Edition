using UnityEngine;

[RequireComponent(typeof(Drone))]
[RequireComponent(typeof(Rigidbody))]
public class DroneMovement : MonoBehaviour
{
	[Tooltip("Movement: speed")]
	public float movementSpeed;

	[Tooltip("Rotation: facing speed")]
	public float rotationFacingSpeed;

	[Tooltip("Rotation: facing stability")]
	public float rotationFacingStability;

	[Tooltip("Avoidance: min/max strength of normal adjustment to collision")]
	public Vector2 avoidanceStrength;

	public const int AVOIDANCE_MASK = -537968901;

	private static readonly float SQRT_TWO = Mathf.Sqrt(2f);

	public Rigidbody rigidbody { get; private set; }

	public Drone drone { get; private set; }

	public void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		drone = GetComponent<Drone>();
	}

	public bool MoveTowards(Vector3 position)
	{
		Vector3 v = (rigidbody.position = Vector3.MoveTowards(rigidbody.position, position, Time.fixedDeltaTime * 0.25f * rigidbody.mass));
		return ApproximatelyEquals(position, v, 0.05f);
	}

	public bool RotateTowards(Quaternion rotation)
	{
		Quaternion q = (rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, rotation, Time.fixedDeltaTime * 20f * rigidbody.mass));
		return ApproximatelyEquals(rotation, q, 0.5f);
	}

	public static bool ApproximatelyEquals(Vector3 v1, Vector3 v2, float range)
	{
		return (v2 - v1).sqrMagnitude <= range * range;
	}

	public static bool ApproximatelyEquals(Quaternion q1, Quaternion q2, float range)
	{
		return Quaternion.Angle(q1, q2) < range;
	}

	public void PathTowards(Vector3 position)
	{
		Vector3 forceMoveTowards = GetForceMoveTowards(position);
		Vector3 normalized = (forceMoveTowards + GetForceAvoidance()).normalized;
		rigidbody.AddTorque(Vector3.Cross(Quaternion.AngleAxis(rigidbody.angularVelocity.magnitude * 57.29578f * rotationFacingStability * 0.1f / rotationFacingSpeed, rigidbody.angularVelocity) * base.transform.forward, forceMoveTowards) * rotationFacingSpeed * rotationFacingSpeed * rigidbody.mass);
		rigidbody.AddForce(normalized * movementSpeed * Time.fixedDeltaTime * rigidbody.mass);
	}

	private Vector3 GetForceMoveTowards(Vector3 target)
	{
		return (target - rigidbody.position).normalized;
	}

	private Vector3 GetForceAvoidance()
	{
		Vector3 vector = Vector3.zero;
		if (!drone.noClip.enabled)
		{
			Vector3 forward = base.transform.forward;
			_ = base.transform.rotation;
			float num = Mathf.Max(0.6f, rigidbody.velocity.sqrMagnitude * 0.4f);
			float num2 = num * 2f;
			Vector3 position = base.transform.position;
			float radius = 0.5f;
			if (Physics.SphereCast(position - forward * 0.1f, radius, forward, out var _, num, -537968901))
			{
				Ray ray = new Ray(position + base.transform.right * 0.5f - base.transform.up * 0.25f, forward + base.transform.right * 0.1f);
				Ray ray2 = new Ray(position - base.transform.right * 0.5f - base.transform.up * 0.25f, forward - base.transform.right * 0.1f);
				Ray ray3 = new Ray(position + base.transform.up * 0.5f, forward + base.transform.up);
				Physics.Raycast(ray, out var hitInfo2, num2, -537968901);
				Physics.Raycast(ray2, out var hitInfo3, num2, -537968901);
				Physics.Raycast(ray3, out var hitInfo4, num2 * SQRT_TWO, -537968901);
				float num3 = ((hitInfo3.collider == null) ? float.PositiveInfinity : hitInfo3.distance);
				float num4 = ((hitInfo2.collider == null) ? float.PositiveInfinity : hitInfo2.distance);
				float num5 = ((hitInfo4.collider == null) ? float.PositiveInfinity : hitInfo4.distance);
				vector = ((num3 < num && num4 < num && num5 > num) ? base.transform.up : ((!(num3 > num4)) ? base.transform.right : (-base.transform.right)));
			}
		}
		return vector.normalized;
	}
}
