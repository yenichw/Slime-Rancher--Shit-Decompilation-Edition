using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GotoWater : FindConsumable
{
	private DestroyOnTouching nonWater;

	private Vector3? tgtLoc;

	private const float GAME_MINS_THRESHOLD = 20f;

	private const float SEARCH_RAD = 30f;

	private const float SEARCH_RAD_SQR = 900f;

	public override void Start()
	{
		base.Start();
		nonWater = GetComponent<DestroyOnTouching>();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (nonWater == null)
		{
			return 0f;
		}
		FindNearestWaterLoc();
		if (!tgtLoc.HasValue)
		{
			return 0f;
		}
		return 1f - nonWater.PctTimeToDestruct();
	}

	public override void Selected()
	{
	}

	private void FindNearestWaterLoc()
	{
		float num = 900f;
		LiquidSourceModel liquidSourceModel = null;
		foreach (LiquidSourceModel instance in SRSingleton<SceneContext>.Instance.GameModel.LiquidSources.Instances)
		{
			float sqrMagnitude = (instance.pos - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				liquidSourceModel = instance;
				num = sqrMagnitude;
			}
		}
		if (liquidSourceModel != null)
		{
			tgtLoc = liquidSourceModel.pos;
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
