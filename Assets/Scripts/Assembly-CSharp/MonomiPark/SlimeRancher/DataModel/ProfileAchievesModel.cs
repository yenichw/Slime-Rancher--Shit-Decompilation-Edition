using System;
using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class ProfileAchievesModel
	{
		public interface Participant
		{
			void InitModel(ProfileAchievesModel model);

			void SetModel(ProfileAchievesModel model);
		}

		public Dictionary<AchievementsDirector.BoolStat, bool> boolStatDict = new Dictionary<AchievementsDirector.BoolStat, bool>();

		public Dictionary<AchievementsDirector.IntStat, int> intStatDict = new Dictionary<AchievementsDirector.IntStat, int>();

		public Dictionary<AchievementsDirector.EnumStat, HashSet<Enum>> enumStatDict = new Dictionary<AchievementsDirector.EnumStat, HashSet<Enum>>();

		public HashSet<AchievementsDirector.Achievement> earnedAchievements = new HashSet<AchievementsDirector.Achievement>();

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

		public void Reset()
		{
			boolStatDict = new Dictionary<AchievementsDirector.BoolStat, bool>();
			intStatDict = new Dictionary<AchievementsDirector.IntStat, int>();
			enumStatDict = new Dictionary<AchievementsDirector.EnumStat, HashSet<Enum>>();
			earnedAchievements = new HashSet<AchievementsDirector.Achievement>();
		}

		public void Push(Dictionary<AchievementsDirector.BoolStat, bool> boolStatDict, Dictionary<AchievementsDirector.IntStat, int> intStatDict, Dictionary<AchievementsDirector.EnumStat, List<Enum>> enumStatDict, List<AchievementsDirector.Achievement> earnedAchievements)
		{
			this.boolStatDict = new Dictionary<AchievementsDirector.BoolStat, bool>(boolStatDict);
			this.intStatDict = new Dictionary<AchievementsDirector.IntStat, int>(intStatDict);
			this.enumStatDict = new Dictionary<AchievementsDirector.EnumStat, HashSet<Enum>>();
			foreach (KeyValuePair<AchievementsDirector.EnumStat, List<Enum>> item in enumStatDict)
			{
				this.enumStatDict[item.Key] = new HashSet<Enum>(item.Value);
			}
			this.earnedAchievements = new HashSet<AchievementsDirector.Achievement>(earnedAchievements);
			AchievementsDirector.SyncAchievements(this);
		}

		public void Pull(out Dictionary<AchievementsDirector.BoolStat, bool> boolStatDict, out Dictionary<AchievementsDirector.IntStat, int> intStatDict, out Dictionary<AchievementsDirector.EnumStat, List<Enum>> enumStatDict, out List<AchievementsDirector.Achievement> earnedAchievements)
		{
			boolStatDict = new Dictionary<AchievementsDirector.BoolStat, bool>(this.boolStatDict);
			intStatDict = new Dictionary<AchievementsDirector.IntStat, int>(this.intStatDict);
			enumStatDict = new Dictionary<AchievementsDirector.EnumStat, List<Enum>>();
			foreach (KeyValuePair<AchievementsDirector.EnumStat, HashSet<Enum>> item in this.enumStatDict)
			{
				enumStatDict[item.Key] = new List<Enum>(item.Value);
			}
			earnedAchievements = new List<AchievementsDirector.Achievement>(this.earnedAchievements);
		}
	}
}
