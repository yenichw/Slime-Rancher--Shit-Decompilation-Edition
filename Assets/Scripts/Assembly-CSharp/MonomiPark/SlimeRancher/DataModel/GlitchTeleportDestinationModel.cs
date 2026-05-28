using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchTeleportDestinationModel
	{
		public interface Participant
		{
			string id { get; }

			void InitModel(GlitchTeleportDestinationModel model);

			void SetModel(GlitchTeleportDestinationModel model);
		}

		public double? activationTime;

		private readonly Participant participant;

		public GlitchTeleportDestinationModel(Participant participant)
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

		public void Push(GlitchTeleportDestinationV01 persistence)
		{
			activationTime = persistence.activationTime;
		}

		public GlitchTeleportDestinationV01 Pull()
		{
			return new GlitchTeleportDestinationV01
			{
				activationTime = activationTime
			};
		}
	}
}
