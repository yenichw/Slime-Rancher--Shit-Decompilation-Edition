namespace MonomiPark.SlimeRancher.DataModel
{
	public class SpawnerTriggerModel : PositionalModel
	{
		public interface Participant
		{
			void InitModel(SpawnerTriggerModel model);

			void SetModel(SpawnerTriggerModel model);
		}

		public double nextTriggerTime;

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

		public void Push(double nextTriggerTime)
		{
			this.nextTriggerTime = nextTriggerTime;
		}

		public void Pull(out double nextTriggerTime)
		{
			nextTriggerTime = this.nextTriggerTime;
		}
	}
}
