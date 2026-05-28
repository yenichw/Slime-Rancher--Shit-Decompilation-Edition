using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class FillableAshSource : AshSource, LandPlotModel.Participant
{
	[Tooltip("The maximum number of consumable units this can hold.")]
	public int maxUnits = 20;

	public float minYPos = 0.05f;

	private float ashFillSpeed = 0.25f;

	private Vector3 initPos;

	private LandPlotModel plotModel;

	private Tween ashMoveTween;

	private Rigidbody body;

	public override void Awake()
	{
		AshSource.allAshes.Add(this);
		body = GetComponent<Rigidbody>();
		initPos = base.transform.localPosition;
		UpdateAshPosition();
	}

	public void InitModel(LandPlotModel model)
	{
		model.ashUnits = maxUnits;
	}

	public void SetModel(LandPlotModel model)
	{
		plotModel = model;
		UpdateAshPosition();
	}

	public override bool Available()
	{
		return plotModel.ashUnits >= 1f;
	}

	public override void ConsumeAsh()
	{
		plotModel.ashUnits = Mathf.Max(plotModel.ashUnits - 1f, 0f);
		UpdateAshPosition();
	}

	public void AddAsh(float amount)
	{
		plotModel.ashUnits = Mathf.Min(plotModel.ashUnits + amount, maxUnits);
		UpdateAshPosition();
	}

	private void UpdateAshPosition()
	{
		if (plotModel != null && base.gameObject.activeInHierarchy)
		{
			ashMoveTween?.Kill();
			ashMoveTween = body.DOMoveY(GetAshYPosition(), ashFillSpeed).SetSpeedBased(isSpeedBased: true);
		}
	}

	private float GetAshYPosition()
	{
		return base.transform.parent.TransformPoint(0f, Mathf.Max(initPos.y * (plotModel.ashUnits / (float)maxUnits), minYPos), 0f).y;
	}

	public float GetAshSpace()
	{
		return (float)maxUnits - plotModel.ashUnits;
	}
}
