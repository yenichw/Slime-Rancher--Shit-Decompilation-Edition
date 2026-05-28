using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class KookadobaNodeModel : PositionalModel
	{
		public interface Participant
		{
			void InitModel(KookadobaNodeModel model);

			void SetModel(KookadobaNodeModel model);

			void Grow(GameObject kookadoba);
		}

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

		public void Grow(GameObject gameObject)
		{
			part.Grow(gameObject);
		}
	}
}
