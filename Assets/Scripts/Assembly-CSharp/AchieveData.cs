using System;
using System.Collections.Generic;

[Serializable]
public class AchieveData : DataModule<AchieveData>
{
	public const int CURR_FORMAT_ID = 2;

	public Dictionary<AchievementsDirector.BoolStat, bool> boolStatDict = new Dictionary<AchievementsDirector.BoolStat, bool>();

	public Dictionary<AchievementsDirector.IntStat, int> intStatDict = new Dictionary<AchievementsDirector.IntStat, int>();

	public Dictionary<AchievementsDirector.EnumStat, List<Enum>> enumStatDict = new Dictionary<AchievementsDirector.EnumStat, List<Enum>>();

	public List<AchievementsDirector.Achievement> earnedAchievements = new List<AchievementsDirector.Achievement>();
}
