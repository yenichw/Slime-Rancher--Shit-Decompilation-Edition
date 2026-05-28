using System.Collections.Generic;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GordoModel
	{
		public interface Participant
		{
			void InitModel(GordoModel model);

			void SetModel(GordoModel model);

			void OnResetEatenCount();
		}

		public int gordoEatenCount;

		public List<Identifiable.Id> fashions = new List<Identifiable.Id>();

		private GameObject gameObj;

		internal int targetCount;

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

		public ZoneDirector.Zone GetZoneId()
		{
			return gameObj.GetComponent<GordoEat>().GetZoneId();
		}

		public bool HasPopped()
		{
			return gordoEatenCount == -1;
		}

		public bool DropsKey()
		{
			return gameObj.GetComponent<GordoEat>().DropsKey();
		}

		public void Push(int gordoEatenCount, List<Identifiable.Id> fashions)
		{
			this.gordoEatenCount = gordoEatenCount;
			this.fashions = fashions;
		}

		public void Pull(out int gordoEatenCount, out List<Identifiable.Id> fashions)
		{
			gordoEatenCount = this.gordoEatenCount;
			fashions = this.fashions;
		}

		public void OnNewGameLoaded(PlayerState.GameMode currGameMode)
		{
			GordoNearBurstOnGameMode component = gameObj.GetComponent<GordoNearBurstOnGameMode>();
			if (component != null && component.NearBurstForGameMode(currGameMode))
			{
				gordoEatenCount = Mathf.Min(targetCount, (int)(targetCount - component.remaining));
			}
		}

		public void SetGameObjectActive(bool isActive)
		{
			gameObj.SetActive(isActive);
		}

		public Identifiable.Id GetGordoId()
		{
			return gameObj.GetComponent<GordoIdentifiable>().id;
		}

		public void EventGordoActivate(bool isFirstActivation)
		{
			if (isFirstActivation)
			{
				gordoEatenCount = 0;
				Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OnResetEatenCount();
				}
			}
			SetGameObjectActive(isActive: true);
		}
	}
}
