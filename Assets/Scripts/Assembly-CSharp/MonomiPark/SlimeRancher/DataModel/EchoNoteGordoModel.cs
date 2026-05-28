using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class EchoNoteGordoModel
	{
		public interface Participant
		{
			void InitModel(EchoNoteGordoModel model);

			void SetModel(EchoNoteGordoModel model);
		}

		public enum State
		{
			NOT_POPPED = 0,
			POPPING_1 = 1,
			POPPING_2 = 2,
			POPPED = 3
		}

		public State state;

		private readonly GameObject gameObject;

		public EchoNoteGordoModel(GameObject gameObject)
		{
			this.gameObject = gameObject;
		}

		public void Init()
		{
			Participant[] componentsInChildren = gameObject.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			Participant[] componentsInChildren = gameObject.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetModel(this);
			}
		}

		public void Activate(bool isFirstActivation)
		{
			state = ((!isFirstActivation) ? state : State.NOT_POPPED);
			gameObject.SetActive(value: true);
		}

		public void Push(EchoNoteGordoV01 persistence)
		{
			state = persistence.state;
		}

		public EchoNoteGordoV01 Pull()
		{
			return new EchoNoteGordoV01
			{
				state = state
			};
		}
	}
}
