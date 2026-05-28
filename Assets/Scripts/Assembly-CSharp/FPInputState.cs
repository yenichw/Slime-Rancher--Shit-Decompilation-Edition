using UnityEngine;

public class FPInputState : BaseState
{
	public Vector2? MouseLookSensitivity;

	public int? MouseLookSmoothSteps;

	public bool? MouseLookAcceleration;

	public float? MouseLookSmoothWeight;

	public float? MouseLookAccelerationThreshold;

	public bool? MouseLookInvert;

	public bool? MouseCursorForced;

	public bool? MouseCursorBlocksMouseLook;

	public bool? Persist;

	public FPInputState(string name)
		: base(name)
	{
	}
}
