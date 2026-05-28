using UnityEngine;

public class GlitchVacuumable : Vacuumable
{
	protected override void SetCaptive(Joint toJoint)
	{
		base.SetCaptive(toJoint);
		if (isCaptive())
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
	}
}
