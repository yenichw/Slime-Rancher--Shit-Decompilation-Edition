using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV10 : GamePersistedDataSet<GameV09>
	{
		public string gameName;

		public string displayName;

		public GameSummaryV04 summary = new GameSummaryV04();

		public WorldV22 world = new WorldV22();

		public PlayerV14 player = new PlayerV14();

		public RanchV07 ranch = new RanchV07();

		public List<ActorDataV09> actors = new List<ActorDataV09>();

		public PediaV03 pedia = new PediaV03();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public HolidayDirectorV02 holiday = new HolidayDirectorV02();

		public override string Identifier => "SRGAME";

		public override uint Version => 10u;

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
			world = WorldV22.Load(reader);
			player = PlayerV14.Load(reader);
			ranch = RanchV07.Load(reader);
			ReadSectionSeparator(reader);
			actors = PersistedDataSet.LoadList<ActorDataV09>(reader);
			ReadSectionSeparator(reader);
			pedia = PediaV03.Load(reader);
			achieve = GameAchieveV03.Load(reader);
			holiday = HolidayDirectorV02.Load(reader);
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
		}

		protected override void UpgradeFrom(GameV09 legacyData)
		{
			gameName = legacyData.gameName;
			displayName = legacyData.displayName;
			summary = legacyData.summary;
			world = legacyData.world;
			achieve = legacyData.achieve;
			ranch = legacyData.ranch;
			pedia = legacyData.pedia;
			holiday = legacyData.holiday;
			actors = legacyData.actors.Select((ActorDataV08 a) => new ActorDataV09(a)).ToList();
			player = new PlayerV14(legacyData.player);
		}

		public static void AssertAreEqual(GameV10 expected, GameV10 actual, bool allowActorMovement = false)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV22.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV14.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV09.AssertAreEqual(expected.actors[i], actual.actors[i], allowActorMovement);
			}
			HolidayDirectorV02.AssertAreEqual(expected.holiday, actual.holiday);
		}

		public static void AssertAreEqual(GameV09 expected, GameV10 actual)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV22.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV14.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			TestUtil.AssertAreEqual(expected.actors, actual.actors, delegate(ActorDataV08 e, ActorDataV09 a, string r)
			{
				ActorDataV09.AssertAreEqual(e, a);
			}, "actors");
			HolidayDirectorV02.AssertAreEqual(expected.holiday, actual.holiday);
		}
	}
}
