using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameAchieveV02 : PersistedDataSet
	{
		public Dictionary<AchievementsDirector.GameFloatStat, float> gameFloatStatDict = new Dictionary<AchievementsDirector.GameFloatStat, float>();

		public Dictionary<AchievementsDirector.GameIntStat, int> gameIntStatDict = new Dictionary<AchievementsDirector.GameIntStat, int>();

		public Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>> gameIdDictStatDict = new Dictionary<AchievementsDirector.GameIdDictStat, Dictionary<Identifiable.Id, int>>();

		public override string Identifier => "SRGA";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			gameFloatStatDict = LoadDictionary(reader, (BinaryReader re) => (AchievementsDirector.GameFloatStat)re.ReadInt32(), (BinaryReader re) => re.ReadSingle());
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

		public static GameAchieveV02 Load(BinaryReader reader)
		{
			GameAchieveV02 gameAchieveV = new GameAchieveV02();
			gameAchieveV.Load(reader.BaseStream);
			return gameAchieveV;
		}

		public static void AssertAreEqual(GameAchieveV02 expected, GameAchieveV02 actual)
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
