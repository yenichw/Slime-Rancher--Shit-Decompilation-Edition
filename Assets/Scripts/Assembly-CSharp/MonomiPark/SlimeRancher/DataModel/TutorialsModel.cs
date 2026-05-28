using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class TutorialsModel
	{
		public interface Participant
		{
			void InitModel(TutorialsModel model);

			void SetModel(TutorialsModel model);
		}

		public HashSet<TutorialDirector.Id> completedIds = new HashSet<TutorialDirector.Id>();

		public HashSet<TutorialDirector.Id> popupQueue = new HashSet<TutorialDirector.Id>();

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

		public void Push(List<TutorialDirector.Id> completedIds, List<TutorialDirector.Id> popupQueue)
		{
			this.completedIds = new HashSet<TutorialDirector.Id>(completedIds);
			this.popupQueue = new HashSet<TutorialDirector.Id>(popupQueue);
		}

		public void Pull(out List<TutorialDirector.Id> completedIds, out List<TutorialDirector.Id> popupQueue)
		{
			completedIds = new List<TutorialDirector.Id>(this.completedIds);
			popupQueue = new List<TutorialDirector.Id>(this.popupQueue);
		}
	}
}
