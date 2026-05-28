using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class AccessDoorModel
	{
		public interface Participant
		{
			void InitModel(AccessDoorModel model);

			void SetModel(AccessDoorModel model);
		}

		public AccessDoor.State state;

		public ProgressDirector.ProgressType[] progress;

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

		public void Push(AccessDoor.State state)
		{
			this.state = state;
		}

		public void Pull(out AccessDoor.State state)
		{
			state = this.state;
		}

		public bool IsUnlockedForGameMode(PlayerState.GameMode currGameMode)
		{
			UnlockedOnGameMode component = gameObj.GetComponent<UnlockedOnGameMode>();
			if (component != null)
			{
				return component.IsUnlockedFor(currGameMode);
			}
			return false;
		}
	}
}
