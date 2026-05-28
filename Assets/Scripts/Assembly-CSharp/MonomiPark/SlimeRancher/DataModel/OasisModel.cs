using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class OasisModel
	{
		public interface Participant
		{
			void InitModel(OasisModel model);

			void SetModel(OasisModel model);
		}

		public bool isLive;

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

		public void Push(bool isLive)
		{
			this.isLive = isLive;
		}

		public void Pull(out bool isLive)
		{
			isLive = this.isLive;
		}
	}
}
