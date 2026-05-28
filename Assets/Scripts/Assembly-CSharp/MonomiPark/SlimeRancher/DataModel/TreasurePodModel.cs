using System.Collections.Generic;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class TreasurePodModel
	{
		public interface Participant
		{
			void InitModel(TreasurePodModel model);

			void SetModel(TreasurePodModel model);
		}

		public TreasurePod.State state;

		public Queue<Identifiable.Id> spawnQueue = new Queue<Identifiable.Id>();

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

		public ZoneDirector.Zone GetZoneId()
		{
			return gameObj.GetComponent<TreasurePod>().GetZoneId();
		}

		public void Push(TreasurePod.State state, List<Identifiable.Id> spawnQueue)
		{
			this.state = state;
			this.spawnQueue = new Queue<Identifiable.Id>();
			for (int i = 0; i < spawnQueue.Count; i++)
			{
				this.spawnQueue.Enqueue(spawnQueue[i]);
			}
		}

		public void Pull(out TreasurePod.State state, out List<Identifiable.Id> spawnQueue)
		{
			state = this.state;
			spawnQueue = new List<Identifiable.Id>();
			if (this.spawnQueue.Count <= 0)
			{
				return;
			}
			foreach (Identifiable.Id item in new List<Identifiable.Id>(this.spawnQueue))
			{
				if (item != 0)
				{
					spawnQueue.Add(item);
				}
			}
		}
	}
}
