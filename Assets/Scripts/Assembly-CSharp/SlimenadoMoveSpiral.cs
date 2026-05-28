using UnityEngine;

public class SlimenadoMoveSpiral : MonoBehaviour
{
	private float rot;

	private float nextGroundCheck;

	private float tgtY;

	private const float SPEED = 3f;

	private const float VERT_SPEED = 5f;

	private const float INIT_ROT = 90f;

	private const float GROUND_CHECK_PERIOD = 1f;

	private const float GROUND_ADJUST_DIST = 10f;

	private const int GROUND_RAY_MASK = 268435457;

	private void Start()
	{
		rot = 90f;
		tgtY = base.transform.position.y;
		nextGroundCheck = Time.time + nextGroundCheck;
	}

	private void FixedUpdate()
	{
		float y = base.transform.position.y;
		if (tgtY > y)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Min(tgtY, y + 5f * Time.fixedDeltaTime), base.transform.position.z);
		}
		else if (tgtY < y)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Max(tgtY, y - 5f * Time.fixedDeltaTime), base.transform.position.z);
		}
		base.transform.Translate(base.transform.forward * (3f * Time.fixedDeltaTime));
		base.transform.Rotate(0f, rot * Time.fixedDeltaTime, 0f, Space.World);
		rot *= Mathf.Pow(0.9f, Time.fixedDeltaTime);
		if (Time.time >= nextGroundCheck)
		{
			if (Physics.Raycast(base.transform.position + Vector3.up * 10f, Vector3.down, out var hitInfo, 20f, 268435457))
			{
				tgtY = hitInfo.point.y;
			}
			else
			{
				Destroyer.Destroy(base.gameObject, "SlimenadoMoveSpiral.FixedUpdate");
			}
			nextGroundCheck = Time.time + 1f;
		}
	}
}
