namespace MonomiPark.SlimeRancher.DataModel
{
	public class RanchCellModel
	{
		public interface Participant
		{
			void InitModel(RanchCellModel model);

			void SetModel(RanchCellModel model);
		}

		public double? hibernationTime;

		public double? sleepingTime;

		private Participant participant;

		public void SetParticipant(Participant participant)
		{
			this.participant = participant;
		}

		public void Init()
		{
			if (participant != null)
			{
				participant.InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			if (participant != null)
			{
				participant.SetModel(this);
			}
		}

		public void Push(double? hibernationTime)
		{
			this.hibernationTime = hibernationTime;
		}

		public void Pull(out double? hibernationTime)
		{
			hibernationTime = this.hibernationTime;
		}
	}
}
