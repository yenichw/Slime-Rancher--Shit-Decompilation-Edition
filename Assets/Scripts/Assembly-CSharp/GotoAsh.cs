using UnityEngine;

public class GotoAsh : FindConsumable
{
	private DestroyOnTouching nonAsh;

	private Vector3? tgtLoc;

	private const float GAME_MINS_THRESHOLD = 20f;

	private const float SEARCH_RAD = 30f;

	private const float SEARCH_RAD_SQR = 900f;

	public override void Start()
	{
		base.Start();
		nonAsh = GetComponent<DestroyOnTouching>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (nonAsh == null)
		{
			return 0f;
		}
		FindNearestAshLoc();
		if (!tgtLoc.HasValue)
		{
			return 0f;
		}
		return 1f - nonAsh.PctTimeToDestruct();
	}

	public override void Selected()
	{
	}

	private void FindNearestAshLoc()
	{
		float num = 900f;
		AshSource ashSource = null;
		foreach (AshSource allAsh in AshSource.allAshes)
		{
			float sqrMagnitude = (allAsh.transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				ashSource = allAsh;
				num = sqrMagnitude;
			}
		}
		if (ashSource != null)
		{
			tgtLoc = ashSource.transform.position;
		}
		else
		{
			tgtLoc = null;
		}
	}

	public override void Deselected()
	{
		base.Deselected();
	}

	public override void Action()
	{
		if (tgtLoc.HasValue && IsGrounded())
		{
			float nextJumpAvail = float.PositiveInfinity;
			MoveTowards(tgtLoc.Value, shouldJump: false, ref nextJumpAvail, 0f);
		}
	}
}
