using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GameAchievesModel
	{
		public interface Participant
		{
			void InitModel(GameAchievesModel model);

			void SetModel(GameAchievesModel model);
		}

		public Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();

		public Dictionary<AchievementsDirector.GameDoubleStat, double> gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>();

		public Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();

		public Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();

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
			gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();
			gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>();
			gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();
			gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_SLEPT] = 0.0;
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_AWOKE] = 32400.0;
		}

		public void Push(Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict, Dictionary<AchievementsDirector.GameDoubleStat, double> gameDoubleStatDict, Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict, Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict)
		{
			this.gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>(gameFloatStatDict);
			this.gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>(gameDoubleStatDict);
			if (!gameDoubleStatDict.ContainsKey(AchievementsDirector.GameDoubleStat.LAST_AWOKE))
			{
				gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_AWOKE] = 0.0;
			}
			if (!gameDoubleStatDict.ContainsKey(AchievementsDirector.GameDoubleStat.LAST_SLEPT))
			{
				gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_SLEPT] = 32400.0;
			}
			if (gameIntStatDict != null)
			{
				this.gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>(gameIntStatDict);
			}
			else
			{
				this.gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();
			}
			this.gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();
			if (gameIdDictStatDict == null)
			{
				return;
			}
			foreach (KeyValuePair<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> item in gameIdDictStatDict)
			{
				this.gameIdDictStatDict[item.Key] = new Dictionary<Identifiable.Id, int>(item.Value, Identifiable.idComparer);
			}
		}

		public void Pull(out Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict, out Dictionary<AchievementsDirector.GameDoubleStat, double> gameDoubleStatDict, out Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict, out Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict)
		{
			gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>(this.gameFloatStatDict);
			gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>(this.gameDoubleStatDict);
			gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>(this.gameIntStatDict);
			gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();
			foreach (KeyValuePair<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> item in this.gameIdDictStatDict)
			{
				gameIdDictStatDict[item.Key] = new Dictionary<Identifiable.Id, int>(item.Value, Identifiable.idComparer);
			}
		}
	}
}
