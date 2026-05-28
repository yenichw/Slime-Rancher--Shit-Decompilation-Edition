using System;
using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV05 : GamePersistedDataSet<GameV04>
	{
		public string gameName;

		public string displayName;

		public GameSummaryV04 summary = new GameSummaryV04();

		public WorldV19 world = new WorldV19();

		public PlayerV13 player = new PlayerV13();

		public RanchV06 ranch = new RanchV06();

		public List<ActorDataV05> actors = new List<ActorDataV05>();

		public PediaV03 pedia = new PediaV03();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public override string Identifier => "SRGAME";

		public override uint Version => 5u;

		protected override void LoadSummaryData(BinaryReader reader)
		{
			gameName = reader.ReadString();
			displayName = reader.ReadString();
			summary = GameSummaryV04.Load(reader);
		}

		protected override void WriteSummaryData(BinaryWriter writer)
		{
			writer.Write(gameName);
			writer.Write(displayName);
			summary.Write(writer.BaseStream);
		}

		protected override void LoadGameData(BinaryReader reader)
		{
			world = WorldV19.Load(reader);
			player = PlayerV13.Load(reader);
			ranch = RanchV06.Load(reader);
			ReadSectionSeparator(reader);
			actors = PersistedDataSet.LoadList<ActorDataV05>(reader);
			ReadSectionSeparator(reader);
			pedia = PediaV03.Load(reader);
			achieve = GameAchieveV03.Load(reader);
		}

		protected override void WriteGameData(BinaryWriter writer)
		{
			world.Write(writer.BaseStream);
			player.Write(writer.BaseStream);
			ranch.Write(writer.BaseStream);
			WriteSectionSeparator(writer);
			PersistedDataSet.WriteList(writer, actors);
			WriteSectionSeparator(writer);
			pedia.Write(writer.BaseStream);
			achieve.Write(writer.BaseStream);
		}

		protected override void UpgradeFrom(GameV04 legacyData)
		{
			gameName = legacyData.gameName;
			displayName = legacyData.gameName;
			world = UpgradeFrom(legacyData.world);
			achieve = legacyData.achieve;
			player = UpgradeFrom(legacyData.player);
			ranch = UpgradeFrom(legacyData.ranch);
			actors = legacyData.actors;
			pedia = UpgradeFrom(legacyData.pedia);
			summary = new GameSummaryV04();
			summary.currency = player.currency;
			summary.gameMode = player.gameMode;
			summary.iconId = player.gameIconId;
			summary.version = player.version;
			summary.worldTime = world.worldTime;
			summary.pediaCount = pedia.unlockedIds.Count;
			summary.saveTimestamp = DateTimeOffset.MinValue;
			summary.saveNumber = 0uL;
		}

		private static WorldV19 UpgradeFrom(WorldV13 legacyData)
		{
			return new WorldV19(new WorldV18(new WorldV17(new WorldV16(new WorldV15(new WorldV14(legacyData))))));
		}

		private static PlayerV13 UpgradeFrom(PlayerV07 legacyData)
		{
			return new PlayerV13(new PlayerV12(new PlayerV11(new PlayerV10(new PlayerV09(new PlayerV08(legacyData))))));
		}

		private static RanchV06 UpgradeFrom(RanchV05 legacyData)
		{
			return new RanchV06(legacyData);
		}

		private static PediaV03 UpgradeFrom(PediaV02 legacyData)
		{
			return new PediaV03(legacyData);
		}

		public static void AssertAreEqual(GameV05 expected, GameV05 actual)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV19.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV06.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
		}

		public static void AssertAreEqual(GameV04 expected, GameV05 actual)
		{
			WorldV19.AssertAreEqual(UpgradeFrom(expected.world), actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(UpgradeFrom(expected.player), actual.player);
			RanchV06.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
		}
	}
}
