using UnityEngine;

public class FPCameraStateManager : BaseStateManager<FPCameraState, vp_FPCamera>
{
	public FPCameraStateManager(vp_FPCamera managedComponent)
		: base(managedComponent)
	{
		CreateStates();
	}

	private void CreateStates()
	{
		states = new FPCameraState[1];
		FPCameraState fPCameraState = new FPCameraState("Default");
		fPCameraState.RenderingFieldOfView = 60f;
		fPCameraState.RenderingZoomDamping = 0.2f;
		fPCameraState.PositionOffset = new Vector3(0f, 1.75f, 0.1f);
		fPCameraState.PositionGroundLimit = 0.1f;
		fPCameraState.PositionSpringStiffness = 0.01f;
		fPCameraState.PositionSpringDamping = 0.25f;
		fPCameraState.PositionSpring2Stiffness = 0.95f;
		fPCameraState.PositionSpring2Damping = 0.25f;
		fPCameraState.PositionKneeling = 0.025f;
		fPCameraState.PositionKneelingSoftness = 1;
		fPCameraState.RotationPitchLimit = new Vector2(90f, -90f);
		fPCameraState.RotationYawLimit = new Vector2(-360f, 360f);
		fPCameraState.RotationSpringStiffness = 0.01f;
		fPCameraState.RotationSpringDamping = 0.25f;
		fPCameraState.RotationKneeling = 0.025f;
		fPCameraState.RotationKneelingSoftness = 1;
		fPCameraState.RotationStrafeRoll = 0.01f;
		fPCameraState.ShakeSpeed = 0f;
		fPCameraState.ShakeAmplitude = new Vector3(10f, 10f, 0f);
		fPCameraState.BobRate = new Vector4(0f, 1.4f, 0f, 0.7f);
		fPCameraState.BobAmplitude = new Vector4(0f, 0.25f, 0f, 0.5f);
		fPCameraState.BobInputVelocityScale = 1;
		fPCameraState.BobMaxInputVelocity = 100;
		fPCameraState.BobStepThreshold = 10;
		AddState(fPCameraState, 0);
	}

	public override void ApplyState(FPCameraState state)
	{
		if (state.RenderingFieldOfView.HasValue)
		{
			managedComponent.RenderingFieldOfView = state.RenderingFieldOfView.Value;
		}
		if (state.RenderingZoomDamping.HasValue)
		{
			managedComponent.RenderingZoomDamping = state.RenderingZoomDamping.Value;
		}
		if (state.PositionOffset.HasValue)
		{
			managedComponent.PositionOffset = state.PositionOffset.Value;
		}
		if (state.PositionGroundLimit.HasValue)
		{
			managedComponent.PositionGroundLimit = state.PositionGroundLimit.Value;
		}
		if (state.PositionSpringStiffness.HasValue)
		{
			managedComponent.PositionSpringStiffness = state.PositionSpringStiffness.Value;
		}
		if (state.PositionSpringDamping.HasValue)
		{
			managedComponent.PositionSpringDamping = state.PositionSpringDamping.Value;
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
		if (state.PositionEarthQuakeFactor.HasValue)
		{
			managedComponent.PositionEarthQuakeFactor = state.PositionEarthQuakeFactor.Value;
		}
		if (state.RotationPitchLimit.HasValue)
		{
			managedComponent.RotationPitchLimit = state.RotationPitchLimit.Value;
		}
		if (state.RotationYawLimit.HasValue)
		{
			managedComponent.RotationYawLimit = state.RotationYawLimit.Value;
		}
		if (state.RotationSpringStiffness.HasValue)
		{
			managedComponent.RotationSpringStiffness = state.RotationSpringStiffness.Value;
		}
		if (state.RotationSpringDamping.HasValue)
		{
			managedComponent.RotationSpringDamping = state.RotationSpringDamping.Value;
		}
		if (state.RotationKneeling.HasValue)
		{
			managedComponent.RotationKneeling = state.RotationKneeling.Value;
		}
		if (state.RotationKneelingSoftness.HasValue)
		{
			managedComponent.RotationKneelingSoftness = state.RotationKneelingSoftness.Value;
		}
		if (state.RotationStrafeRoll.HasValue)
		{
			managedComponent.RotationStrafeRoll = state.RotationStrafeRoll.Value;
		}
		if (state.RotationEarthQuakeFactor.HasValue)
		{
			managedComponent.RotationEarthQuakeFactor = state.RotationEarthQuakeFactor.Value;
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
		if (state.BobStepThreshold.HasValue)
		{
			managedComponent.BobStepThreshold = state.BobStepThreshold.Value;
		}
	}
}
