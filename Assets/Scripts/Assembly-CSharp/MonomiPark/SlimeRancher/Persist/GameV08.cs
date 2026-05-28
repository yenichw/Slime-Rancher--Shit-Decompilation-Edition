using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV08 : GamePersistedDataSet<GameV07>
	{
		public string gameName;

		public string displayName;

		public GameSummaryV04 summary = new GameSummaryV04();

		public WorldV19 world = new WorldV19();

		public PlayerV13 player = new PlayerV13();

		public RanchV07 ranch = new RanchV07();

		public List<ActorDataV05> actors = new List<ActorDataV05>();

		public PediaV03 pedia = new PediaV03();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public HolidayDirectorV01 holiday = new HolidayDirectorV01();

		public DLCV01 dlc = new DLCV01();

		public override string Identifier => "SRGAME";

		public override uint Version => 8u;

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
			ranch = RanchV07.Load(reader);
			ReadSectionSeparator(reader);
			actors = PersistedDataSet.LoadList<ActorDataV05>(reader);
			ReadSectionSeparator(reader);
			pedia = PediaV03.Load(reader);
			achieve = GameAchieveV03.Load(reader);
			holiday = HolidayDirectorV01.Load(reader);
			dlc = DLCV01.Load(reader);
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
			holiday.Write(writer.BaseStream);
			dlc.Write(writer.BaseStream);
		}

		protected override void UpgradeFrom(GameV07 legacyData)
		{
			gameName = legacyData.gameName;
			displayName = legacyData.displayName;
			summary = legacyData.summary;
			world = legacyData.world;
			achieve = legacyData.achieve;
			player = legacyData.player;
			ranch = new RanchV07(legacyData.ranch);
			actors = legacyData.actors;
			pedia = legacyData.pedia;
			holiday = legacyData.holiday;
			dlc = new DLCV01();
		}

		public static void AssertAreEqual(GameV08 expected, GameV08 actual)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV19.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
			HolidayDirectorV01.AssertAreEqual(expected.holiday, actual.holiday);
			DLCV01.AssertAreEqual(expected.dlc, actual.dlc);
		}

		public static void AssertAreEqual(GameV07 expected, GameV08 actual)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV19.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
			HolidayDirectorV01.AssertAreEqual(expected.holiday, actual.holiday);
			DLCV01.AssertAreEqual(new DLCV01(), actual.dlc);
		}
	}
}
