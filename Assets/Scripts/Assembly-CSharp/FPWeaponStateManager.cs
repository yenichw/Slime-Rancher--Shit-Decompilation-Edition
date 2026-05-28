using UnityEngine;

public class FPWeaponStateManager : BaseStateManager<FPWeaponState, vp_FPWeapon>
{
	public FPWeaponStateManager(vp_FPWeapon managedComponent)
		: base(managedComponent)
	{
		CreateStates();
	}

	private void CreateStates()
	{
		FPWeaponState fPWeaponState = null;
		states = new FPWeaponState[4];
		fPWeaponState = new FPWeaponState("Zoom");
		fPWeaponState.ShakeSpeed = 0.05f;
		fPWeaponState.ShakeAmplitude = new Vector3(0.5f, 0f, 0f);
		fPWeaponState.PositionOffset = new Vector3(0f, -0.25f, 0.17f);
		fPWeaponState.RotationOffset = new Vector3(0.4917541f, 0.015994f, 0f);
		fPWeaponState.PositionSpringStiffness = 0.055f;
		fPWeaponState.PositionSpringDamping = 0.45f;
		fPWeaponState.RotationSpringStiffness = 0.025f;
		fPWeaponState.RotationSpringDamping = 0.35f;
		fPWeaponState.RenderingFieldOfView = 35;
		fPWeaponState.RenderingZoomDamping = 0.2f;
		fPWeaponState.BobAmplitude = new Vector4(0.5f, 0.4f, 0.2f, 0.005f);
		fPWeaponState.BobRate = new Vector4(0.8f, -0.4f, 0.4f, 0.4f);
		fPWeaponState.BobInputVelocityScale = 15f;
		fPWeaponState.PositionWalkSlide = new Vector3(0.2f, 0.5f, 0.2f);
		AddState(fPWeaponState, 0);
		fPWeaponState = new FPWeaponState("Attack");
		AddState(fPWeaponState, 1);
		fPWeaponState = new FPWeaponState("Run");
		fPWeaponState.BobAmplitude = new Vector4(1.5f, 1.2f, 0.6f, 0.015f);
		AddState(fPWeaponState, 2);
		fPWeaponState = new FPWeaponState("Default");
		fPWeaponState.RenderingZoomDamping = 0.5f;
		fPWeaponState.RenderingZScale = 1;
		fPWeaponState.PositionSpringStiffness = 0.01f;
		fPWeaponState.PositionSpringDamping = 0.25f;
		fPWeaponState.PositionFallRetract = 1;
		fPWeaponState.PositionPivotSpringStiffness = 0.1f;
		fPWeaponState.PositionPivotSpringDamping = 0.5f;
		fPWeaponState.PositionKneeling = 0.06f;
		fPWeaponState.PositionKneelingSoftness = 1;
		fPWeaponState.PositionWalkSlide = new Vector3(1f, 1f, 1f);
		fPWeaponState.PositionPivot = new Vector3(0f, 0f, -0.2741375f);
		fPWeaponState.RotationPivot = new Vector3(0f, 0f, 0f);
		fPWeaponState.PositionInputVelocityScale = 0.5f;
		fPWeaponState.PositionMaxInputVelocity = 1;
		fPWeaponState.RotationSpringStiffness = 0.01f;
		fPWeaponState.RotationSpringDamping = 0.25f;
		fPWeaponState.RotationPivotSpringStiffness = 0.01f;
		fPWeaponState.RotationPivotSpringDamping = 0.25f;
		fPWeaponState.RotationKneeling = 0;
		fPWeaponState.RotationKneelingSoftness = 1;
		fPWeaponState.RotationLookSway = new Vector3(1f, 1f, 0f);
		fPWeaponState.RotationStrafeSway = new Vector3(1f, 1f, -1f);
		fPWeaponState.RotationFallSway = new Vector3(2f, 0f, 0f);
		fPWeaponState.RotationSlopeSway = 0.75f;
		fPWeaponState.RotationInputVelocityScale = 1;
		fPWeaponState.RotationMaxInputVelocity = 5;
		fPWeaponState.RetractionDistance = 0.4f;
		fPWeaponState.RetractionOffset = new Vector2(0f, 0f);
		fPWeaponState.RetractionRelaxSpeed = 0.25f;
		fPWeaponState.ShakeSpeed = 0.1f;
		fPWeaponState.ShakeAmplitude = new Vector3(0.1f, 0f, 0.3f);
		fPWeaponState.BobRate = new Vector4(1.4f, 0.7f, 0.7f, 0.7f);
		fPWeaponState.BobAmplitude = new Vector4(0.5f, 0.4f, 0.2f, 0.005f);
		fPWeaponState.BobInputVelocityScale = 3.5f;
		fPWeaponState.BobMaxInputVelocity = 250;
		fPWeaponState.BobRequireGroundContact = true;
		fPWeaponState.StepPositionForce = new Vector3(0f, 0.015f, 0f);
		fPWeaponState.StepRotationForce = new Vector3(1.5f, 0f, 0f);
		fPWeaponState.StepSoftness = 5;
		fPWeaponState.StepMinVelocity = 2;
		fPWeaponState.StepPositionBalance = 0;
		fPWeaponState.StepRotationBalance = 0;
		fPWeaponState.StepForceScale = 0.05f;
		fPWeaponState.LookDownActive = false;
		fPWeaponState.LookDownYawLimit = 60f;
		fPWeaponState.LookDownPositionOffsetMiddle = new Vector3(0.35f, -0.37f, 0.78f);
		fPWeaponState.LookDownPositionOffsetLeft = new Vector3(0.27f, -0.31f, 0.7f);
		fPWeaponState.LookDownPositionOffsetRight = new Vector3(0.6f, -0.41f, 0.86f);
		fPWeaponState.LookDownPositionSpringPower = 1f;
		fPWeaponState.LookDownRotationOffsetMiddle = new Vector3(-3.9f, 2.24f, 4.69f);
		fPWeaponState.LookDownRotationOffsetLeft = new Vector3(-7f, -10.5f, 15.6f);
		fPWeaponState.LookDownRotationOffsetRight = new Vector3(-9.2f, -9.8f, 48.84f);
		fPWeaponState.LookDownRotationSpringPower = 1f;
		fPWeaponState.AmbientInterval = new Vector2(0f, 0f);
		fPWeaponState.PositionExitOffset = new Vector3(0f, -1f, 0f);
		fPWeaponState.PositionOffset = new Vector3(0.1493672f, -0.83f, -0.75f);
		fPWeaponState.PositionSpring2Stiffness = 0.2f;
		fPWeaponState.PositionSpring2Damping = 0.5615942f;
		fPWeaponState.RotationExitOffset = new Vector3(40f, 0f, 0f);
		fPWeaponState.RotationOffset = new Vector3(-3.158258f, -5f, 0f);
		fPWeaponState.RotationSpring2Stiffness = 0.85f;
		fPWeaponState.RotationSpring2Damping = 0.6f;
		fPWeaponState.AnimationType = 1;
		fPWeaponState.AnimationGrip = 1;
		fPWeaponState.Persist = false;
		AddState(fPWeaponState, 3);
	}

