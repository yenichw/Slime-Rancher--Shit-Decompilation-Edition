public class FPControllerStateManager : BaseStateManager<FPControllerState, vp_FPController>
{
	public FPControllerStateManager(vp_FPController managedComponent)
		: base(managedComponent)
	{
		CreateStates();
	}

	private void CreateStates()
	{
		states = new FPControllerState[9];
		FPControllerState fPControllerState = new FPControllerState("Dead");
		fPControllerState.PhysicsForceDamping = 0.07f;
		fPControllerState.PhysicsGravityModifier = 0.06f;
		fPControllerState.PhysicsWallBounce = 1f;
		AddState(fPControllerState, 0);
		fPControllerState = new FPControllerState("Freeze");
		fPControllerState.MotorAcceleration = 0f;
		fPControllerState.MotorJumpForce = 0f;
		fPControllerState.MotorAirSpeed = 0f;
		fPControllerState.MotorSlopeSpeedUp = 0f;
		fPControllerState.MotorSlopeSpeedDown = 0f;
		fPControllerState.PhysicsForceDamping = 1f;
		fPControllerState.PhysicsPushForce = 0f;
		fPControllerState.PhysicsGravityModifier = 0f;
		fPControllerState.PhysicsWallBounce = 0f;
		AddState(fPControllerState, 1);
		fPControllerState = new FPControllerState("Zoom");
		fPControllerState.MotorDamping = 0.17f;
		fPControllerState.MotorAcceleration = 0.18f;
		AddState(fPControllerState, 2);
		fPControllerState = new FPControllerState("Crouch");
		fPControllerState.MotorAcceleration = 0.084f;
		fPControllerState.MotorDamping = 0.35f;
		fPControllerState.MotorJumpForce = 0f;
		fPControllerState.MotorAirSpeed = 0f;
		fPControllerState.PhysicsForceDamping = 0.05f;
		fPControllerState.PhysicsPushForce = 5f;
		fPControllerState.PhysicsGravityModifier = 0.2f;
		fPControllerState.PhysicsWallBounce = 0f;
		AddState(fPControllerState, 3);
		fPControllerState = new FPControllerState("Run");
		fPControllerState.MotorAcceleration = 0.45f;
		fPControllerState.MotorAirSpeed = 0.9f;
		AddState(fPControllerState, 4);
		fPControllerState = new FPControllerState("Jetpack1");
		fPControllerState.PhysicsGravityModifier = -0.06f;
		AddState(fPControllerState, 5);
		fPControllerState = new FPControllerState("Jetpack2");
		fPControllerState.PhysicsGravityModifier = -0.06f;
		AddState(fPControllerState, 6);
		fPControllerState = new FPControllerState("Underwater");
		fPControllerState.MotorDamping = 0.35f;
		fPControllerState.PhysicsGravityModifier = 0.1f;
		fPControllerState.MotorJumpForce = 0.09f;
		fPControllerState.MotorJumpForceDamping = 0.05f;
		fPControllerState.MotorJumpForceHold = 0.0015f;
		AddState(fPControllerState, 7);
		fPControllerState = new FPControllerState("Default");
		fPControllerState.MotorAcceleration = 0.25f;
		fPControllerState.MotorDamping = 0.15f;
		fPControllerState.MotorBackwardsSpeed = 0.65f;
		fPControllerState.MotorAirSpeed = 0.9f;
		fPControllerState.MotorSlopeSpeedUp = 1f;
		fPControllerState.MotorSlopeSpeedDown = 1f;
		fPControllerState.MotorJumpForce = 0.18f;
		fPControllerState.MotorJumpForceDamping = 0.08f;
		fPControllerState.MotorJumpForceHold = 0.003f;
		fPControllerState.MotorJumpForceHoldDamping = 0.5f;
		fPControllerState.PhysicsForceDamping = 0.2f;
		fPControllerState.PhysicsPushForce = 5f;
		fPControllerState.PhysicsGravityModifier = 0.2f;
		fPControllerState.PhysicsSlopeSlideLimit = 60f;
		fPControllerState.PhysicsSlopeSlidiness = 0.15f;
		fPControllerState.PhysicsWallBounce = 0f;
		fPControllerState.PhysicsWallFriction = 0f;
		fPControllerState.PhysicsHasCollisionTrigger = true;
		AddState(fPControllerState, 8);
	}

	public override void ApplyState(FPControllerState state)
	{
		if (state.MotorAcceleration.HasValue)
		{
			managedComponent.MotorAcceleration = state.MotorAcceleration.Value;
		}
		if (state.MotorBackwardsSpeed.HasValue)
		{
			managedComponent.MotorBackwardsSpeed = state.MotorBackwardsSpeed.Value;
		}
		if (state.MotorDamping.HasValue)
		{
			managedComponent.MotorDamping = state.MotorDamping.Value;
		}
		if (state.MotorAirSpeed.HasValue)
		{
			managedComponent.MotorAirSpeed = state.MotorAirSpeed.Value;
		}
		if (state.MotorSlopeSpeedUp.HasValue)
		{
			managedComponent.MotorSlopeSpeedUp = state.MotorSlopeSpeedUp.Value;
		}
		if (state.MotorSlopeSpeedDown.HasValue)
		{
			managedComponent.MotorSlopeSpeedDown = state.MotorSlopeSpeedDown.Value;
		}
		if (state.MotorJumpForce.HasValue)
		{
			managedComponent.MotorJumpForce = state.MotorJumpForce.Value;
		}
		if (state.MotorJumpForceDamping.HasValue)
		{
			managedComponent.MotorJumpForceDamping = state.MotorJumpForceDamping.Value;
		}
		if (state.MotorJumpForceHold.HasValue)
		{
			managedComponent.MotorJumpForceHold = state.MotorJumpForceHold.Value;
		}
		if (state.MotorJumpForceHoldDamping.HasValue)
		{
			managedComponent.MotorJumpForceHoldDamping = state.MotorJumpForceHoldDamping.Value;
		}
		if (state.PhysicsForceDamping.HasValue)
		{
			managedComponent.PhysicsForceDamping = state.PhysicsForceDamping.Value;
		}
		if (state.PhysicsPushForce.HasValue)
		{
			managedComponent.PhysicsPushForce = state.PhysicsPushForce.Value;
		}
		if (state.PhysicsGravityModifier.HasValue)
		{
			managedComponent.PhysicsGravityModifier = state.PhysicsGravityModifier.Value;
		}
		if (state.PhysicsSlopeSlideLimit.HasValue)
		{
			managedComponent.PhysicsSlopeSlideLimit = state.PhysicsSlopeSlideLimit.Value;
		}
		if (state.PhysicsSlopeSlidiness.HasValue)
		{
			managedComponent.PhysicsSlopeSlidiness = state.PhysicsSlopeSlidiness.Value;
		}
		if (state.PhysicsWallBounce.HasValue)
		{
			managedComponent.PhysicsWallBounce = state.PhysicsWallBounce.Value;
		}
		if (state.PhysicsWallFriction.HasValue)
		{
			managedComponent.PhysicsWallFriction = state.PhysicsWallFriction.Value;
		}
		if (state.PhysicsHasCollisionTrigger.HasValue)
		{
			managedComponent.PhysicsHasCollisionTrigger = state.PhysicsHasCollisionTrigger.Value;
		}
	}
}
