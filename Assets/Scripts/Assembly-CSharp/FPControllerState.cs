public class FPControllerState : BaseState
{
	public float? MotorAcceleration;

	public float? MotorBackwardsSpeed;

	public float? MotorDamping;

	public float? MotorAirSpeed;

	public float? MotorSlopeSpeedUp;

	public float? MotorSlopeSpeedDown;

	public bool? MotorFreeFly;

	public float? MotorJumpForce;

	public float? MotorJumpForceDamping;

	public float? MotorJumpForceHold;

	public float? MotorJumpForceHoldDamping;

	public float? PhysicsForceDamping;

	public float? PhysicsPushForce;

	public float? PhysicsGravityModifier;

	public float? PhysicsSlopeSlideLimit;

	public float? PhysicsSlopeSlidiness;

	public float? PhysicsWallBounce;

	public float? PhysicsWallFriction;

	public bool? PhysicsHasCollisionTrigger;

	public FPControllerState(string name)
		: base(name)
	{
	}
}
