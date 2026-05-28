using System.Collections.Generic;
using UnityEngine;

public class DroneSubbehaviourRest : DroneProgram
{
	private const float RETHINK_BASE_HOURS = 1f / 3f;

	private const float RETHINK_PERIOD_HOURS = 1f / 6f;

	private double rethinkTime;

	protected override DroneAnimator.Id animation => DroneAnimator.Id.REST;

	protected override DroneAnimatorState.Id animationStateBegin => DroneAnimatorState.Id.REST_BEGIN;

	protected override DroneAnimatorState.Id animationStateEnd => DroneAnimatorState.Id.REST_END;

	public override bool Relevancy()
	{
		return true;
	}

	public override void Selected()
	{
		base.Selected();
		rethinkTime = double.MaxValue;
	}

	public override void Deselected()
	{
		base.Deselected();
		drone.station.battery.onReset -= ForceRethink;
		drone.station.battery.onHasAnyChanged -= OnBatteryHasAnyChanged;
	}

	public void ForceRethink()
	{
		rethinkTime = 0.0;
	}

	protected override IEnumerable<Orientation> GetTargetOrientations()
	{
		yield return drone.GetRestingOrientation();
	}

	protected override Vector3 GetTargetPosition()
	{
		return drone.station.guideRest.position;
	}

	protected override bool CanCancel()
	{
		return false;
	}

	protected override void OnFirstAction()
	{
		base.OnFirstAction();
		rethinkTime = timeDirector.HoursFromNow(1f / 3f);
		drone.noClip.enabled = false;
		drone.onActiveCue.enabled = false;
		drone.station.battery.onReset += ForceRethink;
		drone.station.battery.onHasAnyChanged += OnBatteryHasAnyChanged;
		OnBatteryHasAnyChanged();
	}

	protected override bool OnAction()
	{
		if (timeDirector.HasReached(rethinkTime))
		{
			if (base.plexer.PickNextGatherBehaviour())
			{
				drone.onActiveCue.enabled = true;
				drone.station.animator.SetEnabled(enabled: true);
				return true;
			}
			rethinkTime = timeDirector.HoursFromNow(1f / 6f);
		}
		return false;
	}

	private void OnBatteryHasAnyChanged()
	{
		drone.station.animator.SetEnabled(drone.station.battery.HasAny());
	}
}
