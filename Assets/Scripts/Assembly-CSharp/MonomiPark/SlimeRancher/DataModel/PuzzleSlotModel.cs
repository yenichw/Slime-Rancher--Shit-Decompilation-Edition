using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class PuzzleSlotModel
	{
		public interface Participant
		{
			void InitModel(PuzzleSlotModel model);

			void SetModel(PuzzleSlotModel model);

			void OnFilledChangedFromModel();
		}

		public bool filled;

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

		public void Push(bool filled)
		{
			this.filled = filled;
		}

		public void Pull(out bool filled)
		{
			filled = this.filled;
		}

		public void OnNewGameLoaded(PlayerState.GameMode currGameMode)
		{
			UnlockedOnGameMode component = gameObj.GetComponent<UnlockedOnGameMode>();
			if (component != null && component.IsUnlockedFor(currGameMode))
			{
				filled = true;
				Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OnFilledChangedFromModel();
				}
			}
		}
	}
}
