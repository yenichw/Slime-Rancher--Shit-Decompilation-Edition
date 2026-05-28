using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchImpostoDirectorModel
	{
		public interface Participant
		{
			string id { get; }

			void InitModel(GlitchImpostoDirectorModel model);

			void SetModel(GlitchImpostoDirectorModel model);
		}

		public double? hibernationTime;

		private readonly Participant participant;

		public GlitchImpostoDirectorModel(Participant participant)
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

		public void Push(GlitchImpostoDirectorV01 persistence)
		{
			hibernationTime = persistence.hibernationTime;
		}

		public GlitchImpostoDirectorV01 Pull()
		{
			return new GlitchImpostoDirectorV01
			{
				hibernationTime = hibernationTime
			};
		}
	}
}
