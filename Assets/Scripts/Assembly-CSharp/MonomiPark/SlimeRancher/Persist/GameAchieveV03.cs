using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameAchieveV03 : VersionedPersistedDataSet<GameAchieveV02>
	{
		public Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();

		public Dictionary<AchievementsDirector.GameDoubleStat, double> gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>();

		public Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();

		public Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();

		public override string Identifier => "SRGA";

		public override uint Version => 3u;

		protected override void LoadData(BinaryReader reader)
		{
			gameFloatStatDict = LoadDictionary(reader, (BinaryReader re) => (AchievementsDirector.GameFloatStat)re.ReadInt32(), (BinaryReader re) => re.ReadSingle());
			gameDoubleStatDict = LoadDictionary(reader, (BinaryReader re) => (AchievementsDirector.GameDoubleStat)re.ReadInt32(), (BinaryReader re) => re.ReadDouble());
			gameIntStatDict = LoadDictionary(reader, (BinaryReader re) => (AchievementsDirector.GameIntStat)re.ReadInt32(), (BinaryReader re) => re.ReadInt32());
			gameIdDictStatDict = LoadDictionary(reader, (BinaryReader re) => (AchievementsDirector.GameIdDictStat)re.ReadInt32(), (BinaryReader re) => LoadDictionary(re, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32()));
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteDictionary(writer, gameFloatStatDict, delegate(BinaryWriter w, AchievementsDirector.GameFloatStat key)
			{
				w.Write((int)key);
			}, delegate(BinaryWriter w, float val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, gameDoubleStatDict, delegate(BinaryWriter w, AchievementsDirector.GameDoubleStat key)
			{
				w.Write((int)key);
			}, delegate(BinaryWriter w, double val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, gameIntStatDict, delegate(BinaryWriter w, AchievementsDirector.GameIntStat key)
			{
				w.Write((int)key);
			}, delegate(BinaryWriter w, int val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, gameIdDictStatDict, delegate(BinaryWriter w, AchievementsDirector.GameIdDictStat key)
			{
				w.Write((int)key);
			}, delegate(BinaryWriter w, Dictionary<Identifiable.Id, int> val)
			{
				WriteDictionary(w, val, delegate(BinaryWriter wr, Identifiable.Id key)
				{
					wr.Write((int)key);
				}, delegate(BinaryWriter wr, int v)
				{
					wr.Write(v);
				});
			});
		}

		public static GameAchieveV03 Load(BinaryReader reader)
		{
			GameAchieveV03 gameAchieveV = new GameAchieveV03();
			gameAchieveV.Load(reader.BaseStream);
			return gameAchieveV;
		}

		protected override void UpgradeFrom(GameAchieveV02 legacyData)
		{
			gameFloatStatDict = legacyData.gameFloatStatDict;
			gameIntStatDict = legacyData.gameIntStatDict;
			gameIdDictStatDict = legacyData.gameIdDictStatDict;
			gameDoubleStatDict = new Dictionary<AchievementsDirector.GameDoubleStat, double>();
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_LEFT_RANCH] = gameFloatStatDict.Get((AchievementsDirector.GameFloatStat)0);
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_ENTERED_RANCH] = gameFloatStatDict.Get((AchievementsDirector.GameFloatStat)1);
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_SLEPT] = gameFloatStatDict.Get((AchievementsDirector.GameFloatStat)2);
			gameDoubleStatDict[AchievementsDirector.GameDoubleStat.LAST_AWOKE] = gameFloatStatDict.Get((AchievementsDirector.GameFloatStat)3);
		}

		public static void AssertAreEqual(GameAchieveV03 expected, GameAchieveV03 actual)
		{
			TestUtil.AssertAreEqual(expected.gameFloatStatDict, actual.gameFloatStatDict, delegate
			{
			}, "gameFloatStatDict");
			TestUtil.AssertAreEqual(expected.gameDoubleStatDict, actual.gameDoubleStatDict, delegate
			{
			}, "gameDoubleStatDict");
			TestUtil.AssertAreEqual(expected.gameIntStatDict, actual.gameIntStatDict, delegate
			{
			}, "gameIntStatDict");
			TestUtil.AssertAreEqual(expected.gameIdDictStatDict, actual.gameIdDictStatDict, delegate(Dictionary<Identifiable.Id, int> e, Dictionary<Identifiable.Id, int> a)
			{
				TestUtil.AssertAreEqual(e, a, delegate
				{
				}, e.ToString());
			}, "gameIdDictStatDict");
		}

		public static void AssertAreEqual(GameAchieveV02 expected, GameAchieveV03 actual)
		{
			TestUtil.AssertAreEqual(expected.gameFloatStatDict, actual.gameFloatStatDict, delegate
			{
			}, "gameFloatStatDict");
			TestUtil.AssertAreEqual(expected.gameIntStatDict, actual.gameIntStatDict, delegate
			{
			}, "gameIntStatDict");
			TestUtil.AssertAreEqual(expected.gameIdDictStatDict, actual.gameIdDictStatDict, delegate(Dictionary<Identifiable.Id, int> e, Dictionary<Identifiable.Id, int> a)
			{
				TestUtil.AssertAreEqual(e, a, delegate
				{
				}, e.ToString());
			}, "gameIdDictStatDict");
		}
	}
}
