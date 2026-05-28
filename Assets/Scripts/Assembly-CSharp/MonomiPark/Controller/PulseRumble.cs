using UnityEngine;

namespace MonomiPark.Controller
{
	public class PulseRumble : Rumble
	{
		private int maxPower;

		private float stopTime;

		public PulseRumble(Motor motor, int maxPower, float duration)
			: base(motor)
		{
			stopTime = Time.time + duration;
			this.maxPower = maxPower;
		}

		public override int CurrentPower()
		{
			return maxPower;
		}

		public override bool IsFinished()
		{
			return Time.time >= stopTime;
		}
	}
}
