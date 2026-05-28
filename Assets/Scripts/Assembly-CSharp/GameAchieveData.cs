using System;
using System.Collections.Generic;

[Serializable]
public class GameAchieveData : DataModule<GameAchieveData>
{
	public const int CURR_FORMAT_ID = 1;

	public Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();

	public Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();

	public Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();

	public static void AssertEquals(GameAchieveData dataA, GameAchieveData dataB)
	{
		foreach (AchievementsDirector.GameIdDictStat key in dataA.gameIdDictStatDict.Keys)
		{
			_ = key;
		}
	}
}
