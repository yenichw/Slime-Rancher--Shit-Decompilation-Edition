using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class ScalingLiquidSource : LiquidSource, LiquidConsumer
{
	[Tooltip("The object to scale based on our fullness")]
	public Transform toScale;

	[Tooltip("The maximum number of consumable units this can hold.")]
	public int maxUnits = 20;

	[Tooltip("The amount of liquid to refill each game hour.")]
	public float refillUnitsPerGameHour = 0.25f;

	private TimeDirector timeDir;

	private AmbianceDirector ambianceDir;

	private Vector3 initScale;

	public override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
			ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
			initScale = toScale.localScale;
		}
	}

	protected override void InitModel(LiquidSourceModel model)
	{
		base.InitModel(model);
		model.isScaling = true;
		model.unitsFilled = maxUnits;
	}

	public override bool Available()
	{
		return model.unitsFilled >= 1f;
	}

	public override void ConsumeLiquid()
	{
		model.unitsFilled -= 1f;
	}

	public void AddLiquid(Identifiable.Id liquidId, float amount)
	{
		if (liquidId == base.liquidId)
		{
			model.unitsFilled += 1f;
		}
	}

	public void Update()
	{
		if (Application.isPlaying)
		{
			model.unitsFilled = Mathf.Min(maxUnits, model.unitsFilled + (float)((double)(refillUnitsPerGameHour + ambianceDir.PrecipitationRate()) * timeDir.DeltaWorldTime() * 0.00027777778450399637));
			toScale.localScale = new Vector3(initScale.x, initScale.y * (model.unitsFilled / (float)maxUnits), initScale.z);
		}
	}
}
