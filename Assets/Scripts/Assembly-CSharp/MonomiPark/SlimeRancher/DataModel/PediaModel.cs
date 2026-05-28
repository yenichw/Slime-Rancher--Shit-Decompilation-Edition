using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class PediaModel
	{
		public interface Participant
		{
			void InitModel(PediaModel model);

			void SetModel(PediaModel model);

			void OnUnlockedChanged(HashSet<PediaDirector.Id> unlocked);
		}

		private Participant participant;

		public HashSet<PediaDirector.Id> unlocked = new HashSet<PediaDirector.Id>();

		public int progressGivenForCount;

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

		public void ResetUnlocked(PediaDirector.Id[] initUnlocked)
		{
			unlocked.Clear();
			foreach (PediaDirector.Id id in initUnlocked)
			{
				Unlock(id);
			}
			if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().preventHostiles)
			{
				Unlock(PediaDirector.Id.TARR_SLIME);
			}
		}

		public void Unlock(params PediaDirector.Id[] ids)
		{
			int count = unlocked.Count;
			unlocked.UnionWith(ids);
			if (count != unlocked.Count)
			{
				participant.OnUnlockedChanged(unlocked);
			}
		}

		public void Push(int progressGivenForCount, IEnumerable<PediaDirector.Id> unlocked)
		{
			this.progressGivenForCount = progressGivenForCount;
			foreach (PediaDirector.Id item in unlocked)
			{
				this.unlocked.Add(item);
			}
		}

		public void Pull(out int progressGivenForCount, out IEnumerable<PediaDirector.Id> unlocked)
		{
			progressGivenForCount = this.progressGivenForCount;
			unlocked = new List<PediaDirector.Id>(this.unlocked);
		}
	}
}