	public override void ApplyState(FPWeaponState state)
	{
		if (state.RenderingZoomDamping.HasValue)
		{
			managedComponent.RenderingZoomDamping = state.RenderingZoomDamping.Value;
		}
		if (state.RenderingZScale.HasValue)
		{
			managedComponent.RenderingZScale = state.RenderingZScale.Value;
		}
		if (state.PositionOffset.HasValue)
		{
			managedComponent.PositionOffset = state.PositionOffset.Value;
		}
		if (state.PositionSpringStiffness.HasValue)
		{
			managedComponent.PositionSpringStiffness = state.PositionSpringStiffness.Value;
		}
		if (state.PositionSpringDamping.HasValue)
		{
			managedComponent.PositionSpringDamping = state.PositionSpringDamping.Value;
		}
		if (state.PositionFallRetract.HasValue)
		{
			managedComponent.PositionFallRetract = state.PositionFallRetract.Value;
		}
		if (state.PositionPivotSpringStiffness.HasValue)
		{
			managedComponent.PositionPivotSpringStiffness = state.PositionPivotSpringStiffness.Value;
		}
		if (state.PositionPivotSpringDamping.HasValue)
		{
			managedComponent.PositionPivotSpringDamping = state.PositionPivotSpringDamping.Value;
		}
		if (state.PositionSpring2Stiffness.HasValue)
		{
			managedComponent.PositionSpring2Stiffness = state.PositionSpring2Stiffness.Value;
		}
		if (state.PositionSpring2Damping.HasValue)
		{
			managedComponent.PositionSpring2Damping = state.PositionSpring2Damping.Value;
		}
		if (state.PositionKneeling.HasValue)
		{
			managedComponent.PositionKneeling = state.PositionKneeling.Value;
		}
		if (state.PositionKneelingSoftness.HasValue)
		{
			managedComponent.PositionKneelingSoftness = state.PositionKneelingSoftness.Value;
		}
		if (state.PositionWalkSlide.HasValue)
		{
			managedComponent.PositionWalkSlide = state.PositionWalkSlide.Value;
		}
		if (state.PositionPivot.HasValue)
		{
			managedComponent.PositionPivot = state.PositionPivot.Value;
		}
		if (state.RotationPivot.HasValue)
		{
			managedComponent.RotationPivot = state.RotationPivot.Value;
		}
		if (state.PositionInputVelocityScale.HasValue)
		{
			managedComponent.PositionInputVelocityScale = state.PositionInputVelocityScale.Value;
		}
		if (state.PositionMaxInputVelocity.HasValue)
		{
			managedComponent.PositionMaxInputVelocity = state.PositionMaxInputVelocity.Value;
		}
		if (state.RotationOffset.HasValue)
		{
			managedComponent.RotationOffset = state.RotationOffset.Value;
		}
		if (state.RotationSpringStiffness.HasValue)
		{
			managedComponent.RotationSpringStiffness = state.RotationSpringStiffness.Value;
		}
		if (state.RotationSpringDamping.HasValue)
		{
			managedComponent.RotationSpringDamping = state.RotationSpringDamping.Value;
		}
		if (state.RotationPivotSpringStiffness.HasValue)
		{
			managedComponent.RotationPivotSpringStiffness = state.RotationPivotSpringStiffness.Value;
		}
		if (state.RotationPivotSpringDamping.HasValue)
		{
			managedComponent.RotationPivotSpringDamping = state.RotationPivotSpringDamping.Value;
		}
		if (state.RotationSpring2Stiffness.HasValue)
		{
			managedComponent.RotationSpring2Stiffness = state.RotationSpring2Stiffness.Value;
		}
		if (state.RotationSpring2Damping.HasValue)
		{
			managedComponent.RotationSpring2Damping = state.RotationSpring2Damping.Value;
		}
		if (state.RotationKneeling.HasValue)
		{
			managedComponent.RotationKneeling = state.RotationKneeling.Value;
		}
		if (state.RotationKneelingSoftness.HasValue)
		{
			managedComponent.RotationKneelingSoftness = state.RotationKneelingSoftness.Value;
		}
		if (state.RotationLookSway.HasValue)
		{
			managedComponent.RotationLookSway = state.RotationLookSway.Value;
		}
		if (state.RotationStrafeSway.HasValue)
		{
			managedComponent.RotationStrafeSway = state.RotationStrafeSway.Value;
		}
		if (state.RotationFallSway.HasValue)
		{
			managedComponent.RotationFallSway = state.RotationFallSway.Value;
		}
		if (state.RotationSlopeSway.HasValue)
		{
			managedComponent.RotationSlopeSway = state.RotationSlopeSway.Value;
		}
		if (state.RotationInputVelocityScale.HasValue)
		{
			managedComponent.RotationInputVelocityScale = state.RotationInputVelocityScale.Value;
		}
		if (state.RotationMaxInputVelocity.HasValue)
		{
			managedComponent.RotationMaxInputVelocity = state.RotationMaxInputVelocity.Value;
		}
		if (state.RetractionDistance.HasValue)
		{
			managedComponent.RetractionDistance = state.RetractionDistance.Value;
		}
		if (state.RetractionOffset.HasValue)
		{
			managedComponent.RetractionOffset = state.RetractionOffset.Value;
		}
		if (state.RetractionRelaxSpeed.HasValue)
		{
			managedComponent.RetractionRelaxSpeed = state.RetractionRelaxSpeed.Value;
		}
		if (state.ShakeSpeed.HasValue)
		{
			managedComponent.ShakeSpeed = state.ShakeSpeed.Value;
		}
		if (state.ShakeAmplitude.HasValue)
		{
			managedComponent.ShakeAmplitude = state.ShakeAmplitude.Value;
		}
		if (state.BobRate.HasValue)
		{
			managedComponent.BobRate = state.BobRate.Value;
		}
		if (state.BobAmplitude.HasValue)
		{
			managedComponent.BobAmplitude = state.BobAmplitude.Value;
		}
		if (state.BobInputVelocityScale.HasValue)
		{
			managedComponent.BobInputVelocityScale = state.BobInputVelocityScale.Value;
		}
		if (state.BobMaxInputVelocity.HasValue)
		{
			managedComponent.BobMaxInputVelocity = state.BobMaxInputVelocity.Value;
		}
		if (state.BobRequireGroundContact.HasValue)
		{
			managedComponent.BobRequireGroundContact = state.BobRequireGroundContact.Value;
		}
		if (state.StepPositionForce.HasValue)
		{
			managedComponent.StepPositionForce = state.StepPositionForce.Value;
		}
		if (state.StepRotationForce.HasValue)
		{
			managedComponent.StepRotationForce = state.StepRotationForce.Value;
		}
		if (state.StepSoftness.HasValue)
		{
			managedComponent.StepSoftness = state.StepSoftness.Value;
		}
		if (state.StepMinVelocity.HasValue)
		{
			managedComponent.StepMinVelocity = state.StepMinVelocity.Value;
		}
		if (state.StepPositionBalance.HasValue)
		{
			managedComponent.StepPositionBalance = state.StepPositionBalance.Value;
		}
		if (state.StepRotationBalance.HasValue)
		{
			managedComponent.StepRotationBalance = state.StepRotationBalance.Value;
		}
		if (state.StepForceScale.HasValue)
		{
			managedComponent.StepForceScale = state.StepForceScale.Value;
		}
		if (state.AmbientInterval.HasValue)
		{
			managedComponent.AmbientInterval = state.AmbientInterval.Value;
		}
		if (state.PositionExitOffset.HasValue)
		{
			managedComponent.PositionExitOffset = state.PositionExitOffset.Value;
		}
		if (state.RotationExitOffset.HasValue)
		{
			managedComponent.RotationExitOffset = state.RotationExitOffset.Value;
		}
		if (state.LookDownActive.HasValue)
		{
			managedComponent.LookDownActive = state.LookDownActive.Value;
		}
		if (state.LookDownYawLimit.HasValue)
		{
			managedComponent.LookDownYawLimit = state.LookDownYawLimit.Value;
		}
		if (state.LookDownPositionOffsetMiddle.HasValue)
		{
			managedComponent.LookDownPositionOffsetMiddle = state.LookDownPositionOffsetMiddle.Value;
		}
		if (state.LookDownPositionOffsetLeft.HasValue)
		{
			managedComponent.LookDownPositionOffsetLeft = state.LookDownPositionOffsetLeft.Value;
		}
		if (state.LookDownPositionOffsetRight.HasValue)
		{
			managedComponent.LookDownPositionOffsetRight = state.LookDownPositionOffsetRight.Value;
		}
		if (state.LookDownPositionSpringPower.HasValue)
		{
			managedComponent.LookDownPositionSpringPower = state.LookDownPositionSpringPower.Value;
		}
		if (state.LookDownRotationOffsetMiddle.HasValue)
		{
			managedComponent.LookDownRotationOffsetMiddle = state.LookDownRotationOffsetMiddle.Value;
		}
		if (state.LookDownRotationOffsetLeft.HasValue)
		{
			managedComponent.LookDownRotationOffsetLeft = state.LookDownRotationOffsetLeft.Value;
		}
		if (state.LookDownRotationOffsetRight.HasValue)
		{
			managedComponent.LookDownRotationOffsetRight = state.LookDownRotationOffsetRight.Value;
		}
		if (state.LookDownRotationSpringPower.HasValue)
		{
			managedComponent.LookDownRotationSpringPower = state.LookDownRotationSpringPower.Value;
		}
		if (state.AnimationType.HasValue)
		{
			managedComponent.AnimationType = state.AnimationType.Value;
		}
		if (state.AnimationGrip.HasValue)
		{
			managedComponent.AnimationGrip = state.AnimationGrip.Value;
		}
		if (state.Persist.HasValue)
		{
			managedComponent.Persist = state.Persist.Value;
		}
	}
}
