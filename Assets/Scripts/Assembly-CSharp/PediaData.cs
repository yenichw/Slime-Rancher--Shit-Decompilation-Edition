using System;
using System.Collections.Generic;

[Serializable]
public class PediaData : DataModule<PediaData>
{
	public const int CURR_FORMAT_ID = 1;

	public List<string> unlockedIds = new List<string>();

	public List<string> completedTuts = new List<string>();

	public int progressGivenForPediaCount;

	public static void AssertEquals(PediaData dataA, PediaData dataB)
	{
	}
}
