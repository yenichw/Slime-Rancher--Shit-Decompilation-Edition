namespace MonomiPark.SlimeRancher.DataModel
{
	public class QuicksilverEnergyGeneratorModel
	{
		public interface Participant
		{
			void InitModel(QuicksilverEnergyGeneratorModel model);

			void SetModel(QuicksilverEnergyGeneratorModel model);
		}

		public QuicksilverEnergyGenerator.State state;

		public double? timer;

		private Participant part;

		public void SetParticipant(Participant part)
		{
			this.part = part;
		}

		public void Init()
		{
			part.InitModel(this);
		}

		public void NotifyParticipants()
		{
			part.SetModel(this);
		}

		public void Push(QuicksilverEnergyGenerator.State state, double? timer)
		{
			this.state = state;
			this.timer = timer;
		}

		public void Pull(out QuicksilverEnergyGenerator.State state, out double? timer)
		{
			state = this.state;
			timer = this.timer;
		}
	}
}
