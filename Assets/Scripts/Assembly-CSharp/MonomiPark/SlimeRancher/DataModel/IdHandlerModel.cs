namespace MonomiPark.SlimeRancher.DataModel
{
	public abstract class IdHandlerModel
	{
		public interface Participant
		{
			void InitModel(IdHandlerModel model);

			void SetModel(IdHandlerModel model);

			string GetId();
		}

		private Participant participant;

		public void SetParticipant(Participant participant)
		{
			this.participant = participant;
		}

		public void Init()
		{
			participant.InitModel(this);
		}

		public void NotifyParticipants()
		{
			participant.SetModel(this);
		}
	}
}
