using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GadgetSiteModel
	{
		public interface Participant
		{
			void InitModel(GadgetSiteModel model);

			void SetModel(GadgetSiteModel model);

			void SetAttached(GadgetModel gadgetModel);
		}

		public readonly string id;

		public readonly Transform transform;

		public GadgetModel attached;

		private Participant participant;

		public GadgetSiteModel(string id, Transform transform)
		{
			this.id = id;
			this.transform = transform;
		}

		public void SetParticipant(Participant part)
		{
			participant = part;
		}

		public void Init()
		{
			participant.InitModel(this);
		}

		public void NotifyParticipants()
		{
			participant.SetModel(this);
		}

		public bool HasAttached()
		{
			return attached != null;
		}

		public void Detach()
		{
			attached = null;
			participant.SetAttached(null);
		}

		public void Attach(GameObject gameObj, GadgetModel gadgetModel)
		{
			attached = gadgetModel;
			participant.SetAttached(gadgetModel);
		}

		public void Push()
		{
		}

		public void Pull()
		{
		}
	}
}
