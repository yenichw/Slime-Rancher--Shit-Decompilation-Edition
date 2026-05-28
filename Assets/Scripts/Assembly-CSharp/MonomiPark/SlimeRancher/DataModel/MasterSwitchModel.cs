using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class MasterSwitchModel
	{
		public interface Participant
		{
			void InitModel(MasterSwitchModel model);

			void SetModel(MasterSwitchModel model);
		}

		public SwitchHandler.State state;

		private GameObject gameObj;

		public void SetGameObject(GameObject gameObj)
		{
			this.gameObj = gameObj;
		}

		public void Init()
		{
			Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetModel(this);
			}
		}

		public void Push(SwitchHandler.State state)
		{
			this.state = state;
		}

		public void Pull(out SwitchHandler.State state)
		{
			state = this.state;
		}
	}
}
