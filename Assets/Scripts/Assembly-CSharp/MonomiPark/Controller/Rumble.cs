using System.Collections.Generic;

namespace MonomiPark.Controller
{
	public abstract class Rumble
	{
		public enum Motor
		{
			LARGE = 0,
			SMALL = 1,
			LEFT = 2,
			RIGHT = 3
		}

		public class MotorComparer : IEqualityComparer<Motor>
		{
			public bool Equals(Motor motor1, Motor motor2)
			{
				return motor1 == motor2;
			}

			public int GetHashCode(Motor motor)
			{
				return (int)motor;
			}
		}

		public static IEqualityComparer<Motor> motorComparer = new MotorComparer();

		private Motor motor;

		public Rumble(Motor motor)
		{
			this.motor = motor;
		}

		public Motor GetMotor()
		{
			return motor;
		}

		public abstract int CurrentPower();

		public abstract bool IsFinished();
	}
}
