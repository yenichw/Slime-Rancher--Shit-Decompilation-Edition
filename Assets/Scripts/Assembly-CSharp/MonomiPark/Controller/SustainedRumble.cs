namespace MonomiPark.Controller
{
	public class SustainedRumble : Rumble
	{
		private int power;

		private bool isFinished;

		public SustainedRumble(Motor motor, int power)
			: base(motor)
		{
			this.power = power;
		}

		public override int CurrentPower()
		{
			return power;
		}

		public void UpdatePower(int power)
		{
			this.power = power;
		}

		public void FinishRumble()
		{
			isFinished = true;
		}

		public override bool IsFinished()
		{
			return isFinished;
		}
	}
}
